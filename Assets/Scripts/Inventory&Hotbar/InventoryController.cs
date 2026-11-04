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

    [Header("Death Item Scattering")]
    [SerializeField] private float scatterRadius = 1.5f;
    [SerializeField] private float scatterForce = 3f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        InitializeInventorySlots();
    }

    private void InitializeInventorySlots()
    {
        foreach (Transform child in inventoryPanel.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, inventoryPanel.transform);
        }
    }
    
    public bool CanAddItem(GameObject itemPrefab, int quantity)
    {
        Item itemToAdd = itemPrefab.GetComponent<Item>();

        if (itemToAdd == null || quantity <= 0)
            return false;

        // NON-STACKABLE ITEM
        if (!itemToAdd.stackable)
        {
            foreach (Transform slotTransform in inventoryPanel.transform)
            {
                Slot slot = slotTransform.GetComponent<Slot>();

                if (slot != null && slot.currentItem == null)
                    return true;
            }

            return false;
        }

        int remainingQuantity = quantity;

        // FIRST: Check existing stacks
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            if (remainingQuantity <= 0)
                return true;

            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot == null || slot.currentItem == null)
                continue;

            Item slotItem = slot.currentItem.GetComponent<Item>();

            if (slotItem == null)
                continue;

            if (slotItem.ID == itemToAdd.ID &&
                slotItem.stackable)
            {
                int availableSpace =
                    slotItem.maxStackSize - slotItem.quantity;

                remainingQuantity -= availableSpace;
            }
        }

        // SECOND: Check empty slots
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            if (remainingQuantity <= 0)
                return true;

            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot == null || slot.currentItem != null)
                continue;

            remainingQuantity -= itemToAdd.maxStackSize;
        }

        return remainingQuantity <= 0;
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

                    if (newItemComponent != null)
                    {
                        newItemComponent.CopyDurabilityFrom(itemToAdd);
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

        // UPDATE REMAINING WORLD ITEM

        itemToAdd.quantity = remainingQuantity;
        itemToAdd.UpdateQuantityDisplay();

        // Successfully added something
        return remainingQuantity < originalQuantity;
    }

    public bool HasItem(int itemID, int requiredAmount)
    {
        int totalAmount = 0;

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot == null || slot.currentItem == null)
                continue;

            Item item = slot.currentItem.GetComponent<Item>();

            if (item == null)
                continue;

            if (item.ID != itemID)
                continue;

            totalAmount += item.quantity;

            if (totalAmount >= requiredAmount)
                return true;
        }

        return false;
    }

    public int GetItemQuantity(int itemID)
    {
        int totalAmount = 0;

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot == null || slot.currentItem == null)
                continue;

            Item item = slot.currentItem.GetComponent<Item>();

            if (item == null || item.ID != itemID)
                continue;

            totalAmount += item.quantity;
        }

        return totalAmount;
    }

    public int RemoveItem(int itemID, int amount)
    {
        int remainingAmount = amount;

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            if (remainingAmount <= 0)
                break;

            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot == null || slot.currentItem == null)
                continue;

            Item item = slot.currentItem.GetComponent<Item>();

            if (item == null || item.ID != itemID)
                continue;

            int amountToRemove =
                Mathf.Min(item.quantity, remainingAmount);

            item.RemoveFromStack(amountToRemove);

            remainingAmount -= amountToRemove;

            if (item.quantity <= 0)
            {
                slot.currentItem = null;
            }
        }

        return amount - remainingAmount;
    }

    public void DropAllItems(Vector3 dropPosition, float scatterRadius)
    {
        if (inventoryPanel == null)
            return;

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot == null || slot.currentItem == null)
                continue;

            Item item = slot.currentItem.GetComponent<Item>();

            if (item == null)
                continue;

            // CREATE DROPPED ITEM
            GameObject droppedItem = item.CloneItem(item.quantity);

            if (droppedItem == null)
                continue;

            droppedItem.transform.SetParent(null, true);

            Vector2 offset =
                Random.insideUnitCircle * scatterRadius;

            droppedItem.transform.position =
                dropPosition + new Vector3(offset.x, offset.y, 0f);

            droppedItem.transform.localScale = Vector3.one;

            // CLEAR INVENTORY
            GameObject originalItem = slot.currentItem;
            slot.currentItem = null;

            Destroy(originalItem);
        }

        Debug.Log("Inventory items dropped.");
    }
}