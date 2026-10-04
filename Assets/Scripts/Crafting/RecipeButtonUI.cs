using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeButtonUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI itemName;

    private CraftingRecipe recipe;

    public void Setup(CraftingRecipe recipe, bool unlocked)
    {
        this.recipe = recipe;

        icon.sprite = recipe.recipeIcon;
        itemName.text = recipe.itemPrefab.name;

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
    }

    public void SelectRecipe()
    {
        CraftingController craftingController =
            FindFirstObjectByType<CraftingController>();

        if (craftingController != null)
            craftingController.SelectRecipe(recipe);
    }
}