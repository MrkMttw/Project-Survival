using UnityEngine;
using System.Collections;

public class HitShake : MonoBehaviour
{
    [Header("Shadow")]
    public Transform shadow;

    [Header("Shake Settings")]
    public float shakeDuration = 0.18f;
    public float shakeAngle = 8f;

    private Quaternion originalRotation;
    private Quaternion shadowOriginalRotation;

    private Coroutine shakeCoroutine;

    private void Awake()
    {
        originalRotation = transform.localRotation;

        if (shadow != null)
        {
            shadowOriginalRotation = shadow.localRotation;
        }
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
            float falloff = 1f - progress;

            float angle =
                Mathf.Sin(progress * Mathf.PI * 3f)
                * shakeAngle
                * falloff;

            // Shake the Tree / Stone
            transform.localRotation =
                originalRotation *
                Quaternion.Euler(0f, 0f, angle);

            // Counter-rotate the shadow
            if (shadow != null)
            {
                shadow.localRotation =
                    shadowOriginalRotation *
                    Quaternion.Euler(0f, 0f, -angle);
            }

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localRotation = originalRotation;

        if (shadow != null)
        {
            shadow.localRotation = shadowOriginalRotation;
        }

        shakeCoroutine = null;
    }
}