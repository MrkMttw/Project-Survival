using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeButtonUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI itemName;

    private CraftingRecipe recipe;
    private string originalItemName;

    public void Setup(CraftingRecipe recipe, bool unlocked)
    {
        this.recipe = recipe;
        originalItemName = recipe.itemPrefab.name;

        icon.sprite = recipe.recipeIcon;

        SetUnlocked(unlocked);

        GetComponent<Button>().onClick.AddListener(SelectRecipe);
    }

    public CraftingRecipe GetRecipe()
    {
        return recipe;
    }

    public void SetUnlocked(bool unlocked)
    {
        GetComponent<Button>().interactable = unlocked;

        if (recipe == null || itemName == null)
            return;

        if (recipe.requiredCampfireLevel > 0 &&
            !unlocked &&
            !FindFirstObjectByType<CraftingController>()
                .IsNearCraftingTable())
        {
            itemName.text = "Requires Crafting Table";
        }
        else
        {
            itemName.text = originalItemName;
        }
    }

    public void SelectRecipe()
    {
        CraftingController craftingController =
            FindFirstObjectByType<CraftingController>();

        if (craftingController != null)
            craftingController.SelectRecipe(recipe);
    }
}