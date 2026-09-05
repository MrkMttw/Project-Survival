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

        int quantityBeforePickup = item.quantity;

        bool itemAdded = false;

        // TRY HOTBAR FIRST
        if (hotbarController != null)
        {
            itemAdded = hotbarController.AddItem(nearbyItem);
        }

        // TRY INVENTORY IF HOTBAR COULDN'T TAKE IT
        if (!itemAdded && inventoryController != null)
        {
            itemAdded = inventoryController.AddItem(nearbyItem);
        }

        // NOTHING WAS COLLECTED
        if (!itemAdded)
        {
            return;
        }

        // NON-STACKABLE ITEMS
        // If successfully added, the ground item is completely picked up.
        if (!item.stackable)
        {
            Destroy(nearbyItem);

            nearbyItem = null;

            if (pickUp != null)
                pickUp.SetActive(false);

            return;
        }

        // STACKABLE ITEMS
        int quantityAfterPickup = item.quantity;

        // ALL STACKABLE ITEMS WERE COLLECTED
        if (quantityAfterPickup <= 0)
        {
            Destroy(nearbyItem);

            nearbyItem = null;

            if (pickUp != null)
                pickUp.SetActive(false);

            return;
        }

        // ONLY PART OF THE STACK WAS COLLECTED
        item.UpdateQuantityDisplay();

        Debug.Log(
            "Picked up " +
            (quantityBeforePickup - quantityAfterPickup) +
            " item(s). " +
            quantityAfterPickup +
            " remaining."
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            pickupRadius
        );
    }
}