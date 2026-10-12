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

    [Header("Health UI")]
    public GameObject healthBar;
    public GameObject healthText;

    private Image healthBarImage;
    private TMP_Text healthTextComponent;
    private bool deathTriggered;

    public event Action OnDeath;

    private void Awake()
    {
        healthBarImage = healthBar.GetComponent<Image>();
        healthTextComponent = healthText.GetComponent<TMP_Text>();

        healthTextComponent.horizontalAlignment =
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
        if (healthBarImage != null)
            healthBarImage.fillAmount = currentHealth / maxHealth;

        if (healthTextComponent != null)
        {
            healthTextComponent.text =
                Mathf.CeilToInt(currentHealth) + " / " +
                Mathf.CeilToInt(maxHealth);
        }
    }

    public bool IsDead()
    {
        return currentHealth <= 0f;
    }
}