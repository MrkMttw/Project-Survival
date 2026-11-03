using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingController : MonoBehaviour
{
    [Header("Recipe List")]
    public List<CraftingLevel> recipeLevels;

    [Header("Left Side")]
    public Transform contentPanel;
    public GameObject craftingLevelButtonPrefab;
    public GameObject recipeButtonPrefab;

    [Header("Right Side")]
    public GameObject recipeDetails;
    public Image recipeIcon;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI craftingQuantity;
    public TextMeshProUGUI recipeText;
    public TextMeshProUGUI descriptionDetails;

    [Header("References")]
    public Transform player;
    public MenuController menuController;
    public TabController tabController;

    private CraftingTableBehavior currentCraftingTable;
    private bool isOpen;

    public bool IsOpen => isOpen;
    
    private CampfireController campfireController;
    private CraftingRecipe selectedRecipe;
    private bool nearCraftingTable;

    private void Start()
    {
        campfireController =
            FindFirstObjectByType<CampfireController>();

        recipeDetails.SetActive(false);

        GenerateRecipeList();
    }
    
    private void Update()
    {
        UpdateCraftingPrompt();
    }

    private void OnEnable()
    {
        if (campfireController == null)
        {
            campfireController =
                FindFirstObjectByType<CampfireController>();
        }

        if (contentPanel != null)
        {
            RefreshRecipes();
        }
    }

    public void SetCraftingTableAccess(bool hasAccess)
    {
        nearCraftingTable = hasAccess;
        RefreshRecipes();
    }

    public void RefreshRecipes()
    {
        campfireController =
            FindFirstObjectByType<CampfireController>();

        foreach (Transform child in contentPanel)
        {
            RecipeButtonUI buttonUI =
                child.GetComponent<RecipeButtonUI>();

            if (buttonUI == null)
                continue;

            CraftingRecipe recipe = buttonUI.GetRecipe();

            if (recipe == null)
                continue;

            bool unlocked;

            if (recipe.requiredCampfireLevel <= 0)
            {
                unlocked = true;
            }
            else
            {
                unlocked =
                    nearCraftingTable &&
                    campfireController != null &&
                    campfireController.CampfireLevel >=
                    recipe.requiredCampfireLevel;
            }

            buttonUI.SetUnlocked(unlocked);
        }
    }

    private void GenerateRecipeList()
    {
        foreach (CraftingLevel level in recipeLevels)
        {
            if (level == null)
                continue;

            GameObject levelButton = Instantiate(
                craftingLevelButtonPrefab,
                contentPanel
            );

            TextMeshProUGUI levelText =
                levelButton.GetComponentInChildren<TextMeshProUGUI>();

            if (levelText != null)
                levelText.text = "Campfire Level " + level.level;

            foreach (CraftingRecipe recipe in level.recipes)
            {
                if (recipe == null || recipe.itemPrefab == null)
                    continue;

                GameObject recipeButton = Instantiate(
                    recipeButtonPrefab,
                    contentPanel
                );

                bool unlocked;

                if (recipe.requiredCampfireLevel <= 0)
                {
                    unlocked = true;
                }
                else
                {
                    unlocked =
                        nearCraftingTable &&
                        campfireController != null &&
                        campfireController.CampfireLevel >=
                        recipe.requiredCampfireLevel;
                }

                SetupRecipeButton(
                    recipeButton,
                    recipe,
                    unlocked
                );
            }
        }
    }

    private void SetupRecipeButton(
        GameObject recipeButton,
        CraftingRecipe recipe,
        bool unlocked
    )
    {
        RecipeButtonUI buttonUI =
            recipeButton.GetComponent<RecipeButtonUI>();

        if (buttonUI != null)
            buttonUI.Setup(recipe, unlocked);
    }

    public void SelectRecipe(CraftingRecipe recipe)
    {
        selectedRecipe = recipe;

        recipeDetails.SetActive(true);

        recipeIcon.sprite = recipe.recipeIcon;
        itemName.text = recipe.itemPrefab.name;
        craftingQuantity.text = "x" + recipe.craftingQuantity;
        descriptionDetails.text = recipe.description;

        recipeText.text = "";

        foreach (CraftingIngredient ingredient in recipe.ingredients)
        {
            Item ingredientItem =
                ingredient.itemPrefab.GetComponent<Item>();

            InventoryController inventory =
                InventoryController.instance;

            HotbarController hotbar =
                FindFirstObjectByType<HotbarController>();

            int inventoryAmount = 0;
            int hotbarAmount = 0;

            if (ingredientItem != null)
            {
                if (inventory != null)
                {
                    inventoryAmount =
                        inventory.GetItemQuantity(
                            ingredientItem.ID
                        );
                }

                if (hotbar != null)
                {
                    hotbarAmount =
                        hotbar.GetItemQuantity(
                            ingredientItem.ID
                        );
                }
            }

            bool hasEnough =
                inventoryAmount + hotbarAmount >=
                ingredient.amount;

            string color =
                hasEnough ? "green" : "red";

            recipeText.text +=
                "<color=" + color + ">" +
                ingredient.itemPrefab.name +
                " x" + ingredient.amount +
                "</color>\n";
        }
    }

    public void CraftSelectedRecipe()
    {
        if (selectedRecipe == null)
            return;

        InventoryController inventory =
            InventoryController.instance;

        HotbarController hotbar =
            FindFirstObjectByType<HotbarController>();

        // Check all ingredients first
        foreach (CraftingIngredient ingredient in selectedRecipe.ingredients)
        {
            Item ingredientItem =
                ingredient.itemPrefab.GetComponent<Item>();

            if (ingredientItem == null)
                return;

            int inventoryAmount = 0;
            int hotbarAmount = 0;

            if (inventory != null)
            {
                inventoryAmount =
                    inventory.GetItemQuantity(
                        ingredientItem.ID
                    );
            }

            if (hotbar != null)
            {
                hotbarAmount =
                    hotbar.GetItemQuantity(
                        ingredientItem.ID
                    );
            }

            if (inventoryAmount + hotbarAmount < ingredient.amount)
                return;
        }

        // Check if crafted item has somewhere to go
        bool canStoreItem = false;

        if (hotbar != null)
        {
            canStoreItem =
                hotbar.CanAddItem(
                    selectedRecipe.itemPrefab,
                    selectedRecipe.craftingQuantity
                );
        }

        if (!canStoreItem && inventory != null)
        {
            canStoreItem =
                inventory.CanAddItem(
                    selectedRecipe.itemPrefab,
                    selectedRecipe.craftingQuantity
                );
        }

        if (!canStoreItem)
            return;

        // Remove ingredients
        foreach (CraftingIngredient ingredient in selectedRecipe.ingredients)
        {
            Item ingredientItem =
                ingredient.itemPrefab.GetComponent<Item>();

            int remainingAmount = ingredient.amount;

            // HOTBAR FIRST
            if (hotbar != null)
            {
                int removedFromHotbar =
                    hotbar.RemoveItem(
                        ingredientItem.ID,
                        remainingAmount
                    );

                remainingAmount -= removedFromHotbar;
            }

            // INVENTORY SECOND
            if (remainingAmount > 0 && inventory != null)
            {
                inventory.RemoveItem(
                    ingredientItem.ID,
                    remainingAmount
                );
            }
        }

        GameObject craftedItem =
            Instantiate(selectedRecipe.itemPrefab);

        craftedItem.GetComponent<Item>().quantity =
            selectedRecipe.craftingQuantity;

        bool addedToHotbar = false;

        if (hotbar != null)
        {
            addedToHotbar =
                hotbar.AddItem(craftedItem);
        }

        if (!addedToHotbar && inventory != null)
        {
            inventory.AddItem(craftedItem);
        }

        Destroy(craftedItem);

        SelectRecipe(selectedRecipe);
    }

    public bool IsNearCraftingTable()
    {
        return nearCraftingTable;
    }

    // UPDATE CRAFTING PROMPT
    private void UpdateCraftingPrompt()
    {
        if (PromptController.Instance == null)
            return;

        if (isOpen || player == null)
        {
            PromptController.Instance.ReleasePrompt(
                PromptType.E, this);

            return;
        }

        CraftingTableBehavior[] tables =
            FindObjectsByType<CraftingTableBehavior>(
                FindObjectsSortMode.None);

        bool nearbyTable = false;

        foreach (CraftingTableBehavior table in tables)
        {
            if (table == null)
                continue;

            if (table.IsPlayerInRange(player.position))
            {
                nearbyTable = true;
                break;
            }
        }

        if (nearbyTable)
        {
            PromptController.Instance.RequestPrompt(
                PromptType.E, this);
        }
        else
        {
            PromptController.Instance.ReleasePrompt(
                PromptType.E, this);
        }
    }

    // INTERACT WITH CRAFTING TABLE
    public bool TryInteract()
    {
        if (isOpen)
        {
            CloseCraftingPage();
            return true;
        }

        if (player == null || menuController == null)
            return false;

        CraftingTableBehavior[] tables =
            FindObjectsByType<CraftingTableBehavior>(
                FindObjectsSortMode.None);

        foreach (CraftingTableBehavior table in tables)
        {
            if (table == null ||
                !table.IsPlayerInRange(player.position))
            {
                continue;
            }

            currentCraftingTable = table;
            isOpen = true;

            SetCraftingTableAccess(true);

            if (PromptController.Instance != null)
            {
                PromptController.Instance.ReleasePrompt(
                    PromptType.E, this);
            }

            menuController.OpenCraftingPage();

            if (tabController != null)
            {
                tabController.ActiveTab(3);
            }

            return true;
        }

        return false;
    }

    public void ResetCraftingPanel()
    {
        selectedRecipe = null;

        if (recipeDetails != null)
                recipeDetails.SetActive(false);
    }

    // CLOSE CRAFTING PAGE
    private void CloseCraftingPage()
    {
        isOpen = false;
        currentCraftingTable = null;

        // RESET CRAFTING PANEL
        ResetCraftingPanel();

        SetCraftingTableAccess(false);

        if (menuController != null)
            menuController.menuCanvas.SetActive(false);
    }

    // CLOSE FOR MENU
    public void CloseForMenu()
    {
        if (isOpen)
        {
            CloseCraftingPage();
        }
    }
}