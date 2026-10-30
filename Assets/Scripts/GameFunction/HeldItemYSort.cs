using UnityEngine;

public class HeldItemYSort : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private SpriteRenderer heldItemRenderer;

    [Header("Sorting")]
    [SerializeField] private int sortingOffset = 1;

    void LateUpdate()
    {
        if (playerSpriteRenderer == null || heldItemRenderer == null)
            return;

        heldItemRenderer.sortingLayerID =
            playerSpriteRenderer.sortingLayerID;

        heldItemRenderer.sortingOrder =
            playerSpriteRenderer.sortingOrder + sortingOffset;
    }
}