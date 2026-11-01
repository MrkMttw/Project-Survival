using UnityEngine;

public class RadarInteraction : MonoBehaviour
{
    private RadarController radarController;
    private bool playerInRange = false;

    private void Awake()
    {
        radarController =
            FindFirstObjectByType<RadarController>();
    }

    private void Start()
    {
        if (radarController != null)
            radarController.RegisterRadarPlaced();
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
        if (!playerInRange)
            return false;

        if (radarController == null)
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