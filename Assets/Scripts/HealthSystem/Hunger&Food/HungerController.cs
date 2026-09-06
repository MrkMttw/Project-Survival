using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HungerController : MonoBehaviour
{
    [Header("Hunger")]
    public float maxHunger = 100f;
    public float currentHunger = 100f;

    [Header("Saturation")]
    public float maxSaturation = 100f;
    public float currentSaturation = 0f;

    [Header("Hunger Drain")]
    public float hungerDrainRate = 1f;
    public float drainInterval = 5f;

    [Header("Starvation")]
    public float starvationDamagePercent = 0.01f;

    [Header("Health")]
    public HealthController healthController;

    [Header("Hunger UI")]
    public GameObject hungerBar;
    public GameObject hungerText;

    private float drainTimer;
    private float starvationTimer;

    private Image hungerBarImage;
    private TMP_Text hungerTextComponent;

    private void Awake()
    {
        hungerBarImage = hungerBar.GetComponent<Image>();
        hungerTextComponent = hungerText.GetComponent<TMP_Text>();

        hungerTextComponent.horizontalAlignment =
            HorizontalAlignmentOptions.Center;

        UpdateHungerUI();
    }

    private void Update()
    {
        DrainHunger();
        HandleStarvation();
    }

    private void DrainHunger()
    {
        drainTimer += Time.deltaTime;

        if (drainTimer >= drainInterval)
        {
            currentHunger -= hungerDrainRate;

            currentHunger = Mathf.Clamp(
                currentHunger,
                0f,
                maxHunger
            );

            drainTimer = 0f;

            UpdateHungerUI();
        }
    }

    private void HandleStarvation()
    {
        if (currentHunger <= 0f && healthController != null)
        {
            starvationTimer += Time.deltaTime;

            if (starvationTimer >= 1f)
            {
                float damage =
                    healthController.maxHealth *
                    starvationDamagePercent;

                healthController.TakeDamage(damage);

                starvationTimer = 0f;
            }
        }
        else
        {
            starvationTimer = 0f;
        }
    }

    // RESTORE HUNGER
    public void RestoreHunger(float amount)
    {
        currentHunger += amount;

        currentHunger = Mathf.Clamp(
            currentHunger,
            0f,
            maxHunger
        );

        UpdateHungerUI();
    }

    // RESTORE HEALTH
    public void RestoreHealth(float amount)
    {
        if (amount <= 0f)
            return;

        if (healthController == null)
        {
            Debug.LogWarning(
                "HungerController: HealthController is not assigned!"
            );

            return;
        }

        healthController.Heal(amount);
    }

    // ADD SATURATION
    public void AddSaturation(float amount)
    {
        currentSaturation += amount;

        currentSaturation = Mathf.Clamp(
            currentSaturation,
            0f,
            maxSaturation
        );

        Debug.Log(
            "Saturation increased by " +
            amount +
            ". Current saturation: " +
            currentSaturation
        );
    }

    private void UpdateHungerUI()
    {
        if (hungerBarImage != null)
        {
            hungerBarImage.fillAmount =
                currentHunger / maxHunger;
        }

        if (hungerTextComponent != null)
        {
            hungerTextComponent.text =
                Mathf.CeilToInt(currentHunger) +
                " / " +
                Mathf.CeilToInt(maxHunger);
        }
    }

    public bool IsStarving()
    {
        return currentHunger <= 0f;
    }
}