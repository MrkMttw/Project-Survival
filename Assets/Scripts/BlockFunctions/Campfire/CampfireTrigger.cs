using UnityEngine;

public class CampfireTrigger : MonoBehaviour
{
    private CampfireController campfireController;

    private void Awake()
    {
        campfireController = FindFirstObjectByType<CampfireController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HealthController health =
            other.transform.root.GetComponentInChildren<HealthController>();

        if (health != null)
        {
            campfireController.PlayerEntered(health);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        HealthController health =
            other.transform.root.GetComponentInChildren<HealthController>();

        if (health != null)
        {
            campfireController.PlayerExited(health);
        }
    }
}