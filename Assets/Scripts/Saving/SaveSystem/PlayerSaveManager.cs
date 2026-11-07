using System.Collections;
using System.IO;
using UnityEngine;

public class PlayerSaveManager : MonoBehaviour
{
    [Header("Player References")]
    public Transform player;
    public HealthController healthController;
    public HungerController hungerController;

    [Header("Inventory References")]
    public InventoryController inventoryController;
    public HotbarController hotbarController;
    public ItemDictionary itemDictionary;

    [Header("World References")]
    public WorldGenerator worldGenerator;
    
    private PlayerSaveData loadedData;
    private string savePath;

    private const int SAVE_VERSION = 1;

    // INITIALIZE AND LOAD POSITION
    private void Awake()
    {
        if (SelectedWorldData.world == null)
        {
            Debug.LogWarning(
                "PlayerSaveManager: No selected world. Loading skipped."
            );
            return;
        }

        string worldID = SelectedWorldData.world.worldID;

        savePath = Path.Combine(
            WorldSaveManager.GetWorldDirectory(worldID),
            "player.json"
        );

        if (itemDictionary == null)
        {
            itemDictionary = FindFirstObjectByType<ItemDictionary>();
        }

        loadedData = LoadPlayerData(worldID);

        // RESTORE POSITION BEFORE WORLD GENERATION
        if (loadedData != null && player != null)
        {
            player.position = loadedData.playerPosition;

            Debug.Log("PLAYER POSITION RESTORED.");
        }
    }

    // RESTORE PLAYER DATA AFTER INITIALIZATION
    private IEnumerator Start()
    {
        if (loadedData == null)
            yield break;

        // WAIT FOR INVENTORY START INITIALIZATION
        yield return null;

        RestoreInventory();
        RestoreHotbar();

        // RESTORE PLAYER STATS
        if (healthController != null)
        {
            healthController.RestoreSavedHealth(
                loadedData.currentHealth
            );
        }

        if (hungerController != null)
        {
            hungerController.RestoreSavedHunger(
                loadedData.currentHunger,
                loadedData.currentSaturation
            );
        }

        Debug.Log("PLAYER DATA RESTORED SUCCESSFULLY.");
    }

    // SAVE PLAYER DATA
    public bool SavePlayer()
    {
        if (SelectedWorldData.world == null)
        {
            Debug.LogWarning("Cannot save: No selected world.");
            return false;
        }

        if (player == null)
        {
            Debug.LogError("Cannot save: Player reference is missing.");
            return false;
        }

        string worldID = SelectedWorldData.world.worldID;

        PlayerSaveData data = new PlayerSaveData();

        data.worldID = worldID;
        data.saveVersion = SAVE_VERSION;

        // PLAYER POSITION
        data.playerPosition = player.position;

        // PLAYER STATS
        if (healthController != null)
            data.currentHealth = healthController.currentHealth;

        if (hungerController != null)
        {
            data.currentHunger = hungerController.currentHunger;
            data.currentSaturation = hungerController.currentSaturation;
        }

        // INVENTORY
        SaveSlots(
            inventoryController.inventoryPanel.transform,
            data.inventoryItems
        );

        // HOTBAR
        SaveSlots(
            hotbarController.hotbarPanel.transform,
            data.hotbarItems
        );

        // SELECTED HOTBAR SLOT
        data.selectedHotbarSlot = hotbarController.SelectedSlot;

        bool success = SaveFileHandler.SaveToJson(savePath, data);

        if (success)
        {
            Debug.Log("PLAYER DATA SAVED: " + worldID);
        }

        return success;
    }

    // SAVE ITEMS FROM SLOTS
    private void SaveSlots(
        Transform panel,
        System.Collections.Generic.List<ItemSlotSaveData> savedItems
    )
    {
        if (panel == null)
            return;

        for (int i = 0; i < panel.childCount; i++)
        {
            Slot slot = panel.GetChild(i).GetComponent<Slot>();

            if (slot == null || slot.currentItem == null)
                continue;

            Item item = slot.currentItem.GetComponent<Item>();

            if (item == null)
                continue;

            ItemSlotSaveData savedItem = new ItemSlotSaveData();

            savedItem.slotIndex = i;
            savedItem.itemID = item.ID;
            savedItem.quantity = item.quantity;

            savedItem.durability = item.hasDurability
                ? item.CurrentDurability
                : -1;

            savedItems.Add(savedItem);
        }
    }

