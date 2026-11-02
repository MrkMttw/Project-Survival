using System.Collections;
using UnityEngine;

public class LootDropAnimation : MonoBehaviour
{
    [Header("Drop Animation")]
    [SerializeField] private float scatterDistance = 1.2f;
    [SerializeField] private float popHeight = 1.2f;
    [SerializeField] private float dropDuration = 0.35f;

    [Header("Landing Bounce")]
    [SerializeField] private float bounceHeight = 0.15f;
    [SerializeField] private float bounceDuration = 0.15f;

    public bool IsAnimating { get; private set; }

    public void Play()
    {
        if (IsAnimating)
            return;

        StartCoroutine(PlayDropAnimation());
    }

    private IEnumerator PlayDropAnimation()
    {
        IsAnimating = true;

        Vector3 startPosition = transform.position;

        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        Vector3 targetPosition = startPosition +
            (Vector3)(randomDirection * Random.Range(
                scatterDistance * 0.5f,
                scatterDistance
            ));

        // POP OUTWARD
        float elapsed = 0f;

        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / dropDuration);

            Vector3 position = Vector3.Lerp(
                startPosition,
                targetPosition,
                progress
            );

            position.y += Mathf.Sin(progress * Mathf.PI) * popHeight;

            transform.position = position;

            yield return null;
        }

        transform.position = targetPosition;

        // LANDING BOUNCE
        elapsed = 0f;

        while (elapsed < bounceDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / bounceDuration);

            Vector3 position = targetPosition;
            position.y += Mathf.Sin(progress * Mathf.PI) * bounceHeight;

            transform.position = position;

            yield return null;
        }

        transform.position = targetPosition;
        IsAnimating = false;
    }
}