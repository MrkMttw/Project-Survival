using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CampfireTrigger : MonoBehaviour
{
    private CampfireController campfireController;
    private BoxCollider2D hitbox;
    private BuildingObject buildingObject;

    private void Awake()
    {
        hitbox = GetComponent<BoxCollider2D>();
        buildingObject = GetComponent<BuildingObject>();

        if (hitbox == null)
        {
            Debug.LogError("[CampfireTrigger] BoxCollider2D missing!");
            return;
        }

        if (buildingObject == null)
        {
            Debug.LogError("[CampfireTrigger] BuildingObject missing!");
            return;
        }

        hitbox.isTrigger = true;
    }

    private void Start()
    {
        campfireController =
            FindFirstObjectByType<CampfireController>();

        if (campfireController == null)
        {
            Debug.LogError("[CampfireTrigger] CampfireController NOT FOUND!");
            return;
        }

        campfireController.ConnectToBuildingSaveData(buildingObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (campfireController == null)
            return;

        HealthController health =
            other.transform.root.GetComponentInChildren<HealthController>();

        if (health == null)
        {
            Debug.LogError("[CampfireTrigger] HealthController NOT FOUND!");
            return;
        }

        campfireController.PlayerEntered(health);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (campfireController == null)
            return;

        if (!other.CompareTag("Player"))
            return;

        HealthController health =
            other.transform.root.GetComponentInChildren<HealthController>();

        if (health == null)
            return;

        campfireController.PlayerExited(health);
    }
}