using UnityEngine;
using UnityEngine.InputSystem;

public class WrenchFunction : MonoBehaviour
{
    private PlacementController placementController;

    private void Awake()
    {
        placementController = FindObjectOfType<PlacementController>();
    }

    private void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryRelocateBuilding();
        }

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            TryRetrieveBuilding();
        }
    }

    private Collider2D GetBuildingHit()
    {
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

        Debug.Log("Relocating building: " + building.gameObject.name);

        placementController.StartRelocation(building);
    }

    private void TryRetrieveBuilding()
    {
        BuildingObject building = GetBuilding();

        if (building == null)
            return;

        if (building.itemID <= 0)
        {
            Debug.LogWarning("Building has no Item ID: " + building.gameObject.name);
            return;
        }

        ItemDictionary itemDictionary = FindObjectOfType<ItemDictionary>();

        if (itemDictionary == null)
        {
            Debug.LogError("ItemDictionary not found.");
            return;
        }

        GameObject itemPrefab =
            itemDictionary.GetItemPrefab(building.itemID);

        if (itemPrefab == null)
        {
            Debug.LogError(
                "No item prefab found for Item ID: " + building.itemID
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
            Debug.Log("Cannot retrieve building. Hotbar and inventory are full.");
            return;
        }

        Destroy(building.gameObject);

        Debug.Log("Building retrieved: " + itemPrefab.name);
    }

    public virtual void UseTool()
    {
        Item item = GetComponentInParent<Item>();

        Debug.Log(
            "Tool used: " +
            (item != null ? item.name : gameObject.name)
        );
    }
}