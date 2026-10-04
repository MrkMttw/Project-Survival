using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "New Crafting Recipe",
    menuName = "Crafting/Crafting Recipe"
)]
public class CraftingRecipe : ScriptableObject
{
    [Header("Crafted Item")]
    public GameObject itemPrefab;
    public Sprite recipeIcon;
    public int craftingQuantity = 1;

    [Header("Ingredients")]
    public List<CraftingIngredient> ingredients;

    [Header("Recipe Info")]
    [TextArea]
    public string description;

    public int requiredCampfireLevel = 0;
}

[System.Serializable]
public class CraftingIngredient
{
    public GameObject itemPrefab;
    public int amount = 1;
}