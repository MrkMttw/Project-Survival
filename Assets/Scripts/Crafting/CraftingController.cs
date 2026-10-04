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

    private CampfireController campfireController;
    private CraftingRecipe selectedRecipe;
    
    private void Start()
    {
        campfireController =
            FindFirstObjectByType<CampfireController>();

        recipeDetails.SetActive(false);

        GenerateRecipeList();
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

        if (selectedRecipe != null)
        {
            SelectRecipe(selectedRecipe);
        }
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

            bool unlocked =
                campfireController != null &&
                campfireController.CampfireLevel >=
                recipe.requiredCampfireLevel;

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

                bool unlocked =
                    campfireController != null &&
                    campfireController.CampfireLevel >=
                    recipe.requiredCampfireLevel;

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
}