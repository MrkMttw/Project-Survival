using UnityEngine;

public class CampfireController : MonoBehaviour
{
    [Header("Healing")]
    [Range(0f, 100f)]
    public float healPercentPerSecond = 1f;

    private HealthController playerHealth;
    private bool playerInRange = false;

    private void Update()
    {
        if (!playerInRange || playerHealth == null)
            return;

        // Heal 1% of the player's MAX HP per second
        float healAmount =
            playerHealth.maxHealth *
            (healPercentPerSecond / 100f) *
            Time.deltaTime;

        playerHealth.Heal(healAmount);
    }

    public void PlayerEntered(HealthController health)
    {
        if (health == null)
            return;

        playerHealth = health;
        playerInRange = true;

        Debug.Log("Player entered campfire healing range.");
    }

    public void PlayerExited(HealthController health)
    {
        if (health == null)
            return;

        if (playerHealth == health)
        {
            playerHealth = null;
            playerInRange = false;

            Debug.Log("Player left campfire healing range.");
        }
    }
}