using UnityEngine;

public class PlayerItemCollector : MonoBehaviour
{
    private InventoryController inventoryController;

    public GameObject pickUp;

    private GameObject nearbyItem;

    private void Start()
    {
        inventoryController = FindObjectOfType<InventoryController>();

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
                bool itemAdded = inventoryController.AddItem(nearbyItem);

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