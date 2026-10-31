    using UnityEngine;

public class PlayerItemCollector : MonoBehaviour
{
    private InventoryController inventoryController;
    private HotbarController hotbarController;

    [Header("Pickup Settings")]
    public float pickupRadius = 1.5f;

    private GameObject nearbyItem;

    private void Start()
    {
        inventoryController = FindObjectOfType<InventoryController>();
        hotbarController = FindObjectOfType<HotbarController>();
    }

    private void Update()
    {
        FindNearbyItem();

        // UPDATE PICKUP PROMPT
        UpdatePickupPrompt();

        if (nearbyItem != null && Input.GetKeyDown(KeyCode.F))
        {
            PickupItem();
        }
    }

    // UPDATE PICKUP PROMPT
    private void UpdatePickupPrompt()
    {
        if (PromptController.Instance == null)
            return;

        if (nearbyItem != null)
        {
            PromptController.Instance.RequestPrompt(
                PromptType.F, this);
        }
        else
        {
            PromptController.Instance.ReleasePrompt(
                PromptType.F, this);
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
            return;

        // NON-STACKABLE ITEMS
        if (!item.stackable)
        {
            Destroy(nearbyItem);
            nearbyItem = null;
            return;
        }

        // STACKABLE ITEMS
        int quantityAfterPickup = item.quantity;

        // ALL ITEMS WERE COLLECTED
        if (quantityAfterPickup <= 0)
        {
            Destroy(nearbyItem);
            nearbyItem = null;
            return;
        }

        // UPDATE REMAINING STACK
        item.UpdateQuantityDisplay();

        Debug.Log(
            "Picked up " +
            (quantityBeforePickup - quantityAfterPickup) +
            " item(s). " +
            quantityAfterPickup +
            " remaining."
        );
    }

    private void OnDisable()
    {
        if (PromptController.Instance != null)
        {
            PromptController.Instance.ReleasePrompt(
                PromptType.F, this);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            pickupRadius
        );
    }
}