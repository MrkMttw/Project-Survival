using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(WorldLightRegistration))]
public class WorldObjectLight : MonoBehaviour
{
    [Header("Light Preset")]
    [SerializeField] private WorldObjectLightPreset preset;

    private Light2D[] lights;
    private float noiseOffset;

    public WorldObjectLightPreset Preset => preset;

    public bool ProvidesHostileMobProtection
    {
        get
        {
            if (preset == null)
                return false;

            if (!preset.hasHostileMobProtection)
                return false;

            if (preset.hostileMobProtectionRadius <= 0f)
                return false;

            BuildingObject building = GetBuildingObject();

            if (building != null &&
                building.hasOnOffState &&
                !building.isOn)
            {
                return false;
            }

            return true;
        }
    }

    public float HostileMobProtectionRadius =>
        preset != null
            ? preset.hostileMobProtectionRadius
            : 0f;

    private void Awake()
    {
        lights = GetComponentsInChildren<Light2D>(true);

        noiseOffset = Random.Range(0f, 1000f);

        if (preset == null)
        {
            Debug.LogWarning(
                "WorldObjectLight: No light preset assigned.",
                this
            );

            return;
        }

        ApplyPreset();
    }

    private void OnEnable()
    {
        if (WorldObjectLightController.Instance != null)
        {
            WorldObjectLightController.Instance.RegisterLight(this);
        }
    }

    private void OnDisable()
    {
        if (WorldObjectLightController.Instance != null)
        {
            WorldObjectLightController.Instance.UnregisterLight(this);
        }
    }

    // BUILDING REFERENCE
    private BuildingObject GetBuildingObject()
    {
        BuildingObject building =
            GetComponentInParent<BuildingObject>();

        if (building != null)
            return building;

        return transform.root.GetComponentInChildren<BuildingObject>(true);
    }

    // APPLY PRESET
    public void ApplyPreset()
    {
        if (preset == null || lights == null)
            return;

        foreach (Light2D light in lights)
        {
            if (light == null)
                continue;

            light.intensity = preset.intensity;
            light.pointLightOuterRadius = preset.radius;
        }
    }

    // FLICKER
    public void UpdateFlicker()
    {
        if (preset == null || !preset.enableFlicker)
            return;

        if (lights == null)
            return;

        float noise = Mathf.PerlinNoise(
            noiseOffset,
            Time.time * preset.flickerSpeed
        );

        float variation = (noise * 2f) - 1f;

        float intensityMultiplier =
            1f + (variation * preset.flickerAmount);

        float radiusMultiplier =
            1f + (variation * preset.radiusVariation);

        foreach (Light2D light in lights)
        {
            if (light == null || !light.enabled)
                continue;

            light.intensity =
                preset.intensity * intensityMultiplier;

            light.pointLightOuterRadius =
                preset.radius * radiusMultiplier;
        }
    }

    // CHECK ACTIVE LIGHTS
    public bool HasActiveLight()
    {
        if (lights == null)
            return false;

        foreach (Light2D light in lights)
        {
            if (light != null && light.enabled)
                return true;
        }

        return false;
    }
}