using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ItemLightController : MonoBehaviour
{
    [Header("Light Presets")]

    [Tooltip("Light presets for held items.")]
    public ItemLightPreset[] lightPresets;


    [Header("Night Lighting")]

    [Tooltip("Whether item lighting is currently active.")]
    public bool isNight = true;


    [Header("Player Held Item")]

    [Tooltip("GameObject containing the PlayerHeldItem component.")]
    public GameObject playerHeldItemObject;


    private PlayerHeldItem playerHeldItem;

    private Light2D heldItemLight;

    private ItemLightPreset currentPreset;

    private GameObject currentHeldObject;

    private float noiseOffset;


    private void Awake()
    {
        noiseOffset = Random.Range(0f, 1000f);
    }


    private void Start()
    {
        FindPlayerHeldItem();
    }


    private void Update()
    {
        UpdateHeldItemLight();
    }


    // PLAYER HELD ITEM

    private void FindPlayerHeldItem()
    {
        if (playerHeldItemObject == null)
        {
            Debug.LogWarning(
                "ItemLightController: PlayerHeldItem GameObject is not assigned."
            );

            return;
        }


        playerHeldItem =
            playerHeldItemObject.GetComponent<PlayerHeldItem>();


        if (playerHeldItem == null)
        {
            Debug.LogWarning(
                "ItemLightController: PlayerHeldItem component was not found."
            );
        }
    }


    // MAIN LIGHT UPDATE

    private void UpdateHeldItemLight()
    {
        if (playerHeldItem == null)
        {
            DisableHeldLight();
            return;
        }


        // No lighting during daytime.
        if (!isNight)
        {
            DisableHeldLight();
            return;
        }


        Item currentItem =
            playerHeldItem.GetHeldItem();


        if (currentItem == null)
        {
            DisableHeldLight();
            return;
        }


        ItemLightPreset preset =
            GetPresetForItem(currentItem);


        if (preset == null)
        {
            DisableHeldLight();
            return;
        }


        // If the held item changed,
        // find its Light2D again.
        if (currentPreset != preset)
        {
            currentPreset = preset;

            FindHeldItemLight();
        }


        if (heldItemLight == null)
        {
            FindHeldItemLight();
        }


        if (heldItemLight == null)
        {
            return;
        }


        heldItemLight.enabled = true;


        ApplyPreset(
            heldItemLight,
            preset
        );
    }


    // FIND PRESET

    private ItemLightPreset GetPresetForItem(Item item)
    {
        if (item == null)
            return null;


        if (lightPresets == null ||
            lightPresets.Length == 0)
        {
            return null;
        }


        foreach (ItemLightPreset preset in lightPresets)
        {
            if (preset == null)
                continue;


            if (preset.itemPrefab == null)
                continue;


            // Compare the Item component on the preset prefab.
            Item presetItem =
                preset.itemPrefab.GetComponent<Item>();


            if (presetItem == null)
                continue;


            if (presetItem.ID == item.ID)
            {
                return preset;
            }
        }


        return null;
    }


    // FIND HELD ITEM LIGHT

    private void FindHeldItemLight()
    {
        heldItemLight = null;


        if (playerHeldItemObject == null)
            return;


        Light2D[] lights =
            playerHeldItemObject.GetComponentsInChildren<Light2D>(
                true
            );


        if (lights.Length == 0)
        {
            Debug.LogWarning(
                "ItemLightController: No Light2D found under PlayerHeldItem."
            );

            return;
        }


        // Use the first Light2D found.
        heldItemLight = lights[0];
    }


    // APPLY PRESET

    private void ApplyPreset(
        Light2D light,
        ItemLightPreset preset
    )
    {
        if (light == null ||
            preset == null)
        {
            return;
        }


        // No animation.
        if (!preset.enableFlicker)
        {
            light.intensity =
                preset.intensity;

            light.pointLightOuterRadius =
                preset.radius;

            return;
        }


        // Smooth Perlin Noise.
        float noise =
            Mathf.PerlinNoise(
                noiseOffset,
                Time.time * preset.flickerSpeed
            );


        // Convert 0-1 into -1 to +1.
        float variation =
            (noise * 2f) - 1f;


        // INTENSITY

        float intensityMultiplier =
            1f +
            (variation * preset.flickerAmount);


        light.intensity =
            preset.intensity *
            intensityMultiplier;


        // RADIUS

        float radiusMultiplier =
            1f +
            (variation * preset.radiusVariation);


        light.pointLightOuterRadius =
            preset.radius *
            radiusMultiplier;
    }


    // DISABLE

    private void DisableHeldLight()
    {
        if (heldItemLight == null)
            return;


        heldItemLight.enabled = false;
    }


    // DAY / NIGHT

    public void SetNight(bool night)
    {
        isNight = night;


        if (!night)
        {
            DisableHeldLight();
        }
    }


    // REFRESH

    public void RefreshLights()
    {
        DisableHeldLight();

        heldItemLight = null;

        currentPreset = null;

        currentHeldObject = null;
    }
}