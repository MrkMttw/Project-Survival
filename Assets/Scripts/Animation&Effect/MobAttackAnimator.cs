using System;
using UnityEngine;

public class MobAttackAnimator : MonoBehaviour
{
    [Header("Animation References")]
    [SerializeField] private Animator mobAnimator;

    [Header("Attack Animation Settings")]
    [SerializeField] private string attackTrigger = "Attack";

    [Header("Attack Failsafe")]
    [SerializeField] private float maxAttackDuration = 1.5f;

    public event Action AttackHit;
    public event Action AttackFinished;

    private bool isPlaying;
    private float playStartTime;

    private void Awake()
    {
        if (mobAnimator == null)
            mobAnimator = GetComponent<Animator>();

        if (mobAnimator == null)
        {
            Debug.LogError(
                "[MobAttackAnimator] Animator reference missing!",
                this
            );
        }
    }

    public bool PlayAttack()
    {
        if (mobAnimator == null || string.IsNullOrEmpty(attackTrigger))
            return false;

        if (isPlaying)
            return false;

        isPlaying = true;
        playStartTime = Time.time;

        mobAnimator.ResetTrigger(attackTrigger);
        mobAnimator.SetTrigger(attackTrigger);

        Debug.Log("[MobAttackAnimator] Attack started.");

        return true;
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        if (Time.time - playStartTime >= maxAttackDuration)
        {
            Debug.LogWarning(
                "[MobAttackAnimator] Attack finish event missed. " +
                "Forcing attack reset.",
                this
            );

            ForceFinish();
        }
    }

    // ANIMATION EVENT: HIT FRAME
    public void OnAttackHit()
    {
        if (!isPlaying)
            return;

        Debug.Log("[MobAttackAnimator] Attack hit event received.");

        AttackHit?.Invoke();
    }

    // ANIMATION EVENT: FINISH FRAME
    public void OnAttackFinished()
    {
        if (!isPlaying)
            return;

        Debug.Log("[MobAttackAnimator] Attack finished event received.");

        ForceFinish();
    }

    private void ForceFinish()
    {
        if (!isPlaying)
            return;

        isPlaying = false;

        AttackFinished?.Invoke();
    }

    private void OnDisable()
    {
        isPlaying = false;
    }
}