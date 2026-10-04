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

            bool hasEnoughInventory =
                ingredientItem != null &&
                inventory != null &&
                inventory.HasItem(
                    ingredientItem.ID,
                    ingredient.amount
                );

            bool hasEnoughHotbar =
                ingredientItem != null &&
                hotbar != null &&
                hotbar.HasItem(
                    ingredientItem.ID,
                    ingredient.amount
                );

            bool hasEnough =
                hasEnoughInventory || hasEnoughHotbar;

            string color =
                hasEnough ? "green" : "red";

            recipeText.text +=
                "<color=" + color + ">" +
                ingredient.itemPrefab.name +
                " x" + ingredient.amount +
                "</color>\n";
        }
    }
}