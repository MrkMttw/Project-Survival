using UnityEngine;
using UnityEngine.InputSystem;

public class WrenchFunction : MonoBehaviour
{
    [Header("Item Function")]
    [SerializeField] private Item wrenchItemPrefab;

    [SerializeField] private GameObject instructionUI;

    private PlacementController placementController;
    private PlayerHeldItem playerHeldItem;

    private void Awake()
    {
        placementController = FindObjectOfType<PlacementController>();
        playerHeldItem = FindObjectOfType<PlayerHeldItem>();

        if (instructionUI != null)
            instructionUI.SetActive(false);
    }

    public void UseTool()
    {
        Debug.Log("Wrench function activated!");
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (playerHeldItem == null)
            return;

        Item heldItem = playerHeldItem.GetHeldItem();

        // No item equipped
        if (heldItem == null)
        {
            if (instructionUI != null)
                instructionUI.SetActive(false);

            return;
        }

        // No wrench prefab assigned
        if (wrenchItemPrefab == null)
        {
            Debug.LogWarning(
                "WrenchFunction: Wrench Item Prefab is not assigned!"
            );

            if (instructionUI != null)
                instructionUI.SetActive(false);

            return;
        }

        // Check if the equipped item is the wrench
        if (heldItem.ID != wrenchItemPrefab.ID)
        {
            if (instructionUI != null)
                instructionUI.SetActive(false);

            return;
        }

        BuildingObject building = GetBuilding();

        if (building != null)
        {
            if (instructionUI != null)
                instructionUI.SetActive(true);

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryRelocateBuilding();
            }

            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                TryRetrieveBuilding();
            }
        }
        else
        {
            if (instructionUI != null)
                instructionUI.SetActive(false);
        }
    }

    private Collider2D GetBuildingHit()
    {
        if (Mouse.current == null || Camera.main == null)
            return null;

        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(
            Mouse.current.position.ReadValue()
        );

        mouseWorldPosition.z = 0f;

        return Physics2D.OverlapPoint(mouseWorldPosition);
    }

    private BuildingObject GetBuilding()
    {
        Collider2D hit = GetBuildingHit();

        if (hit == null)
            return null;

        return hit.GetComponentInParent<BuildingObject>();
    }

    private void TryRelocateBuilding()
    {
        BuildingObject building = GetBuilding();

        if (building == null)
            return;

        Debug.Log(
            "Relocating building: " +
            building.gameObject.name
        );

        if (placementController == null)
        {
            Debug.LogError(
                "PlacementController not found."
            );

            return;
        }

        placementController.StartRelocation(building);
    }

    private void TryRetrieveBuilding()
    {
        BuildingObject building = GetBuilding();

        if (building == null)
            return;

        if (building.itemID <= 0)
        {
            Debug.LogWarning(
                "Building has no Item ID: " +
                building.gameObject.name
            );

            return;
        }

        ItemDictionary itemDictionary =
            FindObjectOfType<ItemDictionary>();

        if (itemDictionary == null)
        {
            Debug.LogError(
                "ItemDictionary not found."
            );

            return;
        }

        GameObject itemPrefab =
            itemDictionary.GetItemPrefab(building.itemID);

        if (itemPrefab == null)
        {
            Debug.LogError(
                "No item prefab found for Item ID: " +
                building.itemID
            );

            return;
        }

        HotbarController hotbar =
            FindObjectOfType<HotbarController>();

        InventoryController inventory =
            InventoryController.instance;

        bool added = false;

        // Hotbar first
        if (hotbar != null)
        {
            added = hotbar.AddItem(itemPrefab);
        }

        // Inventory second
        if (!added && inventory != null)
        {
            added = inventory.AddItem(itemPrefab);
        }

        if (!added)
        {
            Debug.Log(
                "Cannot retrieve building. " +
                "Hotbar and inventory are full."
            );

            return;
        }

        Destroy(building.gameObject);

        Debug.Log(
            "Building retrieved: " +
            itemPrefab.name
        );
    }
}