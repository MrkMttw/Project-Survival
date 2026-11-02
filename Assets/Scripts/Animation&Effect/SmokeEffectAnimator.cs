using System.Collections;
using UnityEngine;

public class SmokeEffectAnimator : MonoBehaviour
{
    [Header("Smoke References")]
    [SerializeField] private GameObject smokeObject;
    [SerializeField] private Animator smokeAnimator;

    [Header("Animation Settings")]
    [SerializeField] private string smokeTrigger = "Play";
    [SerializeField] private float smokeDuration = 0.5f;

    private Coroutine smokeCoroutine;

    public float SmokeDuration => smokeDuration;

    private void Awake()
    {
        if (smokeObject != null)
            smokeObject.SetActive(false);
    }

    // PLAY NORMAL SMOKE
    public void PlaySmoke()
    {
        if (smokeObject == null || smokeAnimator == null)
        {
            Debug.LogWarning("SmokeEffectAnimator: Missing references!");
            return;
        }

        if (smokeCoroutine != null)
            StopCoroutine(smokeCoroutine);

        smokeObject.SetActive(true);

        smokeAnimator.ResetTrigger(smokeTrigger);
        smokeAnimator.SetTrigger(smokeTrigger);

        smokeCoroutine = StartCoroutine(StopSmoke());
    }

    /// PLAY DETACHED SMOKE FOR BREAKABLE OBJECTS
    public GameObject PlaySmokeDetached()
    {
        if (smokeObject == null || smokeAnimator == null)
        {
            Debug.LogWarning("SmokeEffectAnimator: Missing references!");
            return null;
        }

        if (smokeCoroutine != null)
        {
            StopCoroutine(smokeCoroutine);
            smokeCoroutine = null;
        }

        // DETACH SMOKE
        smokeObject.transform.SetParent(null, true);

        // FORCE UPRIGHT ROTATION
        smokeObject.transform.rotation = Quaternion.identity;

        // RESET SCALE ROTATION IF NEEDED
        smokeObject.transform.localRotation = Quaternion.identity;

        // ACTIVATE SMOKE
        smokeObject.SetActive(true);

        // RESTART ANIMATION
        smokeAnimator.Rebind();
        smokeAnimator.Update(0f);

        smokeAnimator.ResetTrigger(smokeTrigger);
        smokeAnimator.SetTrigger(smokeTrigger);

        return smokeObject;
    }

    // STOP NORMAL SMOKE
    private IEnumerator StopSmoke()
    {
        yield return new WaitForSeconds(smokeDuration);

        smokeObject.SetActive(false);

        smokeCoroutine = null;
    }
}