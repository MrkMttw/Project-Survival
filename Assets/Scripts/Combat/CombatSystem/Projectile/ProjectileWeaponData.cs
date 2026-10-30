using UnityEngine;

[CreateAssetMenu(
    fileName = "New Projectile Weapon",
    menuName = "Combat/Projectile Weapon Data"
)]
public class ProjectileWeaponData : ScriptableObject
{
    [Header("Weapon")]
    public GameObject itemPrefab;

    [Header("Projectile")]
    public GameObject projectilePrefab;

    [Header("Ammunition")]
    public GameObject ammunitionItem;

    [Header("Attack")]
    public int minDamage = 5;
    public int maxDamage = 10;

    [Tooltip("Attacks per second")]
    public float attackSpeed = 2f;

    [Header("Projectile Movement")]
    public float projectileSpeed = 10f;
    public float maxDistance = 10f;
    public float maxLifetime = 3f;

    [Header("Knockback")]
    public float knockbackStrength = 5f;
}