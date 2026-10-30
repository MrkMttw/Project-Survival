using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class GateBehavior : MonoBehaviour
{
    [Header("Gate References")]
    public SpriteRenderer parentRenderer;
    public CapsuleCollider2D capsuleCollider;
    public GameObject openGatePrefab;

    private BoxCollider2D interactionCollider;
    private Color originalColor;

    private bool isOpen;

    private void Awake()
    {
        interactionCollider = GetComponent<BoxCollider2D>();

        if (parentRenderer == null)
            parentRenderer = GetComponent<SpriteRenderer>();

        if (capsuleCollider == null)
            capsuleCollider = GetComponent<CapsuleCollider2D>();

        if (parentRenderer != null)
            originalColor = parentRenderer.color;

        SetGateState(false);
    }

    public bool IsPlayerInRange(Vector2 playerPosition)
    {
        return interactionCollider != null &&
               interactionCollider.OverlapPoint(playerPosition);
    }

    public void ToggleGate()
    {
        SetGateState(!isOpen);
    }

    private void SetGateState(bool open)
    {
        isOpen = open;

        if (openGatePrefab != null)
            openGatePrefab.SetActive(isOpen);

        if (parentRenderer != null)
        {
            Color color = originalColor;
            color.a = isOpen ? 0f : originalColor.a;
            parentRenderer.color = color;
        }

        if (capsuleCollider != null)
            capsuleCollider.enabled = !isOpen;
    }
}