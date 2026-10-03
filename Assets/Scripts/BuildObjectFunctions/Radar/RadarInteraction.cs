using UnityEngine;

public class RadarInteraction : MonoBehaviour
{
    [Header("Iron")]
    public int ironItemID;

    private RadarController radarController;
    private bool playerInRange = false;

    private void Awake()
    {
        radarController =
            FindFirstObjectByType<RadarController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    public bool CanFeedIron(Item item)
    {
        if (!playerInRange)
            return false;

        if (item == null)
            return false;

        if (radarController == null)
            return false;

        if (radarController.RadarLevel >= 5)
            return false;

        return item.ID == ironItemID;
    }

    public void AddIron(int amount)
    {
        if (radarController == null)
            return;

        radarController.AddIron(amount);
    }
}