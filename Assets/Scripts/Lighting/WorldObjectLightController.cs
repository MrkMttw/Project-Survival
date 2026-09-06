using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WorldObjectLightController : MonoBehaviour
{
    [Header("World Object Light Presets")]

    [Tooltip("Presets used by world object prefabs.")]
    public WorldObjectLightPreset[] lightPresets;


    [Header("Night Lighting")]

    [Tooltip("Enable world object lighting during nighttime.")]
    public bool isNight = true;


    private readonly Dictionary<Light2D, float> noiseOffsets =
        new Dictionary<Light2D, float>();


    private void Start()
    {
        RefreshLights();
    }


    private void Update()
    {
        UpdateWorldLights();
    }


    // UPDATE WORLD LIGHTS

    private void UpdateWorldLights()
    {
        if (lightPresets == null ||
            lightPresets.Length == 0)
        {
            return;
        }


        foreach (WorldObjectLightPreset preset in lightPresets)
        {
            if (preset == null ||
                preset.worldObjectPrefab == null)
            {
                continue;
            }


            GameObject[] objects =
                FindObjectsOfType<GameObject>(true);


            foreach (GameObject worldObject in objects)
            {
                if (!IsMatchingWorldObject(
                    worldObject,
                    preset.worldObjectPrefab))
                {
                    continue;
                }


                Light2D[] lights =
                    worldObject.GetComponentsInChildren<Light2D>(
                        true
                    );


                foreach (Light2D light in lights)
                {
                    if (light == null)
                        continue;


                    if (!isNight)
                    {
                        light.enabled = false;
                        continue;
                    }


                    light.enabled = true;


                    ApplyPreset(
                        light,
                        preset
                    );
                }
            }
        }
    }


    // MATCH WORLD OBJECT

    private bool IsMatchingWorldObject(
        GameObject worldObject,
        GameObject prefab
    )
    {
        if (worldObject == null ||
            prefab == null)
        {
            return false;
        }


        string worldObjectName =
            worldObject.name
                .Replace("(Clone)", "")
                .Trim();


        string prefabName =
            prefab.name
                .Replace("(Clone)", "")
                .Trim();


        return worldObjectName == prefabName;
    }


    // APPLY PRESET

    private void ApplyPreset(
        Light2D light,
        WorldObjectLightPreset preset
    )
    {
        if (light == null ||
            preset == null)
        {
            return;
        }


        // NO FLICKER

        if (!preset.enableFlicker)
        {
            light.intensity =
                preset.intensity;

            light.pointLightOuterRadius =
                preset.radius;

            return;
        }


        // CREATE NOISE OFFSET

        if (!noiseOffsets.ContainsKey(light))
        {
            noiseOffsets.Add(
                light,
                Random.Range(0f, 1000f)
            );
        }


        float noiseOffset =
            noiseOffsets[light];


        // PERLIN NOISE

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
            (
                variation *
                preset.flickerAmount
            );


        light.intensity =
            preset.intensity *
            intensityMultiplier;


        // RADIUS

        float radiusMultiplier =
            1f +
            (
                variation *
                preset.radiusVariation
            );


        light.pointLightOuterRadius =
            preset.radius *
            radiusMultiplier;
    }


    // DAY / NIGHT

    public void SetNight(bool night)
    {
        isNight = night;


        if (!night)
        {
            DisableWorldObjectLights();
        }
        else
        {
            RefreshLights();
        }
    }


    // DISABLE WORLD OBJECT LIGHTS

    private void DisableWorldObjectLights()
    {
        if (lightPresets == null)
            return;


        foreach (WorldObjectLightPreset preset in lightPresets)
        {
            if (preset == null ||
                preset.worldObjectPrefab == null)
            {
                continue;
            }


            GameObject[] objects =
                FindObjectsOfType<GameObject>(true);


            foreach (GameObject worldObject in objects)
            {
                if (!IsMatchingWorldObject(
                    worldObject,
                    preset.worldObjectPrefab))
                {
                    continue;
                }


                Light2D[] lights =
                    worldObject.GetComponentsInChildren<Light2D>(
                        true
                    );


                foreach (Light2D light in lights)
                {
                    if (light != null)
                    {
                        light.enabled = false;
                    }
                }
            }
        }
    }


    // REFRESH

    public void RefreshLights()
    {
        noiseOffsets.Clear();

        UpdateWorldLights();
    }
}