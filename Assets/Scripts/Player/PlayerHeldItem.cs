using UnityEngine;

public class PlayerHeldItem : MonoBehaviour
{
    [Header("Held Item Visual")]
    public SpriteRenderer heldItemRenderer;

    [Header("Currently Held Item")]
    public Item heldItem;

    public void SetHeldItem(Item item)
    {
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
        heldItem = null;

        if (heldItemRenderer != null)
        {
            heldItemRenderer.sprite = null;
            heldItemRenderer.enabled = false;
        }
    }

    public Item GetHeldItem()
    {
        return heldItem;
    }
}