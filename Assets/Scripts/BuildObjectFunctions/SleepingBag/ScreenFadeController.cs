using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFadeController : MonoBehaviour
{
    [Header("Fade Settings")]
    public Image fadePanel;
    public float fadeDuration = 1f;

    private void Awake()
    {
        if (fadePanel == null)
        {
            Debug.LogError(
                "ScreenFadeController: Fade Panel is not assigned."
            );
            return;
        }

        SetAlpha(0f);
    }

    public IEnumerator FadeOut()
    {
        yield return FadeTo(1f);
    }

    public IEnumerator FadeIn()
    {
        yield return FadeTo(0f);
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        if (fadePanel == null)
            yield break;

        Color color = fadePanel.color;
        float startAlpha = color.a;
        float elapsed = 0f;
        float duration = Mathf.Max(0f, fadeDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                Mathf.Clamp01(elapsed / duration)
            );

            SetAlpha(alpha);

            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    public void SetImmediate(float alpha)
    {
        SetAlpha(Mathf.Clamp01(alpha));
    }

    private void SetAlpha(float alpha)
    {
        if (fadePanel == null)
            return;

        Color color = fadePanel.color;
        color.a = alpha;
        fadePanel.color = color;
    }
}