using UnityEngine;

public class MenuParallax : MonoBehaviour
{
    public float offsetMultiplier = 1f;
    public float smoothTime = 0.3f;

    private Vector3 startPosition;
    private Vector3 velocity;

    private void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);

        Vector2 offset = mousePosition - new Vector3(1f, 1f);

        Vector3 targetPosition = startPosition + (Vector3)offset * offsetMultiplier;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }
}