using UnityEngine;

[CreateAssetMenu(
    fileName = "NewWorldObjectLightPreset",
    menuName = "Lighting/World Object Light Preset"
)]
public class WorldObjectLightPreset : ScriptableObject
{
    [Header("World Object")]
    public GameObject worldObjectPrefab;

    [Header("Light Settings")]
    [Min(0f)]
    public float intensity = 2f;

    [Min(0f)]
    public float radius = 5f;

    [Header("Hostile Mob Protection")]
    public bool hasHostileMobProtection = false;

    [Min(0f)]
    public float hostileMobProtectionRadius = 10f;

    [Header("Flicker")]
    public bool enableFlicker = true;

    [Range(0f, 1f)]
    public float flickerAmount = 0.15f;

    [Min(0f)]
    public float flickerSpeed = 2f;

    [Header("Radius Animation")]
    [Range(0f, 1f)]
    public float radiusVariation = 0.03f;
}