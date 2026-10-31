using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class BuildingPreset
{
    public string direction;
    public GameObject buildingPrefab;
    public GameObject ghostPrefab;
}

public class Item : MonoBehaviour
{
    public int ID;

    public Image itemImage;

    [Header("Stack Settings")]
    public bool stackable = true;
    public int maxStackSize = 99;

    [Header("Quantity")]
    public int quantity = 1;

    [Header("Durability")]
    public bool hasDurability;

    [Min(1)]
    public int maxDurability = 100;

    [SerializeField]
    private int currentDurability = 100;

    public int CurrentDurability => currentDurability;

    [Header("Building")]
    public bool isBuildable;
    public GameObject buildingPrefab;
    public GameObject ghostPrefab;

    [Header("Directional Building")]
    public bool useDirectionalPrefabs;

    public List<BuildingPreset> buildingPresets =
        new List<BuildingPreset>();

    [Header("Smelting")]
    public bool isSmeltable;

    [Min(0.1f)]
    public float smeltingTime = 5f;

    [Header("Fuel")]
    public bool isFuel;

    [Min(0.1f)]
    public float fuelBurnDuration = 10f;

    private TMP_Text quantityText;
    private DurabilityBehavior durabilityBar;

    private void Awake()
    {
        quantityText = GetComponentInChildren<TMP_Text>();

        if (hasDurability)
        {
            stackable = false;
            quantity = 1;

            currentDurability = maxDurability;
        }
        else if (!stackable)
        {
            quantity = 1;
        }
        else
        {
            quantity = Mathf.Clamp(
                quantity,
                1,
                maxStackSize
            );
        }

        durabilityBar =
            GetComponentInChildren<DurabilityBehavior>(true);

        UpdateQuantityDisplay();
    }

    private void Start()
    {
        UpdateDurabilityBar();
    }

    private void OnValidate()
    {
        smeltingTime = Mathf.Max(0.1f, smeltingTime);
        fuelBurnDuration = Mathf.Max(0.1f, fuelBurnDuration);

        maxDurability = Mathf.Max(1, maxDurability);

        if (hasDurability)
        {
            stackable = false;
            quantity = 1;

            currentDurability = Mathf.Clamp(
                currentDurability,
                0,
                maxDurability
            );
        }
    }

    public virtual void UseItem()
    {
        Debug.Log("Using item: " + name);
    }

    // DURABILITY

    public bool UseDurability(int uses = 1)
    {
        if (!hasDurability)
            return true;

        if (uses <= 0)
            return true;

        currentDurability = Mathf.Max(
            0,
            currentDurability - uses
        );

        UpdateDurabilityBar();

        if (currentDurability <= 0)
        {
            BreakItem();
            return false;
        }

        return true;
    }

    public void RestoreDurability(int amount)
    {
        if (!hasDurability || amount <= 0)
            return;

        currentDurability = Mathf.Min(
            maxDurability,
            currentDurability + amount
        );

        UpdateDurabilityBar();
    }

    public float GetDurabilityPercentage()
    {
        if (!hasDurability || maxDurability <= 0)
            return 0f;

        return (float)currentDurability / maxDurability;
    }

    public void SetCurrentDurability(int amount)
    {
        if (!hasDurability)
            return;

        currentDurability = Mathf.Clamp(
            amount,
            0,
            maxDurability
        );

        UpdateDurabilityBar();

        if (currentDurability <= 0)
            BreakItem();
    }

    public void CopyDurabilityFrom(Item source)
    {
        if (source == null || !hasDurability)
            return;

        SetCurrentDurability(source.CurrentDurability);
    }

    private void UpdateDurabilityBar()
    {
        if (durabilityBar == null)
        {
            durabilityBar =
                GetComponentInChildren<DurabilityBehavior>(true);
        }

        if (durabilityBar != null)
            durabilityBar.UpdateBar();
    }

    private void BreakItem()
    {
        Slot slot = GetComponentInParent<Slot>();

        if (slot != null && slot.currentItem == gameObject)
            slot.currentItem = null;

        Destroy(gameObject);
    }

    // QUANTITY

    public void UpdateQuantityDisplay()
    {
        if (quantityText != null)
        {
            quantityText.text =
                quantity > 1 ? quantity.ToString() : "";
        }
    }

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
            BreakItem();
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

        cloneItem.quantity = stackable
            ? Mathf.Clamp(newQuantity, 1, maxStackSize)
            : 1;

        // DURABILITY
        // Awake initializes new durable items at maximum durability.

        cloneItem.UpdateQuantityDisplay();

        return clone;
    }
}