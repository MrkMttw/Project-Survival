using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CampfireLightController : MonoBehaviour
{
    [Header("Light Settings")]
    [Min(0f)]
    [SerializeField] private float intensity = 2f;

    [Min(0f)]
    [SerializeField] private float radius = 5f;

    [Header("Flicker")]
    [SerializeField] private bool enableFlicker = true;

    [Range(0f, 1f)]
    [SerializeField] private float flickerAmount = 0.15f;

    [Min(0f)]
    [SerializeField] private float flickerSpeed = 2f;

    [Header("Radius Animation")]
    [Range(0f, 1f)]
    [SerializeField] private float radiusVariation = 0.03f;

    [Header("Visuals")]
    [SerializeField] private GameObject unlitVisual;

    private Light2D campfireLight;
    private CampfireController campfireController;
    private SpriteRenderer litVisual;
    private Animator campfireAnimator;

    private Coroutine flickerRoutine;

    private float noiseOffset;
    private bool hasFuel;

    private void Awake()
    {
        campfireLight = GetComponent<Light2D>();
        litVisual = GetComponent<SpriteRenderer>();
        campfireAnimator = GetComponent<Animator>();

        campfireController =
            FindFirstObjectByType<CampfireController>();

        noiseOffset = Random.Range(0f, 1000f);

        if (campfireLight == null)
            Debug.LogError("Campfire Light2D missing.");

        if (litVisual == null)
            Debug.LogError("Campfire SpriteRenderer missing.");

        if (campfireAnimator == null)
            Debug.LogError("Campfire Animator missing.");

        if (campfireController == null)
            Debug.LogError("CampfireController missing.");

        ApplyBaseLightSettings();
    }

    private void OnEnable()
    {
        if (campfireController == null)
            return;

        campfireController.FuelStateChanged += SetLight;

        SetLight(campfireController.currentWood > 0);
    }

    private void OnDisable()
    {
        if (campfireController != null)
            campfireController.FuelStateChanged -= SetLight;

        StopFlicker();
    }

    // FUEL STATE

    public void SetLight(bool isLit)
    {
        hasFuel = isLit;

        if (litVisual != null)
            litVisual.enabled = hasFuel;

        if (campfireAnimator != null)
            campfireAnimator.enabled = hasFuel;

        if (unlitVisual != null)
            unlitVisual.SetActive(!hasFuel);

        UpdateLightState();
    }

    // LIGHT STATE

    private void UpdateLightState()
    {
        if (campfireLight == null)
            return;

        campfireLight.enabled = hasFuel;

        if (hasFuel && enableFlicker)
            StartFlicker();
        else
            StopFlicker();
    }

    // BASE LIGHT SETTINGS

    private void ApplyBaseLightSettings()
    {
        if (campfireLight == null)
            return;

        campfireLight.intensity = intensity;
        campfireLight.pointLightOuterRadius = radius;
    }

    // FLICKER

    private void StartFlicker()
    {
        if (flickerRoutine != null)
            return;

        flickerRoutine = StartCoroutine(FlickerLoop());
    }

    private void StopFlicker()
    {
        if (flickerRoutine != null)
        {
            StopCoroutine(flickerRoutine);
            flickerRoutine = null;
        }

        ApplyBaseLightSettings();
    }

    private IEnumerator FlickerLoop()
    {
        while (hasFuel &&
               enableFlicker &&
               campfireLight != null &&
               campfireLight.enabled)
        {
            float noise = Mathf.PerlinNoise(
                noiseOffset,
                Time.time * flickerSpeed
            );

            float variation = (noise * 2f) - 1f;

            // INTENSITY

            float intensityMultiplier =
                1f + (variation * flickerAmount);

            campfireLight.intensity =
                intensity * intensityMultiplier;

            // RADIUS

            float radiusMultiplier =
                1f + (variation * radiusVariation);

            campfireLight.pointLightOuterRadius =
                radius * radiusMultiplier;

            yield return null;
        }

        flickerRoutine = null;
        ApplyBaseLightSettings();
    }
}