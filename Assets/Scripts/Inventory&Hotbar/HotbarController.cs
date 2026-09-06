using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarController : MonoBehaviour
{
    public GameObject hotbarPanel;
    public GameObject slotPrefab;
    public int slotCount = 7;

    [Header("Player")]
    public Transform player;

    public PlayerHeldItem playerHeldItem;

    [Header("Building")]
    public PlacementController placementController;

    private int selectedSlot = -1;

    private ItemDictionary itemDictionary;
    private Key[] hotbarKeys;

    private void Awake()
    {
        itemDictionary = FindObjectOfType<ItemDictionary>();

        hotbarKeys = new Key[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            hotbarKeys[i] = i < 9
                ? (Key)((int)Key.Digit1 + i)
                : Key.Digit0;
        }
    }

    private void Update()
    {
        for (int i = 0; i < slotCount; i++)
        {
            if (Keyboard.current[hotbarKeys[i]].wasPressedThisFrame)
            {
                UseItemInSlot(i);
            }
        }

        // Drop selected hotbar item
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            if (Keyboard.current.leftShiftKey.isPressed ||
                Keyboard.current.rightShiftKey.isPressed)
            {
                DropSelectedStack();
            }
            else
            {
                DropSelectedItem();
            }
        }
    }

    private void DropSelectedItem()
    {
        if (hotbarPanel == null)
            return;

        if (selectedSlot < 0 ||
            selectedSlot >= hotbarPanel.transform.childCount)
            return;

        Slot slot = hotbarPanel.transform
            .GetChild(selectedSlot)
            .GetComponent<Slot>();

        if (slot == null || slot.currentItem == null)
            return;

        Item item = slot.currentItem.GetComponent<Item>();

        if (item == null)
            return;

        // Drop ONE item
        GameObject droppedItem = item.CloneItem(1);

        if (droppedItem == null)
            return;

        droppedItem.transform.position = player.position;

        // Remove one from the hotbar
        item.quantity--;

        if (item.quantity <= 0)
        {
            Destroy(slot.currentItem);
            slot.currentItem = null;

            playerHeldItem.ClearHeldItem();

            HighlightSlot(-1);
            selectedSlot = -1;
        }
        else
        {
            item.UpdateQuantityDisplay();

            // Still holding the same item
            playerHeldItem.SetHeldItem(item);
        }
    }

    private void DropSelectedStack()
    {
        if (hotbarPanel == null)
            return;

        if (selectedSlot < 0 ||
            selectedSlot >= hotbarPanel.transform.childCount)
            return;

        Slot slot = hotbarPanel.transform
            .GetChild(selectedSlot)
            .GetComponent<Slot>();

        if (slot == null || slot.currentItem == null)
            return;

        Item item = slot.currentItem.GetComponent<Item>();

        if (item == null)
            return;

        int amountToDrop = item.quantity;

        // Drop entire stack
        GameObject droppedItem =
            item.CloneItem(amountToDrop);

        if (droppedItem == null)
            return;

        droppedItem.transform.position = player.position;

        // Empty hotbar slot
        Destroy(slot.currentItem);
        slot.currentItem = null;

        // Nothing left to hold
        playerHeldItem.ClearHeldItem();

        HighlightSlot(-1);
        selectedSlot = -1;
    }

    public void SelectSlot(int index)
    {
        UseItemInSlot(index);
    }

    private void UseItemInSlot(int index)
    {
        if (hotbarPanel == null)
            return;

        if (index < 0 ||
            index >= hotbarPanel.transform.childCount)
            return;

        // Click/press currently equipped slot again = unequip
        if (selectedSlot == index && playerHeldItem != null)
        {
            playerHeldItem.ClearHeldItem();

            if (placementController != null)
            {
                placementController.CancelPlacement();
            }

            HighlightSlot(-1);

            selectedSlot = -1;

            return;
        }

        // Select new slot
        selectedSlot = index;

        HighlightSlot(selectedSlot);

        Slot slot = hotbarPanel.transform
            .GetChild(index)
            .GetComponent<Slot>();

        if (slot == null)
            return;

        // Empty slot
        if (slot.currentItem == null)
        {
            if (playerHeldItem != null)
            {
                playerHeldItem.ClearHeldItem();
            }

            if (placementController != null)
            {
                placementController.CancelPlacement();
            }

            return;
        }

        Item item = slot.currentItem.GetComponent<Item>();

        if (item == null)
            return;

        // Equip item
        if (playerHeldItem != null)
        {
            playerHeldItem.SetHeldItem(item);
        }

        // Building
        if (item.isBuildable)
        {
            if (placementController != null)
            {
                placementController.StartPlacement(item);
            }
        }
        else
        {
            if (placementController != null)
            {
                placementController.CancelPlacement();
            }

            WrenchFunction tool =
                item.GetComponentInChildren<WrenchFunction>();

            if (tool != null)
            {
                tool.UseTool();
            }
            else
            {
                item.UseItem();
            }
        }
    }

    // CONSUME SELECTED ITEM

    public void ConsumeSelectedItem(int amount = 1)
    {
        if (hotbarPanel == null)
            return;

        if (selectedSlot < 0 ||
            selectedSlot >= hotbarPanel.transform.childCount)
            return;

        Slot slot = hotbarPanel.transform
            .GetChild(selectedSlot)
            .GetComponent<Slot>();

        if (slot == null || slot.currentItem == null)
            return;

        Item item = slot.currentItem.GetComponent<Item>();

        if (item == null)
            return;

        // Last item in the stack
        if (item.quantity <= amount)
        {
            // Clear held visual BEFORE destroying the item
            if (playerHeldItem != null)
            {
                playerHeldItem.ClearHeldItem();
            }

            // Clear hotbar slot
            slot.currentItem = null;

            // Remove highlight
            HighlightSlot(-1);

            // Nothing selected anymore
            selectedSlot = -1;

            // Destroy the item
            item.RemoveFromStack(amount);

            return;
        }

        // Stack still has items remaining
        item.RemoveFromStack(amount);

        // Continue holding the item
        if (playerHeldItem != null)
        {
            playerHeldItem.SetHeldItem(item);
        }
    }

    private void HighlightSlot(int index)
    {
        if (hotbarPanel == null)
            return;

        for (int i = 0;
            i < hotbarPanel.transform.childCount;
            i++)
        {
            Transform slot =
                hotbarPanel.transform.GetChild(i);

            Transform highlight =
                slot.Find("Highlight");

            if (highlight != null)
            {
                highlight.gameObject.SetActive(i == index);
            }
        }
    }

    // ADD ITEM

    public bool AddItem(GameObject itemPrefab)
    {
        Item itemToAdd = itemPrefab.GetComponent<Item>();

        if (itemToAdd == null)
            return false;

        int originalQuantity = itemToAdd.quantity;

        // NON-STACKABLE ITEM
        if (!itemToAdd.stackable)
        {
            foreach (Transform slotTransform in hotbarPanel.transform)
            {
                Slot slot =
                    slotTransform.GetComponent<Slot>();

                if (slot != null &&
                    slot.currentItem == null)
                {
                    GameObject newItem =
                        Instantiate(
                            itemPrefab,
                            slotTransform
                        );

                    newItem.GetComponent<RectTransform>()
                        .anchoredPosition = Vector2.zero;

                    Item newItemComponent =
                        newItem.GetComponent<Item>();

                    if (newItemComponent != null)
                    {
                        newItemComponent.quantity = 1;
                        newItemComponent.UpdateQuantityDisplay();
                    }

                    slot.currentItem = newItem;

                    // One non-stackable item was taken
                    itemToAdd.quantity -= 1;

                    return true;
                }
            }

            return false;
        }

        // STACKABLE ITEM

        int remainingQuantity = itemToAdd.quantity;

        // FIRST: Fill existing stacks
        foreach (Transform slotTransform in hotbarPanel.transform)
        {
            if (remainingQuantity <= 0)
                break;

            Slot slot =
                slotTransform.GetComponent<Slot>();

            if (slot == null ||
                slot.currentItem == null)
                continue;

            Item slotItem =
                slot.currentItem.GetComponent<Item>();

            if (slotItem == null)
                continue;

            // Same item and stackable
            if (slotItem.ID == itemToAdd.ID &&
                slotItem.stackable)
            {
                int addedAmount =
                    slotItem.AddToStack(remainingQuantity);

                remainingQuantity -= addedAmount;
            }
        }

        // SECOND: Create new stacks
        foreach (Transform slotTransform in hotbarPanel.transform)
        {
            if (remainingQuantity <= 0)
                break;

            Slot slot =
                slotTransform.GetComponent<Slot>();

            if (slot == null ||
                slot.currentItem != null)
                continue;

            GameObject newItem =
                Instantiate(
                    itemPrefab,
                    slotTransform
                );

            Item newItemComponent =
                newItem.GetComponent<Item>();

            if (newItemComponent != null)
            {
                int amountForThisStack =
                    Mathf.Min(
                        remainingQuantity,
                        newItemComponent.maxStackSize
                    );

                newItemComponent.quantity =
                    amountForThisStack;

                newItemComponent.UpdateQuantityDisplay();

                remainingQuantity -= amountForThisStack;
            }

            newItem.GetComponent<RectTransform>()
                .anchoredPosition = Vector2.zero;

            slot.currentItem = newItem;
        }

        // UPDATE WORLD ITEM
        itemToAdd.quantity = remainingQuantity;
        itemToAdd.UpdateQuantityDisplay();

        return remainingQuantity < originalQuantity;
    }

    // SAVE

    public List<InventorySaveData> GetHotbarItems()
    {
        List<InventorySaveData> hotbarData =
            new List<InventorySaveData>();

        foreach (Transform slotTransform in hotbarPanel.transform)
        {
            Slot slot =
                slotTransform.GetComponent<Slot>();

            if (slot.currentItem != null)
            {
                Item item =
                    slot.currentItem.GetComponent<Item>();

                hotbarData.Add(new InventorySaveData
                {
                    itemID = item.ID,
                    slotIndex =
                        slotTransform.GetSiblingIndex()
                });
            }
        }

        return hotbarData;
    }

    // LOAD

    public void SetHotbarItems(
        List<InventorySaveData> hotbarSaveData)
    {
        foreach (Transform child in hotbarPanel.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(
                slotPrefab,
                hotbarPanel.transform
            );
        }

        foreach (InventorySaveData data in hotbarSaveData)
        {
            if (data.slotIndex < slotCount)
            {
                Slot slot =
                    hotbarPanel.transform
                    .GetChild(data.slotIndex)
                    .GetComponent<Slot>();

                GameObject itemPrefab =
                    itemDictionary.GetItemPrefab(
                        data.itemID
                    );

                if (itemPrefab != null)
                {
                    GameObject item =
                        Instantiate(
                            itemPrefab,
                            slot.transform
                        );

                    item.GetComponent<RectTransform>()
                        .anchoredPosition = Vector2.zero;

                    slot.currentItem = item;
                }
            }
        }
    }
}