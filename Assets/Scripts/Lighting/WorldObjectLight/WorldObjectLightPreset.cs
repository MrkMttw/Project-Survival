using UnityEngine;

[CreateAssetMenu(
    fileName = "NewWorldObjectLightPreset",
    menuName = "Lighting/World Object Light Preset"
)]
public class WorldObjectLightPreset : ScriptableObject
{
    [Header("World Object")]

    [Tooltip("The world object prefab that uses this light preset.")]
    public GameObject worldObjectPrefab;


    [Header("Light Settings")]

    [Tooltip("Base brightness of the light.")]
    [Min(0f)]
    public float intensity = 2f;

    [Tooltip("Base outer radius of the light.")]
    [Min(0f)]
    public float radius = 5f;


    [Header("Flicker")]

    [Tooltip("Enable smooth light flickering.")]
    public bool enableFlicker = true;

    [Range(0f, 1f)]
    [Tooltip("How much the intensity changes.")]
    public float flickerAmount = 0.15f;

    [Tooltip("How quickly the light flickers.")]
    [Min(0f)]
    public float flickerSpeed = 2f;


    [Header("Radius Animation")]

    [Range(0f, 1f)]
    [Tooltip("Small variation in the light radius. Keep this low for a stable light.")]
    public float radiusVariation = 0.03f;
}