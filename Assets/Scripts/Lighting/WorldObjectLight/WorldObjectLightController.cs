using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-90)]
public class WorldObjectLightController : MonoBehaviour
{
    public static WorldObjectLightController Instance { get; private set; }

    [Header("World Object Light Presets")]
    public WorldObjectLightPreset[] lightPresets;

    [Header("Night Lighting")]
    public bool isNight = true;

    private readonly HashSet<WorldObjectLight> registeredLights =
        new HashSet<WorldObjectLight>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError(
                "WorldObjectLightController: Another instance already exists."
            );

            enabled = false;
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (!isNight)
            return;

        registeredLights.RemoveWhere(light => light == null);

        foreach (WorldObjectLight worldLight in registeredLights)
        {
            if (worldLight == null)
                continue;

            if (worldLight.Preset == null)
                continue;

            if (!worldLight.Preset.enableFlicker)
                continue;

            if (!worldLight.HasActiveLight())
                continue;

            worldLight.UpdateFlicker();
        }
    }

    // REGISTER
    public void RegisterLight(WorldObjectLight worldLight)
    {
        if (worldLight == null)
            return;

        registeredLights.Add(worldLight);
        worldLight.ApplyPreset();
    }

    // UNREGISTER
    public void UnregisterLight(WorldObjectLight worldLight)
    {
        if (worldLight == null)
            return;

        registeredLights.Remove(worldLight);
    }

    // HOSTILE MOB PROTECTION
    public bool IsPositionProtectedFromHostileSpawning(
        Vector2 position)
    {
        registeredLights.RemoveWhere(light => light == null);

        foreach (WorldObjectLight worldLight in registeredLights)
        {
            if (worldLight == null || !worldLight.isActiveAndEnabled)
                continue;

            if (!worldLight.ProvidesHostileMobProtection)
                continue;

            float radius =
                worldLight.HostileMobProtectionRadius;

            float distanceSquared =
                (position -
                (Vector2)worldLight.transform.position).sqrMagnitude;

            if (distanceSquared <= radius * radius)
                return true;
        }

        return false;
    }

    // DAY / NIGHT
    public void SetNight(bool night)
    {
        isNight = night;

        if (night)
            RefreshLights();
    }

    // REFRESH
    public void RefreshLights()
    {
        foreach (WorldObjectLight worldLight in registeredLights)
        {
            if (worldLight == null)
                continue;

            worldLight.ApplyPreset();
        }
    }
}