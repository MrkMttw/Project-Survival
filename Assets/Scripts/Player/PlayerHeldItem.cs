using UnityEngine;

public class PlayerHeldItem : MonoBehaviour
{
    [Header("Held Item Visual")]
    public SpriteRenderer heldItemRenderer;

    [Header("Directional Adjustment")]
    [SerializeField] private float rightOffset = 0.5f;
    [SerializeField] private float leftOffset = 0.5f;
    [SerializeField] private float downOffset = 0.5f;

    [Header("Currently Held Item")]
    public Item heldItem;

    private Animator animator;
    private Vector3 defaultLocalPosition;

    private void Awake()
    {
        animator = GetComponentInParent<Animator>();

        if (heldItemRenderer != null)
        {
            defaultLocalPosition = heldItemRenderer.transform.localPosition;
        }
    }

    private void LateUpdate()
    {
        if (animator == null || heldItemRenderer == null)
            return;

        float inputX = animator.GetFloat("InputX");
        float inputY = animator.GetFloat("InputY");

        // USE LAST FACING DIRECTION WHEN IDLE
        if (Mathf.Approximately(inputX, 0f) &&
            Mathf.Approximately(inputY, 0f))
        {
            inputX = animator.GetFloat("LastInputX");
            inputY = animator.GetFloat("LastInputY");
        }

        UpdateHeldItemDirection(inputX, inputY);
    }

    private void UpdateHeldItemDirection(float x, float y)
    {
        if (heldItem == null)
        {
            heldItemRenderer.enabled = false;
            return;
        }

        // HIDE WHEN FACING STRAIGHT UP
        if (y > 0 && Mathf.Approximately(x, 0f))
        {
            heldItemRenderer.enabled = false;
            return;
        }

        heldItemRenderer.enabled = true;

        // DOWN
        if (y < 0 && Mathf.Approximately(x, 0f))
        {
            heldItemRenderer.flipX = false;

            heldItemRenderer.transform.localPosition = new Vector3(
                downOffset,
                defaultLocalPosition.y,
                defaultLocalPosition.z
            );

            return;
        }

        // RIGHT
        if (x > 0)
        {
            heldItemRenderer.flipX = true;

            heldItemRenderer.transform.localPosition =
                defaultLocalPosition + Vector3.right * rightOffset;
        }

        // LEFT
        else if (x < 0)
        {
            heldItemRenderer.flipX = false;

            heldItemRenderer.transform.localPosition =
                defaultLocalPosition + Vector3.left * leftOffset;
        }
    }

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
            heldItemRenderer.flipX = false;
            heldItemRenderer.transform.localPosition = defaultLocalPosition;
        }
    }

    public Item GetHeldItem()
    {
        return heldItem;
    }
}