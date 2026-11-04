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

        return true;
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        if (Time.time - playStartTime >= maxAttackDuration)
        {
            ForceFinish();
        }
    }

    // ANIMATION EVENT: HIT FRAME
    public void OnAttackHit()
    {
        if (!isPlaying)
            return;

        AttackHit?.Invoke();
    }

    // ANIMATION EVENT: FINISH FRAME
    public void OnAttackFinished()
    {
        if (!isPlaying)
            return;

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