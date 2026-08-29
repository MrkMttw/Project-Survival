using UnityEngine;

public class PlayerItemCollector : MonoBehaviour
{
    private InventoryController inventoryController;

    // CHANGED: Added HotbarController reference
    private HotbarController hotbarController;

    public GameObject pickUp;

    private GameObject nearbyItem;

    private void Start()
    {
        inventoryController = FindObjectOfType<InventoryController>();

        // CHANGED: Find the HotbarController
        hotbarController = FindObjectOfType<HotbarController>();

        pickUp.SetActive(false);
    }

    private void Update()
    {
        // If there is an item nearby and E is pressed
        if (nearbyItem != null && Input.GetKeyDown(KeyCode.E))
        {
            Item item = nearbyItem.GetComponent<Item>();

            if (item != null)
            {
                // CHANGED: Try adding the item to the hotbar FIRST
                bool itemAdded = hotbarController.AddItem(nearbyItem);

                // CHANGED: If hotbar is full, try the inventory instead
                if (!itemAdded)
                {
                    itemAdded = inventoryController.AddItem(nearbyItem);
                }

                if (itemAdded)
                {
                    Destroy(nearbyItem);

                    nearbyItem = null;
                    pickUp.SetActive(false);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Item"))
        {
            nearbyItem = collision.gameObject;
            pickUp.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Item"))
        {
            // Only clear it if we're leaving the item we're currently targeting
            if (collision.gameObject == nearbyItem)
            {
                nearbyItem = null;
                pickUp.SetActive(false);
            }
        }
    }
}