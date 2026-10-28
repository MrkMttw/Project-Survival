using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class FurnaceBehavior : MonoBehaviour
{
    private BuildingObject building;
    private BoxCollider2D interactionCollider;
    private WorldGenerator worldGenerator;
    private FurnaceController furnaceController;

    private int inputItemID;
    private int inputQuantity;
    private int fuelItemID;
    private int fuelQuantity;
    private int outputItemID;
    private int outputQuantity;

    private bool isCooking;
    private float smeltProgress;
    private float fuelRemaining;

    public bool IsCooking => isCooking;

    private void Awake()
    {
        building = GetComponentInChildren<BuildingObject>();
        interactionCollider = GetComponent<BoxCollider2D>();
        worldGenerator = FindFirstObjectByType<WorldGenerator>();
        furnaceController = FindFirstObjectByType<FurnaceController>();
    }

    private void Start()
    {
        RestoreState();
    }

    private void Update()
    {
        if (building == null || worldGenerator == null)
            return;

        // A furnace only processes while its own chunk is loaded.
        ProcessFurnace(Time.deltaTime);
        SaveState();
    }

    public bool IsPlayerInRange(Vector2 playerPosition)
    {
        return building != null &&
               interactionCollider != null &&
               interactionCollider.OverlapPoint(playerPosition);
    }

    public void SetCooking(bool cooking)
    {
        isCooking = cooking;
        SaveState();
    }

    public float GetFuelProgress()
    {
        if (fuelRemaining <= 0f ||
            fuelItemID <= 0 ||
            worldGenerator == null ||
            worldGenerator.itemDictionary == null)
        {
            return 0f;
        }

        GameObject fuelPrefab =
            worldGenerator.itemDictionary.GetItemPrefab(fuelItemID);

        if (fuelPrefab == null)
            return 0f;

        Item fuelItem = fuelPrefab.GetComponent<Item>();

        if (fuelItem == null || fuelItem.fuelBurnDuration <= 0f)
            return 0f;

        return Mathf.Clamp01(
            fuelRemaining / fuelItem.fuelBurnDuration);
    }

    public float GetSmeltProgress()
    {
        if (inputItemID <= 0 ||
            inputQuantity <= 0 ||
            worldGenerator == null ||
            worldGenerator.itemDictionary == null)
        {
            return 0f;
        }

        GameObject inputPrefab =
            worldGenerator.itemDictionary.GetItemPrefab(inputItemID);

        if (inputPrefab == null)
            return 0f;

        Item inputItem = inputPrefab.GetComponent<Item>();

        if (inputItem == null || inputItem.smeltingTime <= 0f)
            return 0f;

        return Mathf.Clamp01(smeltProgress / inputItem.smeltingTime);
    }

    private void ProcessFurnace(float deltaTime)
    {
        if (fuelRemaining > 0f)
        {
            fuelRemaining = Mathf.Max(0f, fuelRemaining - deltaTime);
        }

        if (!isCooking || inputQuantity <= 0)
            return;

        CraftingRecipe recipe = FindMatchingRecipe();

        if (recipe == null || recipe.itemPrefab == null)
            return;

        Item resultItem = recipe.itemPrefab.GetComponent<Item>();

        if (resultItem == null)
            return;

        if (outputQuantity > 0 &&
            (outputItemID != resultItem.ID ||
             !resultItem.stackable ||
             outputQuantity + recipe.craftingQuantity > resultItem.maxStackSize))
        {
            return;
        }

        if (fuelRemaining <= 0f)
        {
            if (fuelQuantity <= 0 || worldGenerator.itemDictionary == null)
                return;

            GameObject fuelPrefab =
                worldGenerator.itemDictionary.GetItemPrefab(fuelItemID);

            if (fuelPrefab == null)
                return;

            Item fuelItem = fuelPrefab.GetComponent<Item>();

            if (fuelItem == null || !fuelItem.isFuel)
                return;

            fuelQuantity--;
            fuelRemaining = fuelItem.fuelBurnDuration;
            RefreshOpenSlots();
        }

        GameObject inputPrefab =
            worldGenerator.itemDictionary.GetItemPrefab(inputItemID);

        if (inputPrefab == null)
            return;

        Item inputItem = inputPrefab.GetComponent<Item>();

        if (inputItem == null)
            return;

        smeltProgress += deltaTime;

        if (smeltProgress < inputItem.smeltingTime)
            return;

        smeltProgress = 0f;
        inputQuantity--;

        if (inputQuantity <= 0)
        {
            inputItemID = 0;
            inputQuantity = 0;
        }

        if (outputQuantity <= 0)
            outputItemID = resultItem.ID;

        outputQuantity += recipe.craftingQuantity;

        SaveState();
        RefreshOpenSlots();
    }

    private void RefreshOpenSlots()
    {
        if (furnaceController != null)
        {
            furnaceController.RefreshSlots(this);
        }
    }

    private CraftingRecipe FindMatchingRecipe()
    {
        if (inputItemID <= 0 ||
            furnaceController == null ||
            furnaceController.furnaceRecipes == null)
        {
            return null;
        }

        foreach (CraftingRecipe recipe in furnaceController.furnaceRecipes)
        {
            if (recipe == null ||
                !recipe.isSmeltingRecipe ||
                recipe.itemPrefab == null ||
                recipe.ingredients == null ||
                recipe.ingredients.Count != 1 ||
                recipe.craftingQuantity <= 0)
            {
                continue;
            }

            CraftingIngredient ingredient = recipe.ingredients[0];

            if (ingredient == null ||
                ingredient.itemPrefab == null ||
                ingredient.amount <= 0)
            {
                continue;
            }

            Item recipeInput = ingredient.itemPrefab.GetComponent<Item>();

            if (recipeInput != null &&
                recipeInput.ID == inputItemID &&
                inputQuantity >= ingredient.amount)
            {
                return recipe;
            }
        }

        return null;
    }

    private void RestoreState()
    {
        if (worldGenerator == null || building == null)
            return;

        if (!worldGenerator.TryGetBuildingSaveData(
            building, out BuildingSaveData data))
        {
            return;
        }

        inputItemID = data.furnaceInputItemID;
        inputQuantity = data.furnaceInputQuantity;
        fuelItemID = data.furnaceFuelItemID;
        fuelQuantity = data.furnaceFuelQuantity;
        outputItemID = data.furnaceOutputItemID;
        outputQuantity = data.furnaceOutputQuantity;
        isCooking = data.furnaceIsCooking;
        smeltProgress = data.furnaceSmeltProgress;
        fuelRemaining = data.furnaceFuelRemaining;
    }

    private void SaveState()
    {
        if (worldGenerator == null || building == null)
            return;

        if (!worldGenerator.TryGetBuildingSaveData(
            building, out BuildingSaveData data))
        {
            return;
        }

        data.furnaceInputItemID = inputItemID;
        data.furnaceInputQuantity = inputQuantity;
        data.furnaceFuelItemID = fuelItemID;
        data.furnaceFuelQuantity = fuelQuantity;
        data.furnaceOutputItemID = outputItemID;
        data.furnaceOutputQuantity = outputQuantity;
        data.furnaceIsCooking = isCooking;
        data.furnaceSmeltProgress = smeltProgress;
        data.furnaceFuelRemaining = fuelRemaining;
    }

    public void LoadSlots(Slot inputSlot, Slot fuelSlot, Slot outputSlot)
    {
        ClearSlot(inputSlot);
        ClearSlot(fuelSlot);
        ClearSlot(outputSlot);

        CreateSlotItem(inputSlot, inputItemID, inputQuantity);
        CreateSlotItem(fuelSlot, fuelItemID, fuelQuantity);
        CreateSlotItem(outputSlot, outputItemID, outputQuantity);
    }

    public void SyncSlots(Slot inputSlot, Slot fuelSlot, Slot outputSlot)
    {
        ReadSlot(inputSlot, out inputItemID, out inputQuantity);
        ReadSlot(fuelSlot, out fuelItemID, out fuelQuantity);
        ReadSlot(outputSlot, out outputItemID, out outputQuantity);

        SaveState();
    }

    private void ReadSlot(Slot slot, out int itemID, out int quantity)
    {
        itemID = 0;
        quantity = 0;

        if (slot == null || slot.currentItem == null)
            return;

        Item item = slot.currentItem.GetComponent<Item>();

        if (item == null || item.quantity <= 0)
            return;

        itemID = item.ID;
        quantity = item.quantity;
    }

    private void CreateSlotItem(Slot slot, int itemID, int quantity)
    {
        if (slot == null || itemID <= 0 || quantity <= 0 ||
            worldGenerator == null || worldGenerator.itemDictionary == null)
        {
            return;
        }

        GameObject prefab =
            worldGenerator.itemDictionary.GetItemPrefab(itemID);

        if (prefab == null)
            return;

        GameObject itemObject = Instantiate(prefab, slot.transform);
        itemObject.transform.localScale = Vector3.one;

        RectTransform rect = itemObject.GetComponent<RectTransform>();

        if (rect != null)
            rect.anchoredPosition = Vector2.zero;

        Item item = itemObject.GetComponent<Item>();

        if (item == null)
        {
            Destroy(itemObject);
            return;
        }

        item.quantity = item.stackable
            ? Mathf.Clamp(quantity, 1, item.maxStackSize)
            : 1;

        item.UpdateQuantityDisplay();
        slot.currentItem = itemObject;
    }

    private void ClearSlot(Slot slot)
    {
        if (slot == null)
            return;

        if (slot.currentItem != null)
            Destroy(slot.currentItem);

        slot.currentItem = null;
    }
}
