using System;
using System.Collections.Generic;
using UnityEngine;

public class DropLootController : MonoBehaviour
{
    public static DropLootController Instance { get; private set; }

    [Header("World References")]
    public WorldGenerator worldGenerator;
    public Transform droppedItemsParent;

    private WorldStateData currentWorldState;

    private Dictionary<GameObject, string> trackedLoot =
        new Dictionary<GameObject, string>();

    private bool lootRestored;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("DropLootController: Another instance already exists.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitializeReferences();
    }

    private void InitializeReferences()
    {
        if (worldGenerator == null)
            worldGenerator = FindObjectOfType<WorldGenerator>();

        if (worldGenerator == null)
        {
            Debug.LogError("DropLootController: WorldGenerator is missing.");
            return;
        }

        if (worldGenerator.overWorld == null)
        {
            Debug.LogError("DropLootController: OverWorld reference is missing.");
            return;
        }

        if (droppedItemsParent == null)
        {
            droppedItemsParent =
                worldGenerator.overWorld.Find("DroppedItems");
        }

        if (droppedItemsParent == null)
        {
            GameObject droppedItemsObject =
                new GameObject("DroppedItems");

            droppedItemsObject.transform.SetParent(
                worldGenerator.overWorld
            );

            droppedItemsParent = droppedItemsObject.transform;
        }
    }

    // INITIALIZE WORLD LOOT
    public void InitializeWorldLoot(WorldStateData stateData)
    {
        if (stateData == null)
        {
            Debug.LogError("DropLootController: World state is missing.");
            return;
        }

        currentWorldState = stateData;

        if (currentWorldState.droppedLoot == null)
        {
            currentWorldState.droppedLoot =
                new List<DroppedLootSaveData>();
        }

        if (lootRestored)
            return;

        RestoreWorldLoot();

        lootRestored = true;
    }

    // SPAWN LOOT
    public GameObject SpawnLoot(
        GameObject itemPrefab,
        int amount,
        Vector3 position)
    {
        if (itemPrefab == null)
        {
            Debug.LogWarning("DropLootController: Item prefab is missing.");
            return null;
        }

        if (amount <= 0)
            return null;

        if (droppedItemsParent == null)
        {
            Debug.LogError("DropLootController: DroppedItems parent is not initialized.");
            return null;
        }

        Item item = itemPrefab.GetComponent<Item>();

        if (item == null)
        {
            Debug.LogWarning(
                $"DropLootController: {itemPrefab.name} does not have an Item component."
            );

            return null;
        }

        GameObject droppedItem = item.CloneItem(amount);

        if (droppedItem == null)
            return null;

        droppedItem.transform.position = position;
        droppedItem.transform.SetParent(droppedItemsParent, true);

        LootDropAnimation lootAnimation =
            droppedItem.GetComponent<LootDropAnimation>();

        if (lootAnimation == null)
        {
            lootAnimation =
                droppedItem.AddComponent<LootDropAnimation>();
        }

        lootAnimation.Play();

        RegisterLoot(droppedItem);

        return droppedItem;
    }

    // REGISTER LOOT
    private void RegisterLoot(GameObject droppedItem)
    {
        if (currentWorldState == null)
        {
            Debug.LogWarning(
                "DropLootController: World state is not initialized. Loot cannot be saved."
            );

            return;
        }

        Item item = droppedItem.GetComponent<Item>();

        if (item == null)
            return;

        string lootID = Guid.NewGuid().ToString();

        Vector3 position = droppedItem.transform.position;
        Vector2Int chunk = GetLootChunk(position);

        DroppedLootSaveData data = new DroppedLootSaveData
        {
            lootID = lootID,

            itemID = item.ID,
            quantity = item.quantity,

            hasDurability = item.hasDurability,
            currentDurability = item.CurrentDurability,

            positionX = position.x,
            positionY = position.y,
            positionZ = position.z,

            chunkX = chunk.x,
            chunkY = chunk.y
        };

        currentWorldState.droppedLoot.Add(data);

        trackedLoot[droppedItem] = lootID;
    }

    // RESTORE WORLD LOOT
    private void RestoreWorldLoot()
    {
        if (worldGenerator == null ||
            worldGenerator.itemDictionary == null)
        {
            Debug.LogError(
                "DropLootController: ItemDictionary is missing."
            );

            return;
        }

        foreach (DroppedLootSaveData data in currentWorldState.droppedLoot)
        {
            if (data == null || string.IsNullOrEmpty(data.lootID))
                continue;

            GameObject itemPrefab =
                worldGenerator.itemDictionary.GetItemPrefab(data.itemID);

            if (itemPrefab == null)
                continue;

            Item prefabItem = itemPrefab.GetComponent<Item>();

            if (prefabItem == null)
                continue;

            GameObject droppedItem =
                prefabItem.CloneItem(data.quantity);

            if (droppedItem == null)
                continue;

            Vector3 position = new Vector3(
                data.positionX,
                data.positionY,
                data.positionZ
            );

            droppedItem.transform.position = position;
            droppedItem.transform.SetParent(
                droppedItemsParent,
                true
            );

            Item restoredItem = droppedItem.GetComponent<Item>();

            if (restoredItem != null && data.hasDurability)
            {
                restoredItem.SetCurrentDurability(
                    data.currentDurability
                );
            }

            trackedLoot[droppedItem] = data.lootID;
        }

        Debug.Log(
            "Restored dropped loot: " +
            trackedLoot.Count
        );
    }

    // UPDATE LOOT SAVE DATA
    public void UpdateLoot(GameObject droppedItem)
    {
        if (droppedItem == null ||
            currentWorldState == null)
        {
            return;
        }

        if (!trackedLoot.TryGetValue(
            droppedItem,
            out string lootID))
        {
            return;
        }

        Item item = droppedItem.GetComponent<Item>();

        if (item == null)
            return;

        DroppedLootSaveData data =
            currentWorldState.droppedLoot.Find(
                loot => loot.lootID == lootID
            );

        if (data == null)
            return;

        Vector3 position = droppedItem.transform.position;
        Vector2Int chunk = GetLootChunk(position);

        data.quantity = item.quantity;

        data.hasDurability = item.hasDurability;
        data.currentDurability = item.CurrentDurability;

        data.positionX = position.x;
        data.positionY = position.y;
        data.positionZ = position.z;

        data.chunkX = chunk.x;
        data.chunkY = chunk.y;
    }

    // REMOVE COLLECTED LOOT
    public void RemoveLoot(GameObject droppedItem)
    {
        if (droppedItem == null ||
            currentWorldState == null)
        {
            return;
        }

        if (!trackedLoot.TryGetValue(
            droppedItem,
            out string lootID))
        {
            return;
        }

        currentWorldState.droppedLoot.RemoveAll(
            loot => loot.lootID == lootID
        );

        trackedLoot.Remove(droppedItem);
    }

    // GET LOOT CHUNK
    public Vector2Int GetLootChunk(Vector3 worldPosition)
    {
        if (worldGenerator == null)
        {
            Debug.LogError("DropLootController: WorldGenerator is missing.");
            return Vector2Int.zero;
        }

        return worldGenerator.GetChunkCoordinate(worldPosition);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}