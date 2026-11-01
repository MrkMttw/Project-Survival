using System.Collections;
using UnityEngine;

public class PlayerHeldItem : MonoBehaviour
{
    [Header("Held Item Visual")]
    public SpriteRenderer heldItemRenderer;

    [Header("Directional Adjustment")]
    [SerializeField] private float rightOffset = 0.5f;
    [SerializeField] private float leftOffset = 0.5f;
    [SerializeField] private float downOffset = 0.5f;

    [Header("Swing Animation")]
    [SerializeField] private float swingAngle = 70f;
    [SerializeField] private float swingDuration = 0.12f;
    [SerializeField] private float returnDuration = 0.10f;

    [Header("Currently Held Item")]
    public Item heldItem;

    private Animator animator;
    private Vector3 defaultLocalPosition;
    private Quaternion defaultLocalRotation;

    private Coroutine swingCoroutine;

    private void Awake()
    {
        animator = GetComponentInParent<Animator>();

        if (heldItemRenderer != null)
        {
            defaultLocalPosition =
                heldItemRenderer.transform.localPosition;

            defaultLocalRotation =
                heldItemRenderer.transform.localRotation;
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

    // PLAY SWING ANIMATION
    public void PlaySwing()
    {
        if (heldItemRenderer == null || heldItem == null)
            return;

        if (animator == null)
            return;

        if (swingCoroutine != null)
        {
            StopCoroutine(swingCoroutine);
        }

        swingCoroutine = StartCoroutine(SwingAnimation());
    }

    private IEnumerator SwingAnimation()
    {
        float inputX = animator.GetFloat("InputX");
        float inputY = animator.GetFloat("InputY");

        // USE LAST FACING DIRECTION WHEN IDLE
        if (Mathf.Approximately(inputX, 0f) &&
            Mathf.Approximately(inputY, 0f))
        {
            inputX = animator.GetFloat("LastInputX");
            inputY = animator.GetFloat("LastInputY");
        }

        // MIRROR SWING DIRECTION WHEN FACING LEFT
        float direction = inputX < 0f ? -1f : -1f;

        Transform weaponTransform = heldItemRenderer.transform;

        Quaternion startRotation =
            defaultLocalRotation *
            Quaternion.Euler(0f, 0f, -swingAngle * direction);

        Quaternion endRotation =
            defaultLocalRotation *
            Quaternion.Euler(0f, 0f, swingAngle * direction);

        // SWING FORWARD
        float elapsed = 0f;

        while (elapsed < swingDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / swingDuration
            );

            weaponTransform.localRotation = Quaternion.Slerp(
                startRotation,
                endRotation,
                progress
            );

            yield return null;
        }

        // RETURN TO ORIGINAL ROTATION
        elapsed = 0f;

        Quaternion currentRotation = weaponTransform.localRotation;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / returnDuration
            );

            weaponTransform.localRotation = Quaternion.Slerp(
                currentRotation,
                defaultLocalRotation,
                progress
            );

            yield return null;
        }

        weaponTransform.localRotation = defaultLocalRotation;

        swingCoroutine = null;
    }

    public void SetHeldItem(Item item)
    {
        // STOP PREVIOUS SWING
        StopSwing();

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
            return;
        }
    }

    // STOP SWING
    private void StopSwing()
    {
        if (swingCoroutine != null)
        {
            StopCoroutine(swingCoroutine);
            swingCoroutine = null;
        }

        if (heldItemRenderer != null)
        {
            heldItemRenderer.transform.localRotation =
                defaultLocalRotation;
        }
    }

    public void ClearHeldItem()
    {
        StopSwing();

        heldItem = null;

        if (heldItemRenderer != null)
        {
            heldItemRenderer.sprite = null;
            heldItemRenderer.enabled = false;
            heldItemRenderer.flipX = false;

            heldItemRenderer.transform.localPosition =
                defaultLocalPosition;

            heldItemRenderer.transform.localRotation =
                defaultLocalRotation;
        }
    }

    public Item GetHeldItem()
    {
        return heldItem;
    }
}