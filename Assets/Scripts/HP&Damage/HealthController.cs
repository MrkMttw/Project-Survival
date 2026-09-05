using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthController : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("Health UI")]
    public GameObject healthBar;
    public GameObject healthText;

    private Image healthBarImage;
    private TMP_Text healthTextComponent;

    private void Awake()
    {
        healthBarImage = healthBar.GetComponent<Image>();
        healthTextComponent = healthText.GetComponent<TMP_Text>();

        healthTextComponent.horizontalAlignment = HorizontalAlignmentOptions.Center;

        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateHealthUI();

        Debug.Log("Health: " + currentHealth);
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateHealthUI();

        Debug.Log("Health restored: " + amount);
    }

    private void UpdateHealthUI()
    {
        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = currentHealth / maxHealth;
        }

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