    // LOAD PLAYER DATA
    private PlayerSaveData LoadPlayerData(string worldID)
    {
        if (!SaveFileHandler.SaveExists(savePath))
        {
            Debug.Log("No player save found. Starting new player data.");
            return null;
        }

        PlayerSaveData data =
            SaveFileHandler.LoadFromJson<PlayerSaveData>(savePath);

        if (IsValidSave(data, worldID))
            return data;

        Debug.LogWarning(
            "Player save is invalid. Attempting backup recovery."
        );

        bool recovered =
            SaveBackupHandler.RestoreLatestBackup<PlayerSaveData>(
                savePath,
                backup => IsValidSave(backup, worldID)
            );

        if (!recovered)
        {
            Debug.LogError("Player save recovery failed.");
            return null;
        }

        data = SaveFileHandler.LoadFromJson<PlayerSaveData>(savePath);

        if (IsValidSave(data, worldID))
        {
            Debug.Log("Player save recovered from backup.");
            return data;
        }

        return null;
    }

    // VALIDATE PLAYER SAVE
    private bool IsValidSave(PlayerSaveData data, string worldID)
    {
        return data != null
            && data.worldID == worldID
            && data.saveVersion == SAVE_VERSION
            && data.inventoryItems != null
            && data.hotbarItems != null;
    }

    // RESTORE INVENTORY
    private void RestoreInventory()
    {
        if (inventoryController == null ||
            inventoryController.inventoryPanel == null)
            return;

        RestoreSlots(
            inventoryController.inventoryPanel.transform,
            loadedData.inventoryItems
        );
    }

    // RESTORE HOTBAR
    private void RestoreHotbar()
    {
        if (hotbarController == null ||
            hotbarController.hotbarPanel == null)
            return;

        RestoreSlots(
            hotbarController.hotbarPanel.transform,
            loadedData.hotbarItems
        );

        hotbarController.RestoreSelectedSlot(
            loadedData.selectedHotbarSlot
        );
    }

    // RESTORE ITEMS INTO THEIR ORIGINAL SLOTS
    private void RestoreSlots(
        Transform panel,
        System.Collections.Generic.List<ItemSlotSaveData> savedItems
    )
    {
        if (panel == null || itemDictionary == null)
            return;

        foreach (ItemSlotSaveData savedItem in savedItems)
        {
            if (savedItem.slotIndex < 0 ||
                savedItem.slotIndex >= panel.childCount)
                continue;

            Transform slotTransform =
                panel.GetChild(savedItem.slotIndex);

            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot == null)
                continue;

            GameObject prefab =
                itemDictionary.GetItemPrefab(savedItem.itemID);

            if (prefab == null)
                continue;

            GameObject restoredItem = Instantiate(
                prefab,
                slotTransform
            );

            restoredItem.transform.localPosition = Vector3.zero;
            restoredItem.transform.localRotation = Quaternion.identity;
            restoredItem.transform.localScale = Vector3.one;

            Item item = restoredItem.GetComponent<Item>();

            if (item == null)
            {
                Destroy(restoredItem);
                continue;
            }

            // RESTORE QUANTITY
            item.quantity = item.stackable
                ? Mathf.Clamp(savedItem.quantity, 1, item.maxStackSize)
                : 1;

            item.UpdateQuantityDisplay();

            // RESTORE DURABILITY
            if (item.hasDurability && savedItem.durability >= 0)
            {
                item.SetCurrentDurability(savedItem.durability);
            }

            slot.currentItem = restoredItem;
        }
    }

    // SAVE AND RETURN TO MAIN MENU
    public void ExitToMainMenu()
    {
        bool playerSaved = SavePlayer();

        bool worldSaved = worldGenerator != null &&
                        worldGenerator.SaveCurrentWorldState();

        if (!playerSaved || !worldSaved)
        {
            Debug.LogError("Save failed. Staying in game.");
            return;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
    }
}