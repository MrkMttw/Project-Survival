using UnityEngine;

public class CraftingTableTrigger : MonoBehaviour
{
    private CraftingController craftingController;

    private void Awake()
    {
        craftingController =
            FindFirstObjectByType<CraftingController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (craftingController != null)
            craftingController.SetCraftingTableAccess(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (craftingController != null)
            craftingController.SetCraftingTableAccess(false);
    }
}