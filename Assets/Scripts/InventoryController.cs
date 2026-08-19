using UnityEngine;

/// <summary>
/// Controls the initialization and setup of the inventory system.
/// Creates inventory slots and populates them with items at startup.
/// </summary>
public class InventoryController : MonoBehaviour
{
    /// <summary>
    /// The parent panel that contains all inventory slots.
    /// </summary>
    public GameObject inventoryPanel;

    /// <summary>
    /// The prefab used to create individual inventory slots.
    /// </summary>
    public GameObject slotPrefab;

    /// <summary>
    /// The total number of slots to create in the inventory.
    /// </summary>
    public int slotCount;

    /// <summary>
    /// Array of item prefabs to populate into the inventory slots.
    /// Items are assigned sequentially to slots.
    /// </summary>
    public GameObject[] itemPrefabs;

    /// <summary>
    /// Initializes the inventory by creating slots and populating them with items.
    /// Called automatically when the script starts.
    /// </summary>
    void Start()
    {
        for (int i = 0; i < slotCount; i++)
        {
            Slot slot = Instantiate(slotPrefab, inventoryPanel.transform).GetComponent<Slot>();

            if (i < itemPrefabs.Length)
            {
                GameObject item = Instantiate(itemPrefabs[i], slot.transform);

                item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                slot.currentItem = item;
            }
        }
    }
}