using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ContainerBehavior : MonoBehaviour
{
    [Header("Container Settings")]
    [SerializeField, Min(1)]
    private int capacity = 20;

    public int Capacity => capacity;

    private BuildingObject building;
    private WorldGenerator worldGenerator;
    private BoxCollider2D interactionCollider;
    private void Awake()
    {
        building = GetComponentInChildren<BuildingObject>(true);
        worldGenerator = FindFirstObjectByType<WorldGenerator>();
        interactionCollider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        InitializeContainer();
    }

    // INITIALIZE CONTAINER

    private void InitializeContainer()
    {
        if (building == null || worldGenerator == null)
        {
            Debug.LogError(
                "ContainerBehavior: Missing BuildingObject or WorldGenerator.",
                this
            );

            return;
        }

        // ENSURE BUILDING RECORD EXISTS
        if (!worldGenerator.TryGetBuildingSaveData(
            building, out BuildingSaveData data))
        {
            worldGenerator.RegisterBuilding(building);

            if (!worldGenerator.TryGetBuildingSaveData(
                building, out data))
            {
                Debug.LogError(
                    "ContainerBehavior: Could not register building: " +
                    building.buildingID,
                    this
                );

                return;
            }
        }

        // RESTORE EXISTING CONTAINER
        if (data.hasContainerState)
        {
            // RESTORE SAVED CAPACITY
            if (data.containerCapacity > 0)
            {
                capacity = data.containerCapacity;
            }

            if (data.containerItems == null)
            {
                data.containerItems =
                    new List<ItemSlotSaveData>();
            }

            return;
        }

        // INITIALIZE NEW CONTAINER
        data.hasContainerState = true;
        data.containerCapacity = capacity;

        data.containerItems =
            new List<ItemSlotSaveData>();

        SaveState();
    }

    // CHECK PLAYER INTERACTION RANGE

    public bool IsPlayerInRange(Vector2 playerPosition)
    {
        return building != null &&
            interactionCollider != null &&
            interactionCollider.OverlapPoint(playerPosition);
    }

    // LOAD CONTAINER SLOTS

    public void LoadSlots(Slot[] slots)
    {
        if (!IsReady() || slots == null)
            return;

        if (!worldGenerator.TryGetBuildingSaveData(
            building, out BuildingSaveData data))
        {
            return;
        }

        // CLEAR EXISTING UI ITEMS
        foreach (Slot slot in slots)
        {
            ClearSlot(slot);
        }

        if (data.containerItems == null)
            return;

        // RESTORE SAVED ITEMS
        foreach (ItemSlotSaveData savedItem in data.containerItems)
        {
            if (savedItem == null ||
                savedItem.slotIndex < 0 ||
                savedItem.slotIndex >= slots.Length ||
                savedItem.slotIndex >= capacity)
            {
                continue;
            }

            CreateSlotItem(
                slots[savedItem.slotIndex],
                savedItem
            );
        }
    }

    // SYNC CONTAINER SLOTS

    public void SyncSlots(Slot[] slots)
    {
        if (!IsReady() || slots == null)
            return;

        if (!worldGenerator.TryGetBuildingSaveData(
            building, out BuildingSaveData data))
        {
            return;
        }

        if (data.containerItems == null)
        {
            data.containerItems =
                new List<ItemSlotSaveData>();
        }

        data.containerItems.Clear();

        int slotLimit = Mathf.Min(
            slots.Length,
            capacity
        );

        if (slots.Length != capacity)
        {
            Debug.LogWarning(
                "ContainerBehavior: Slot count does not match capacity. " +
                "Expected: " + capacity +
                ", Received: " + slots.Length,
                this
            );
        }

        // SAVE EACH OCCUPIED SLOT
        for (int i = 0; i < slotLimit; i++)
        {
            Slot slot = slots[i];

            if (slot == null || slot.currentItem == null)
                continue;

            Item item = slot.currentItem.GetComponent<Item>();

            if (item == null || item.quantity <= 0)
                continue;

            ItemSlotSaveData savedItem =
                new ItemSlotSaveData();

            savedItem.slotIndex = i;
            savedItem.itemID = item.ID;

            savedItem.quantity = item.stackable
                ? Mathf.Clamp(item.quantity, 1, item.maxStackSize)
                : 1;

            savedItem.durability = item.hasDurability
                ? item.CurrentDurability
                : 0;

            data.containerItems.Add(savedItem);
        }

        SaveState();
    }

    // CREATE SAVED ITEM

    private void CreateSlotItem(
        Slot slot,
        ItemSlotSaveData savedItem)
    {
        if (slot == null ||
            savedItem == null ||
            savedItem.itemID <= 0 ||
            savedItem.quantity <= 0 ||
            worldGenerator.itemDictionary == null)
        {
            return;
        }

        GameObject prefab =
            worldGenerator.itemDictionary.GetItemPrefab(
                savedItem.itemID
            );

        if (prefab == null)
            return;

        GameObject itemObject = Instantiate(
            prefab,
            slot.transform
        );

        itemObject.transform.localScale = Vector3.one;

        RectTransform rect =
            itemObject.GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.anchoredPosition = Vector2.zero;
        }

        Item item = itemObject.GetComponent<Item>();

        if (item == null)
        {
            Destroy(itemObject);
            return;
        }

        // RESTORE QUANTITY
        item.quantity = item.stackable
            ? Mathf.Clamp(
                savedItem.quantity,
                1,
                item.maxStackSize
            )
            : 1;

        item.UpdateQuantityDisplay();

        // RESTORE DURABILITY
        if (item.hasDurability)
        {
            // A broken item should not be restored.
            if (savedItem.durability <= 0)
            {
                Destroy(itemObject);
                return;
            }

            item.SetCurrentDurability(
                Mathf.Clamp(
                    savedItem.durability,
                    1,
                    item.maxDurability
                )
            );
        }

        slot.currentItem = itemObject;
    }

    // CLEAR SLOT

    private void ClearSlot(Slot slot)
    {
        if (slot == null)
            return;

        if (slot.currentItem != null)
        {
            Destroy(slot.currentItem);
        }

        slot.currentItem = null;
    }

    // SAVE CONTAINER STATE

    public void SaveState()
    {
        if (!IsReady())
            return;

        if (!worldGenerator.TryGetBuildingSaveData(
            building, out BuildingSaveData data))
        {
            return;
        }

        data.hasContainerState = true;
        data.containerCapacity = capacity;

        if (data.containerItems == null)
        {
            data.containerItems =
                new List<ItemSlotSaveData>();
        }
    }

    // VALIDATION

    private bool IsReady()
    {
        return building != null &&
               worldGenerator != null &&
               worldGenerator.itemDictionary != null;
    }

    // OBJECT DESTRUCTION

    private void OnDestroy()
    {
        SaveState();
    }
}