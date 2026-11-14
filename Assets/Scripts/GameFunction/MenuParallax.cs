using UnityEngine;
using UnityEngine.InputSystem;

public class MenuParallax : MonoBehaviour
{
    [Header("PARALLAX SETTINGS")]
    public float offsetMultiplier = 20f;
    public float smoothTime = 0.3f;

    private RectTransform rectTransform;
    private RectTransform parentRect;

    private Vector2 startPosition;
    private Vector2 velocity;

    private Canvas canvas;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        parentRect = rectTransform.parent as RectTransform;

        canvas = GetComponentInParent<Canvas>();

        startPosition = rectTransform.anchoredPosition;
    }

    private void Update()
    {
        if (Mouse.current == null || parentRect == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            mousePosition,
            uiCamera,
            out Vector2 localMousePosition))
        {
            return;
        }

        Vector2 rectSize = parentRect.rect.size;

        // Calculate the actual center of the parent rectangle
        Vector2 centerOffset = new Vector2(
            (0.5f - parentRect.pivot.x) * rectSize.x,
            (0.5f - parentRect.pivot.y) * rectSize.y
        );

        Vector2 normalizedPosition = new Vector2(
            (localMousePosition.x - centerOffset.x) / (rectSize.x * 0.5f),
            (localMousePosition.y - centerOffset.y) / (rectSize.y * 0.5f)
        );

        // Prevent excessive movement
        normalizedPosition = Vector2.ClampMagnitude(normalizedPosition, 1f);

        Vector2 targetPosition = startPosition +
            normalizedPosition * offsetMultiplier;

        rectTransform.anchoredPosition = Vector2.SmoothDamp(
            rectTransform.anchoredPosition,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }
}