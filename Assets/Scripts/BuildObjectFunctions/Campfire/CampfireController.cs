using UnityEngine;

public class CampfireController : MonoBehaviour
{
    [Header("Healing")]
    [Range(0f, 100f)]
    public float healPercentPerSecond = 1f;

    private HealthController playerHealth;
    private bool playerInRange = false;

    private bool isGhost;

    private void Update()
    {
        if (isGhost || !playerInRange || playerHealth == null)
            return;

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

    public void SetGhostMode(bool ghost)
    {
        isGhost = ghost;

        if (ghost)
        {
            playerHealth = null;
            playerInRange = false;
        }
    }
}