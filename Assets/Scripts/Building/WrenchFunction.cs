using UnityEngine;
using UnityEngine.InputSystem;

public class WrenchFunction : MonoBehaviour
{
    [Header("Item Function")]
    [SerializeField] private int wrenchItemID = 1;

    private PlacementController placementController;
    private PlayerHeldItem playerHeldItem;
    private Item pendingRelocationTool;

    private void Awake()
    {
        placementController = FindObjectOfType<PlacementController>();
        playerHeldItem = FindObjectOfType<PlayerHeldItem>();
    }

    public void UseTool()
    {
        Debug.Log("Wrench function activated!");
    }

    private void Update()
    {
        Item heldItem = null;
        BuildingObject building = null;

        if (playerHeldItem != null)
        {
            heldItem = playerHeldItem.GetHeldItem();

            if (heldItem != null && heldItem.ID == wrenchItemID)
                building = GetBuilding();
        }

        // UPDATE WRENCH PROMPTS
        UpdateWrenchPrompts(building != null);

        if (Keyboard.current == null)
            return;

        if (heldItem == null || heldItem.ID != wrenchItemID)
            return;

        if (building != null)
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
    }

    // UPDATE WRENCH PROMPTS
    private void UpdateWrenchPrompts(bool canInteract)
    {
        if (PromptController.Instance == null)
            return;

        if (canInteract)
        {
            PromptController.Instance.RequestPrompt(PromptType.E, this);
            PromptController.Instance.RequestPrompt(PromptType.F, this);
        }
        else
        {
            PromptController.Instance.ReleasePrompt(PromptType.E, this);
            PromptController.Instance.ReleasePrompt(PromptType.F, this);
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

        if (placementController == null)
        {
            Debug.LogError("PlacementController not found.");
            return;
        }

        Item wrench = playerHeldItem.GetHeldItem();

        bool started = placementController.StartRelocation(building);

        if (!started)
            return;

        pendingRelocationTool = wrench;

        placementController.RelocationFinished += HandleRelocationFinished;
    }

    private void HandleRelocationFinished(bool successful)
    {
        placementController.RelocationFinished -= HandleRelocationFinished;

        if (successful && pendingRelocationTool != null)
        {
            pendingRelocationTool.UseDurability();
        }

        pendingRelocationTool = null;
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

        ItemDictionary itemDictionary = FindObjectOfType<ItemDictionary>();

        if (itemDictionary == null)
        {
            Debug.LogError("ItemDictionary not found.");
            return;
        }

        GameObject itemPrefab = itemDictionary.GetItemPrefab(building.itemID);

        if (itemPrefab == null)
        {
            Debug.LogError(
                "No item prefab found for Item ID: " +
                building.itemID
            );

            return;
        }

        // CREATE TEMPORARY ITEM
        GameObject retrieveItem = Instantiate(itemPrefab);

        Item retrieveItemComponent = retrieveItem.GetComponent<Item>();

        if (retrieveItemComponent == null)
        {
            Debug.LogError(
                "No Item component on prefab for Item ID: " +
                building.itemID
            );

            Destroy(retrieveItem);
            return;
        }

        // SET QUANTITY
        retrieveItemComponent.quantity = 1;
        retrieveItemComponent.UpdateQuantityDisplay();

        HotbarController hotbar = FindObjectOfType<HotbarController>();
        InventoryController inventory = InventoryController.instance;

        bool added = false;

        // HOTBAR FIRST
        if (hotbar != null)
        {
            added = hotbar.AddItem(retrieveItem);
        }

        // INVENTORY SECOND
        if (!added && inventory != null)
        {
            added = inventory.AddItem(retrieveItem);
        }

        Destroy(retrieveItem);

        if (!added)
        {
            Debug.Log(
                "Cannot retrieve building. " +
                "Hotbar and inventory are full."
            );

            return;
        }

        // CONSUME WRENCH DURABILITY
        Item wrench = playerHeldItem.GetHeldItem();

        if (wrench != null)
        {
            wrench.UseDurability();
        }

        WorldGenerator worldGenerator = FindObjectOfType<WorldGenerator>();

        if (worldGenerator != null)
        {
            worldGenerator.RemoveBuilding(building);

            FindFirstObjectByType<RadarController>()
                ?.RefreshCoordinateUI();
        }

        TentController tentController = FindObjectOfType<TentController>();

        if (tentController != null)
        {
            tentController.DebugTentCount();
        }

        Destroy(building.gameObject);

        Debug.Log(
            "Building retrieved: " +
            itemPrefab.name
        );
    }

    private void OnDisable()
    {
        if (PromptController.Instance != null)
        {
            PromptController.Instance.ReleasePrompt(PromptType.E, this);
            PromptController.Instance.ReleasePrompt(PromptType.F, this);
        }
    }
}