using UnityEngine;

public class PlayerHeldItem : MonoBehaviour
{
    [Header("Held Item Visual")]
    public SpriteRenderer heldItemRenderer;

    [Header("Currently Held Item")]
    public Item heldItem;

    [Header("Placement")]
    public PlacementController placementController;

    private void Awake()
    {
        // Automatically find PlacementController if not assigned.
        if (placementController == null)
        {
            placementController =
                FindFirstObjectByType<PlacementController>();
        }
    }

    public void SetHeldItem(Item item)
    {
        // If the player is switching away from the currently held item,
        // cancel any active building placement first.
        if (heldItem != null && heldItem != item)
        {
            CancelPlacement();
        }

        heldItem = item;

        if (item == null)
        {
            ClearHeldItem();
            return;
        }

        SpriteRenderer itemSprite =
            item.GetComponentInChildren<SpriteRenderer>();

        if (itemSprite != null)
        {
            heldItemRenderer.sprite = itemSprite.sprite;
            heldItemRenderer.enabled = true;
        }
        else
        {
            Debug.LogWarning(
                "PlayerHeldItem: No SpriteRenderer found on " + item.name
            );

            ClearHeldItem();
        }
    }

    public void ClearHeldItem()
    {
        // IMPORTANT:
        // Cancel building placement before clearing the held item.
        CancelPlacement();

        heldItem = null;

        if (heldItemRenderer != null)
        {
            heldItemRenderer.sprite = null;
            heldItemRenderer.enabled = false;
        }
    }

    private void CancelPlacement()
    {
        if (placementController != null)
        {
            placementController.CancelPlacement();
        }
    }

    public Item GetHeldItem()
    {
        return heldItem;
    }
}