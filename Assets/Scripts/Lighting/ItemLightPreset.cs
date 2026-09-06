using UnityEngine;

[CreateAssetMenu(
    fileName = "NewItemLightPreset",
    menuName = "Lighting/Item Light Preset"
)]
public class ItemLightPreset : ScriptableObject
{
    [Header("Item")]

    [Tooltip("The item prefab that uses this light preset.")]
    public GameObject itemPrefab;


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
    [Tooltip("How much the intensity changes during flickering.")]
    public float flickerAmount = 0.15f;

    [Tooltip("Speed of the flickering animation.")]
    [Min(0f)]
    public float flickerSpeed = 2f;


    [Header("Radius Animation")]

    [Range(0f, 1f)]
    [Tooltip("Small variation in the light radius. Keep this low for a stable light.")]
    public float radiusVariation = 0.03f;
}