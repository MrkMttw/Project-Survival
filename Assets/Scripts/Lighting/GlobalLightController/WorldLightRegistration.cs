using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WorldLightRegistration : MonoBehaviour
{
    [Header("Prefab Identification")]
    [SerializeField] private bool isIndependentPrefab;
    [SerializeField] private string independentPrefabID;

    [Header("Light Availability")]
    [SerializeField] private bool isAvailable = true;

    private Light2D[] lights;
    private bool isWithinRange;

    public bool IsWithinRange => isWithinRange;
    public bool IsIndependentPrefab => isIndependentPrefab;
    public string IndependentPrefabID => independentPrefabID;

    private void Awake()
    {
        lights = GetComponentsInChildren<Light2D>(true);
    }

    private void OnEnable()
    {
        if (GlobalLightingController.Instance != null)
        {
            GlobalLightingController.Instance.RegisterLight(this);
        }
        else
        {
            Debug.LogWarning(
                "WorldLightRegistration: GlobalLightingController was not found.",
                this
            );
        }

        RefreshLightState();
    }

    private void OnDisable()
    {
        if (GlobalLightingController.Instance != null)
        {
            GlobalLightingController.Instance.UnregisterLight(this);
        }

        SetWithinRange(false);
    }

    // DISTANCE STATE

    public void SetWithinRange(bool withinRange)
    {
        isWithinRange = withinRange;
        RefreshLightState();
    }

    // AVAILABILITY

    public void SetAvailable(bool available)
    {
        isAvailable = available;
        RefreshLightState();
    }

    // APPLY LIGHT STATE

    public void RefreshLightState()
    {
        if (lights == null)
            return;

        bool shouldEnable = isAvailable && isWithinRange;

        foreach (Light2D light in lights)
        {
            if (light != null)
                light.enabled = shouldEnable;
        }
    }
}