using UnityEngine;

public class RadarInteraction : MonoBehaviour
{
    private RadarController radarController;
    private BuildingObject buildingObject;

    private bool playerInRange = false;

    private void Awake()
    {
        buildingObject = GetComponent<BuildingObject>();

        if (buildingObject == null)
            buildingObject = GetComponentInParent<BuildingObject>();
    }

    private void Start()
    {
        radarController = FindFirstObjectByType<RadarController>();

        if (radarController == null)
        {
            Debug.LogError("[RadarInteraction] RadarController NOT FOUND!");
            return;
        }

        if (buildingObject == null)
        {
            Debug.LogError("[RadarInteraction] BuildingObject NOT FOUND!");
            return;
        }

        radarController.ConnectToBuildingSaveData(buildingObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    public bool CanFeedIron(Item item)
    {
        if (!playerInRange || radarController == null)
            return false;

        if (radarController.RadarLevel >= 5)
            return false;

        return radarController.IsIronItem(item);
    }

    public void AddIron(int amount)
    {
        if (radarController == null)
            return;

        radarController.AddIron(amount);
    }
}