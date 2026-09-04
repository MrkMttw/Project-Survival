using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Item : MonoBehaviour
{
    public int ID;

    public Image itemImage;

    [Header("Stack Settings")]
    public bool stackable = true;

    [Tooltip("Maximum number of this item that can exist in one stack.")]
    public int maxStackSize = 99;

    [Header("Quantity")]
    public int quantity = 1;

    [Header("Building")]
    public bool isBuildable;
    public GameObject buildingPrefab;
    private TMP_Text quantityText;

    private void Awake()
    {
        quantityText = GetComponentInChildren<TMP_Text>();

        // Non-stackable items can only have a quantity of 1
        if (!stackable)
        {
            quantity = 1;
        }
        else
        {
            // Prevent the starting quantity from exceeding the stack limit
            quantity = Mathf.Clamp(quantity, 1, maxStackSize);
        }

        UpdateQuantityDisplay();
    }

    public virtual void UseItem()
    {
        Debug.Log("Using item: " + name);
    }

    public void UpdateQuantityDisplay()
    {
        if (quantityText != null)
        {
            quantityText.text = quantity > 1 ? quantity.ToString() : "";
        }
    }

    // Adds as many items as possible without exceeding maxStackSize.
    // Returns the number of items that were actually added.
    public int AddToStack(int amount = 1)
    {
        if (!stackable || amount <= 0)
            return 0;

        int availableSpace = maxStackSize - quantity;

        int amountAdded = Mathf.Min(amount, availableSpace);

        quantity += amountAdded;

        UpdateQuantityDisplay();

        return amountAdded;
    }

    public int RemoveFromStack(int amount = 1)
    {
        if (amount <= 0)
            return 0;

        int removed = Mathf.Min(amount, quantity);

        quantity -= removed;

        if (quantity <= 0)
        {
            Destroy(gameObject);
            return removed;
        }

        UpdateQuantityDisplay();

        return removed;
    }

    public bool IsStackFull()
    {
        if (!stackable)
            return true;

        return quantity >= maxStackSize;
    }

    public int GetRemainingStackSpace()
    {
        if (!stackable)
            return 0;

        return maxStackSize - quantity;
    }

    public GameObject CloneItem(int newQuantity)
    {
        GameObject clone = Instantiate(gameObject);

        Item cloneItem = clone.GetComponent<Item>();

        cloneItem.stackable = stackable;
        cloneItem.maxStackSize = maxStackSize;

        if (stackable)
        {
            cloneItem.quantity = Mathf.Clamp(newQuantity, 1, maxStackSize);
        }
        else
        {
            cloneItem.quantity = 1;
        }

        cloneItem.UpdateQuantityDisplay();

        return clone;
    }
}