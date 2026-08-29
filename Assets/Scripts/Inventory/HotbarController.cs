using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarController : MonoBehaviour
{
    public GameObject hotbarPanel;
    public GameObject slotPrefab;
    public int slotCount = 7;

    private ItemDictionary itemDictionary;
    private Key[] hotbarKeys;

    private void Awake()
    {
        itemDictionary = FindObjectOfType<ItemDictionary>();

        hotbarKeys = new Key[slotCount];
        for (int i = 0; i < slotCount; i++)
        {
            hotbarKeys[i] = i < 9 ? (Key)((int)Key.Digit1 + i) : Key.Digit0;
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
    }

    private void UseItemInSlot(int index)
    {
        Slot slot = hotbarPanel.transform.GetChild(index).GetComponent<Slot>();

        if (slot.currentItem != null)
        {
            Item item = slot.currentItem.GetComponent<Item>();
            item.UseItem();
        }
    }

    // CHANGED: Added this method so pickups can be added to the hotbar first
    public bool AddItem(GameObject itemPrefab)
    {
        Item itemToAdd = itemPrefab.GetComponent<Item>();

        if (itemToAdd == null)
            return false;

        // FIRST: Check if the item already exists in the hotbar
        foreach (Transform slotTransform in hotbarPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot != null && slot.currentItem != null)
            {
                Item slotItem = slot.currentItem.GetComponent<Item>();

                if (slotItem != null && slotItem.ID == itemToAdd.ID)
                {
                    slotItem.AddToStack(itemToAdd.quantity);
                    return true;
                }
            }
        }

        // SECOND: Find an empty hotbar slot
        foreach (Transform slotTransform in hotbarPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot != null && slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slotTransform);

                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                slot.currentItem = newItem;

                return true;
            }
        }

        // Hotbar is full
        return false;
    }

    public List<InventorySaveData> GetHotbarItems()
    {
        List<InventorySaveData> hotbarData = new List<InventorySaveData>();

        foreach (Transform slotTransform in hotbarPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();

                hotbarData.Add(new InventorySaveData
                {
                    itemID = item.ID,
                    slotIndex = slotTransform.GetSiblingIndex()
                });
            }
        }

        return hotbarData;
    }

    public void SetHotbarItems(List<InventorySaveData> hotbarSaveData)
    {
        foreach (Transform child in hotbarPanel.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, hotbarPanel.transform);
        }

        foreach (InventorySaveData data in hotbarSaveData)
        {
            if (data.slotIndex < slotCount)
            {
                Slot slot = hotbarPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();
                GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);

                if (itemPrefab != null)
                {
                    GameObject item = Instantiate(itemPrefab, slot.transform);
                    item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    slot.currentItem = item;
                }
            }
        }
    }
}