using UnityEngine;

public class PlayerHeldItem : MonoBehaviour
{
    public SpriteRenderer heldItemRenderer;

    public void SetHeldItem(Sprite sprite)
    {
        if (sprite == null)
        {
            ClearHeldItem();
            return;
        }

        heldItemRenderer.sprite = sprite;
        heldItemRenderer.enabled = true;
    }

    public void ClearHeldItem()
    {
        heldItemRenderer.sprite = null;
        heldItemRenderer.enabled = false;
    }
}