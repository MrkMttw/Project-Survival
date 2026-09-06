using UnityEngine;

[CreateAssetMenu(fileName = "NewFoodPreset", menuName = "Food/Food Preset")]
public class FoodPreset : ScriptableObject
{
    [Header("Food")]
    public GameObject itemPrefab;

    [Header("Hunger")]
    public float hungerRestore = 10f;

    [Header("Health")]
    [Tooltip("Amount of HP restored when eating this food. Set to 0 for no healing.")]
    public float hpRestore = 0f;

    [Header("Eating")]
    public float eatingDelay = 2f;
}