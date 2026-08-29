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
        if (itemToAdd == null) return false;

        // Loop through slots to check if we can stack with an existing item of the same type
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot != null && slot.currentItem != null)
            {
                Item slotItem = slot.currentItem.GetComponent<Item>();
                if (slotItem != null && slotItem.ID == itemToAdd.ID)
                {
                    slotItem.AddToStack();
                    return true;
                }
            }
        }

        // Loop through every slot inside the inventory panel to find an empty slot
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            // Check if the slot exists and is currently empty
            if (slot != null && slot.currentItem == null)
            {
                // Instantiate the item prefab as a child of the empty slot
                GameObject newItem = Instantiate(itemPrefab, slotTransform);

                // Center the item inside the UI slot
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                // Track the item in the slot
                slot.currentItem = newItem;

                return true; // Item successfully added
            }
        }

        // If no empty slot was found
        Debug.Log("Inventory is full");
        return false;
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