using UnityEngine;

[CreateAssetMenu(fileName = "NewFoodPreset", menuName = "Food/Food Preset")]
public class FoodPreset : ScriptableObject
{
    [Header("Food")]
    public GameObject itemPrefab;

    [Header("Hunger")]
    public float hungerRestore = 10f;

    [Header("Eating")]
    public float eatingDelay = 2f;
}