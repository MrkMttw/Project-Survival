using UnityEngine;

public class CampfireTrigger : MonoBehaviour
{
    private CampfireController campfireController;

    private void Awake()
    {
        campfireController =
            FindFirstObjectByType<CampfireController>();

        if (campfireController == null)
        {
            Debug.LogError(
                "CampfireTrigger could not find CampfireController."
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (campfireController == null)
            return;

        HealthController health =
            other.transform.root.GetComponentInChildren<HealthController>();

        if (health != null)
        {
            campfireController.PlayerEntered(health);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (campfireController == null)
            return;

        HealthController health =
            other.transform.root.GetComponentInChildren<HealthController>();

        if (health != null)
        {
            campfireController.PlayerExited(health);
        }
    }
}