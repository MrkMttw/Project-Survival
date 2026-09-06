using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public static InventoryController instance { get; private set; }

    private ItemDictionary itemDictionary;

    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefabs;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void Start()
    {
        itemDictionary = FindObjectOfType<ItemDictionary>();
    }
    
    public bool AddItem(GameObject itemPrefab)
    {
        Item itemToAdd = itemPrefab.GetComponent<Item>();

        if (itemToAdd == null)
            return false;

        if (itemToAdd.quantity <= 0)
            return false;

        // NON-STACKABLE ITEM

        if (!itemToAdd.stackable)
        {
            foreach (Transform slotTransform in inventoryPanel.transform)
            {
                Slot slot = slotTransform.GetComponent<Slot>();

                if (slot != null && slot.currentItem == null)
                {
                    GameObject newItem = Instantiate(itemPrefab, slotTransform);

                    newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                    Item newItemComponent = newItem.GetComponent<Item>();

                    if (newItemComponent != null)
                    {
                        newItemComponent.quantity = 1;
                        newItemComponent.UpdateQuantityDisplay();
                    }

                    slot.currentItem = newItem;

                    return true;
                }
            }

            Debug.Log("Inventory is full");
            return false;
        }

        // STACKABLE ITEM

        int remainingQuantity = itemToAdd.quantity;
        int originalQuantity = itemToAdd.quantity;

        // FIRST: Fill existing stacks

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            if (remainingQuantity <= 0)
                break;

            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot == null || slot.currentItem == null)
                continue;

            Item slotItem = slot.currentItem.GetComponent<Item>();

            if (slotItem == null)
                continue;

            // Same item + both stackable
            if (slotItem.ID == itemToAdd.ID && slotItem.stackable)
            {
                int addedAmount = slotItem.AddToStack(remainingQuantity);

                remainingQuantity -= addedAmount;
            }
        }

        // SECOND: Create new stacks in empty slots

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            if (remainingQuantity <= 0)
                break;

            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot == null || slot.currentItem != null)
                continue;

            GameObject newItem = Instantiate(itemPrefab, slotTransform);

            Item newItemComponent = newItem.GetComponent<Item>();

            if (newItemComponent != null)
            {
                int amountForThisStack = Mathf.Min(
                    remainingQuantity,
                    newItemComponent.maxStackSize
                );

                newItemComponent.quantity = amountForThisStack;
                newItemComponent.UpdateQuantityDisplay();

                remainingQuantity -= amountForThisStack;
            }

            newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            slot.currentItem = newItem;
        }

        // -----------------------------------------------------
        // UPDATE REMAINING WORLD ITEM
        // -----------------------------------------------------

        itemToAdd.quantity = remainingQuantity;
        itemToAdd.UpdateQuantityDisplay();

        // Successfully added something
        return remainingQuantity < originalQuantity;
    }

    public List<InventorySaveData> GetInventoryItems()
    {
        List<InventorySaveData> invData = new List<InventorySaveData>();

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();

                invData.Add(new InventorySaveData
                {
                    itemID = item.ID,
                    slotIndex = slotTransform.GetSiblingIndex(),
                    quantity = item.quantity
                });
            }
        }

        return invData;
    }

    public void SetInventoryItems(List<InventorySaveData> inventorySaveData)
    {
        foreach (Transform child in inventoryPanel.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, inventoryPanel.transform);
        }

        foreach (InventorySaveData data in inventorySaveData)
        {
            if (data.slotIndex < slotCount)
            {
                Slot slot = inventoryPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();
                GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);

                if (itemPrefab != null)
                {
                    GameObject item = Instantiate(itemPrefab, slot.transform);
                    item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                    Item itemComponent = item.GetComponent<Item>();
                    if (itemComponent != null && data.quantity > 1)
                    {
                        itemComponent.quantity = data.quantity;
                        itemComponent.UpdateQuantityDisplay();
                    }

                    slot.currentItem = item;
                }
            }
        }
    }
}