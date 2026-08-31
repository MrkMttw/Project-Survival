using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightCycle : MonoBehaviour
{
    [Header("References")]
    public GameClock gameClock;
    public Light2D globalLight;

    [Header("Lighting")]
    [Range(0f, 1f)]
    public float dayIntensity = 1f;

    [Range(0f, 1f)]
    public float nightIntensity = 0.2f;

    [Header("Time")]
    [Range(0f, 24f)]
    public float sunriseTime = 6f;

    [Range(0f, 24f)]
    public float sunsetTime = 18f;

    [Header("Transition")]
    public float transitionSpeed = 2f;

    private float targetIntensity;

    void Start()
    {
        if (gameClock == null)
        {
            Debug.LogError("DayNightCycle: GameClock is not assigned!");
            return;
        }

        if (globalLight == null)
        {
            Debug.LogError("DayNightCycle: Global Light 2D is not assigned!");
            return;
        }

        UpdateTargetIntensity();

        // Start at the correct brightness immediately
        globalLight.intensity = targetIntensity;
    }

    void Update()
    {
        if (gameClock == null || globalLight == null)
            return;

        UpdateTargetIntensity();

        // Smoothly transition lighting
        globalLight.intensity = Mathf.MoveTowards(
            globalLight.intensity,
            targetIntensity,
            transitionSpeed * Time.deltaTime
        );
    }

    void UpdateTargetIntensity()
    {
        float hour = gameClock.GetHour();
        float minute = gameClock.GetMinute();

        float currentTime = hour + (minute / 60f);

        if (currentTime >= sunriseTime && currentTime < sunsetTime)
        {
            targetIntensity = dayIntensity;
        }
        else
        {
            targetIntensity = nightIntensity;
        }
    }
}