using System.Collections;
using UnityEngine;

public class HitFlashBehavior : MonoBehaviour
{
    [Header("Hit Flash Settings")]
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.15f;

    private SpriteRenderer spriteRenderer;
    private Coroutine flashCoroutine;

    private Color originalColor;

    private void Awake()
    {
        // GET ROOT SPRITE RENDERER
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogWarning(
                "HitFlashController: Root SpriteRenderer is missing!",
                gameObject
            );

            return;
        }

        originalColor = spriteRenderer.color;
    }

    public void PlayHitFlash()
    {
        if (spriteRenderer == null)
            return;

        // RESTART FLASH IF HIT AGAIN
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        spriteRenderer.color = flashColor;

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        yield return new WaitForSeconds(flashDuration);

        // RESTORE ORIGINAL COLOR
        spriteRenderer.color = originalColor;

        flashCoroutine = null;
    }
}