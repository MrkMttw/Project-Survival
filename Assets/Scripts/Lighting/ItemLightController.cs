using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ItemLightController : MonoBehaviour
{
    [Header("Light Presets")]
    public ItemLightPreset[] lightPresets;

    [Header("Night Lighting")]
    public bool isNight = true;

    [Header("Player Held Item")]
    public GameObject playerHeldItemObject;

    private PlayerHeldItem playerHeldItem;
    private Light2D heldItemLight;

    private ItemLightPreset activePreset;
    private ItemLightPreset retainedPreset;

    private readonly Dictionary<int, ItemLightPreset> presetDictionary =
        new Dictionary<int, ItemLightPreset>();

    private float noiseOffset;

    private bool lightRetained;
    private bool flickerActive;
    private bool promptRequested;

    private int retainedItemID = -1;

    private void Awake()
    {
        noiseOffset = Random.Range(0f, 1000f);

        FindPlayerHeldItem();
        BuildPresetDictionary();
    }

    private void OnEnable()
    {
        if (playerHeldItem != null)
        {
            playerHeldItem.HeldItemChanged += OnHeldItemChanged;
        }

        RefreshHeldItemLight();
        UpdateLightPrompt();
    }

    private void Start()
    {
        // REFRESH AFTER OTHER START METHODS
        RefreshHeldItemLight();
        UpdateLightPrompt();
    }

    private void OnDisable()
    {
        if (playerHeldItem != null)
        {
            playerHeldItem.HeldItemChanged -= OnHeldItemChanged;
        }

        DisableHeldLight();
        ReleaseLightPrompt();
    }

    private void Update()
    {
        if (!flickerActive ||
            heldItemLight == null ||
            activePreset == null ||
            !heldItemLight.enabled)
        {
            return;
        }

        UpdateFlicker();
    }

    // FIND PLAYER HELD ITEM

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

    // BUILD PRESET CACHE

    private void BuildPresetDictionary()
    {
        presetDictionary.Clear();

        if (lightPresets == null)
            return;

        foreach (ItemLightPreset preset in lightPresets)
        {
            if (preset == null || preset.itemPrefab == null)
                continue;

            Item presetItem =
                preset.itemPrefab.GetComponent<Item>();

            if (presetItem == null)
                continue;

            if (!presetDictionary.ContainsKey(presetItem.ID))
            {
                presetDictionary.Add(
                    presetItem.ID,
                    preset
                );
            }
        }
    }

    // EQUIPMENT EVENT

    private void OnHeldItemChanged(Item item)
    {
        RefreshHeldItemLight();
        UpdateLightPrompt();
    }

    // MAIN LIGHT REFRESH

    private void RefreshHeldItemLight()
    {
        if (playerHeldItem == null)
        {
            DisableHeldLight();
            return;
        }

        if (!isNight)
        {
            DisableHeldLight();
            return;
        }

        Item currentItem =
            playerHeldItem.GetHeldItem();

        ItemLightPreset preset =
            GetPresetForItem(currentItem);

        // NO ITEM OR NO LIGHT PRESET
        // KEEP RETAINED LIGHT

        if (preset == null)
        {
            if (lightRetained &&
                retainedPreset != null)
            {
                EnableHeldLight(retainedPreset);
            }
            else
            {
                DisableHeldLight();
            }

            return;
        }

        // ONLY USE RETAINED LIGHT
        // WHEN THE RETAINED ITEM IS EQUIPPED

        if (lightRetained &&
            retainedItemID == currentItem.ID)
        {
            EnableHeldLight(retainedPreset);
        }
        else
        {
            DisableHeldLight();
        }
    }

    // FIND PRESET

    private ItemLightPreset GetPresetForItem(Item item)
    {
        if (item == null)
            return null;

        if (presetDictionary.TryGetValue(
            item.ID,
            out ItemLightPreset preset))
        {
            return preset;
        }

        return null;
    }

    // FIND HELD ITEM LIGHT

    private void FindHeldItemLight()
    {
        if (heldItemLight != null)
            return;

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

        heldItemLight = lights[0];
    }

    // ENABLE HELD LIGHT

    private void EnableHeldLight(ItemLightPreset preset)
    {
        if (preset == null)
            return;

        FindHeldItemLight();

        if (heldItemLight == null)
            return;

        activePreset = preset;

        heldItemLight.enabled = true;

        ApplyPreset(heldItemLight, preset);

        flickerActive = preset.enableFlicker;
    }

    // TOGGLE LIGHT

    public void ToggleLight()
    {
        if (playerHeldItem == null)
            return;

        Item currentItem =
            playerHeldItem.GetHeldItem();

        if (currentItem == null)
            return;

        ItemLightPreset preset =
            GetPresetForItem(currentItem);

        if (preset == null)
            return;

        // TURN OFF RETAINED LIGHT

        if (lightRetained &&
            retainedItemID == currentItem.ID)
        {
            lightRetained = false;
            retainedItemID = -1;
            retainedPreset = null;

            RefreshHeldItemLight();

            return;
        }

        // RETAIN THIS ITEM'S LIGHT

        lightRetained = true;
        retainedItemID = currentItem.ID;
        retainedPreset = preset;

        RefreshHeldItemLight();
    }

    public bool TryToggleHeldItemLight()
    {
        if (playerHeldItem == null)
            return false;

        Item currentItem =
            playerHeldItem.GetHeldItem();

        if (currentItem == null)
            return false;

        if (GetPresetForItem(currentItem) == null)
            return false;

        ToggleLight();

        return true;
    }

    // APPLY PRESET

    private void ApplyPreset(
        Light2D light,
        ItemLightPreset preset
    )
    {
        if (light == null || preset == null)
            return;

        light.intensity = preset.intensity;
        light.pointLightOuterRadius = preset.radius;
    }

    // FLICKER

    private void UpdateFlicker()
    {
        float noise = Mathf.PerlinNoise(
            noiseOffset,
            Time.time * activePreset.flickerSpeed
        );

        float variation = (noise * 2f) - 1f;

        // INTENSITY

        float intensityMultiplier =
            1f + (variation * activePreset.flickerAmount);

        heldItemLight.intensity =
            activePreset.intensity * intensityMultiplier;

        // RADIUS

        float radiusMultiplier =
            1f + (variation * activePreset.radiusVariation);

        heldItemLight.pointLightOuterRadius =
            activePreset.radius * radiusMultiplier;
    }

    // DISABLE LIGHT

    private void DisableHeldLight()
    {
        flickerActive = false;
        activePreset = null;

        if (heldItemLight != null)
        {
            heldItemLight.enabled = false;
        }
    }

    // DAY / NIGHT

    public void SetNight(bool night)
    {
        if (isNight == night)
            return;

        isNight = night;

        RefreshHeldItemLight();
    }

    // REFRESH

    public void RefreshLights()
    {
        DisableHeldLight();

        heldItemLight = null;

        BuildPresetDictionary();

        RefreshHeldItemLight();
        UpdateLightPrompt();
    }

    // UPDATE LIGHT PROMPT

    private void UpdateLightPrompt()
    {
        if (PromptController.Instance == null)
            return;

        bool shouldShowPrompt = false;

        if (playerHeldItem != null)
        {
            Item currentItem =
                playerHeldItem.GetHeldItem();

            shouldShowPrompt =
                currentItem != null &&
                GetPresetForItem(currentItem) != null;
        }

        if (shouldShowPrompt == promptRequested)
            return;

        promptRequested = shouldShowPrompt;

        if (promptRequested)
        {
            PromptController.Instance.RequestPrompt(
                PromptType.E,
                this
            );
        }
        else
        {
            PromptController.Instance.ReleasePrompt(
                PromptType.E,
                this
            );
        }
    }

    private void ReleaseLightPrompt()
    {
        if (PromptController.Instance != null)
        {
            PromptController.Instance.ReleasePrompt(
                PromptType.E,
                this
            );
        }

        promptRequested = false;
    }
}