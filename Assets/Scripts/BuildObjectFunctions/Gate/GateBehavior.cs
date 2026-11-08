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

    public bool IsOpen => isOpen;

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

    // CHECK PLAYER RANGE
    public bool IsPlayerInRange(Vector2 playerPosition)
    {
        return interactionCollider != null &&
               interactionCollider.OverlapPoint(playerPosition);
    }

    // TOGGLE GATE
    public void ToggleGate()
    {
        SetGateState(!isOpen);
        UpdateSavedGateState();
    }

    // SET GATE STATE
    public void SetGateState(bool open)
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

    // UPDATE SAVED GATE STATE
    private void UpdateSavedGateState()
    {
        WorldGenerator worldGenerator =
            FindFirstObjectByType<WorldGenerator>();

        if (worldGenerator == null)
            return;

        BuildingObject building =
            GetComponentInParent<BuildingObject>();

        if (building == null)
            building = GetComponentInChildren<BuildingObject>(true);

        if (building == null)
            return;

        if (worldGenerator.TryGetBuildingSaveData(
            building, out BuildingSaveData data))
        {
            data.hasGateState = true;
            data.gateIsOpen = isOpen;
        }
    }
}