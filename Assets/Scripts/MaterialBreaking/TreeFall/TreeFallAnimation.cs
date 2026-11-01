using System.Collections;
using UnityEngine;

public class TreeFallAnimation : MonoBehaviour
{
    [Header("Tree Elements")]
    public Transform treeElement;
    public GameObject shadowElement;

    [Header("Fall Settings")]
    public float fallDuration = 0.8f;
    public float fallAngle = 90f;

    [Header("Animation")]
    public AnimationCurve fallCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private bool isFalling;

    public IEnumerator PlayFall(Vector2 attackerPosition)
    {
        if (isFalling || treeElement == null)
            yield break;

        isFalling = true;

        // DETACH SHADOW
        if (shadowElement != null)
        {
            shadowElement.transform.SetParent(
                null,
                true
            );
        }

        // DETERMINE FALL DIRECTION
        float direction =
            attackerPosition.x < transform.position.x ? 1f : -1f;

        // GET ROTATION
        Quaternion startRotation = treeElement.localRotation;

        float startAngle = treeElement.localEulerAngles.z;

        float targetAngle =
            startAngle + (fallAngle * direction);

        Quaternion targetRotation =
            Quaternion.Euler(0f, 0f, targetAngle);

        // PLAY FALL ANIMATION
        float elapsedTime = 0f;

        while (elapsedTime < fallDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsedTime / fallDuration
            );

            float curveValue = fallCurve.Evaluate(progress);

            treeElement.localRotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                curveValue
            );

            yield return null;
        }

        // FINAL ROTATION
        treeElement.localRotation = targetRotation;

        Debug.Log(gameObject.name + " finished falling!");
    }
}