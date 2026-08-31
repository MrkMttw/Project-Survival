using UnityEngine;

public class PlayerItemCollector : MonoBehaviour
{
    private InventoryController inventoryController;
    private HotbarController hotbarController;

    [Header("Pickup Settings")]
    public float pickupRadius = 1.5f;

    [Header("Pickup UI")]
    public GameObject pickUp;

    private GameObject nearbyItem;

    private void Start()
    {
        inventoryController = FindObjectOfType<InventoryController>();
        hotbarController = FindObjectOfType<HotbarController>();

        if (pickUp != null)
            pickUp.SetActive(false);
    }

    private void Update()
    {
        FindNearbyItem();

        if (nearbyItem != null && Input.GetKeyDown(KeyCode.F))
        {
            PickupItem();
        }
    }

    private void FindNearbyItem()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            transform.position,
            pickupRadius
        );

        GameObject closestItem = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D collider in colliders)
        {
            Item item = collider.GetComponentInParent<Item>();

            if (item == null)
                continue;

            float distance = Vector2.Distance(
                transform.position,
                item.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestItem = item.gameObject;
            }
        }

        nearbyItem = closestItem;

        if (pickUp != null)
            pickUp.SetActive(nearbyItem != null);
    }

    private void PickupItem()
    {
        if (nearbyItem == null)
            return;

        Item item = nearbyItem.GetComponent<Item>();

        if (item == null)
            return;

        bool itemAdded = false;

        // Try Hotbar first
        if (hotbarController != null)
        {
            itemAdded = hotbarController.AddItem(nearbyItem);
        }

        // Try Inventory if Hotbar is full
        if (!itemAdded && inventoryController != null)
        {
            itemAdded = inventoryController.AddItem(nearbyItem);
        }

        // Only destroy if successfully collected
        if (itemAdded)
        {
            Destroy(nearbyItem);

            nearbyItem = null;

            if (pickUp != null)
                pickUp.SetActive(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}