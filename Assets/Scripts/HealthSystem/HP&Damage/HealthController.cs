using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthController : MonoBehaviour
{
    [Header("Health")]
    public float baseMaxHealth = 100f;
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("PC Health UI")]
    public GameObject healthBar;
    public GameObject healthText;

    [Header("Mobile Health UI")]
    public GameObject mobileHealthBar;
    public GameObject mobileHealthText;

    public event Action OnDeath;

    private Image healthBarImage;
    private TMP_Text healthTextComponent;

    private Image mobileHealthBarImage;
    private TMP_Text mobileHealthTextComponent;

    private bool deathTriggered;
    private HitFlashBehavior hitFlash;

    private void Awake()
    {
        hitFlash = GetComponentInParent<HitFlashBehavior>();

        // PC UI
        if (healthBar != null)
            healthBarImage = healthBar.GetComponent<Image>();

        if (healthText != null)
            healthTextComponent = healthText.GetComponent<TMP_Text>();

        // MOBILE UI
        if (mobileHealthBar != null)
            mobileHealthBarImage = mobileHealthBar.GetComponent<Image>();

        if (mobileHealthText != null)
            mobileHealthTextComponent = mobileHealthText.GetComponent<TMP_Text>();

        if (healthTextComponent != null)
            healthTextComponent.horizontalAlignment =
                HorizontalAlignmentOptions.Center;

        if (mobileHealthTextComponent != null)
            mobileHealthTextComponent.horizontalAlignment =
                HorizontalAlignmentOptions.Center;

        maxHealth = baseMaxHealth;
        currentHealth = maxHealth;

        UpdateHealthUI();
    }

    public void UpdateMaxHealth(int campfireLevel)
    {
        maxHealth = baseMaxHealth + (campfireLevel * 20f);
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateHealthUI();
    }

    public void TakeDamage(float amount)
    {
        if (deathTriggered)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        // PLAY HIT FLASH
        if (amount > 0f)
            hitFlash?.PlayHitFlash();

        UpdateHealthUI();

        Debug.Log("Health: " + currentHealth);

        if (currentHealth <= 0f)
        {
            deathTriggered = true;
            OnDeath?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (deathTriggered || currentHealth >= maxHealth)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateHealthUI();
    }

    public void RespawnHealth()
    {
        currentHealth = maxHealth;
        deathTriggered = false;

        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        float healthPercentage = currentHealth / maxHealth;

        // PC UI
        if (healthBarImage != null)
            healthBarImage.fillAmount = healthPercentage;

        if (healthTextComponent != null)
        {
            healthTextComponent.text =
                Mathf.CeilToInt(currentHealth) + " / " +
                Mathf.CeilToInt(maxHealth);
        }

        // MOBILE UI
        if (mobileHealthBarImage != null)
            mobileHealthBarImage.fillAmount = healthPercentage;

        if (mobileHealthTextComponent != null)
        {
            mobileHealthTextComponent.text =
                Mathf.CeilToInt(currentHealth) + " / " +
                Mathf.CeilToInt(maxHealth);
        }
    }

    public bool IsDead()
    {
        return currentHealth <= 0f;
    }

    // RESTORE SAVED HEALTH
    public void RestoreSavedHealth(float savedHealth)
    {
        currentHealth = Mathf.Clamp(savedHealth, 0f, maxHealth);

        UpdateHealthUI();
    }
}