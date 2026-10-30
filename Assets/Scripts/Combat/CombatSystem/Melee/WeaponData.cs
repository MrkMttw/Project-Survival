using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Combat/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon")]
    public GameObject itemPrefab;

    [Header("Attack")]
    public int minDamage = 5;
    public int maxDamage = 10;

    [Tooltip("Attacks per second")]
    public float attackSpeed = 2f;

    public float attackRange = 1.5f;

    [Header("Knockback")]
    public float knockbackStrength = 5f;
}