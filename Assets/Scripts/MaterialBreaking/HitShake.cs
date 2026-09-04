using UnityEngine;
using System.Collections;

public class HitShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public float shakeDuration = 0.18f;
    public float shakeAngle = 8f;

    private Quaternion originalRotation;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        originalRotation = transform.localRotation;
    }

    public void Shake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        shakeCoroutine = StartCoroutine(ShakeAnimation());
    }

    private IEnumerator ShakeAnimation()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float progress = elapsed / shakeDuration;

            // Smoothly go back toward the original rotation
            float falloff = 1f - progress;

            // Sine wave creates a swaying motion
            float angle =
                Mathf.Sin(progress * Mathf.PI * 3f)
                * shakeAngle
                * falloff;

            transform.localRotation =
                originalRotation *
                Quaternion.Euler(0f, 0f, angle);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localRotation = originalRotation;
        shakeCoroutine = null;
    }
}