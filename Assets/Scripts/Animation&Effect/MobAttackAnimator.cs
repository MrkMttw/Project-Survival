using System;
using UnityEngine;

public class MobAttackAnimator : MonoBehaviour
{
    [Header("Animation References")]
    [SerializeField] private Animator mobAnimator;

    [Header("Attack Animation Settings")]
    [SerializeField] private string attackTrigger = "Attack";

    [Tooltip("Force-finish the attack if the animation event fails.")]
    [SerializeField] private float maxAttackDuration = 1.5f;

    public event Action AttackStarted;
    public event Action AttackHit;
    public event Action AttackFinished;

    public bool IsPlaying => isPlaying;

    private bool isPlaying;
    private bool hasStarted;
    private float playTimer;

    private void Awake()
    {
        if (mobAnimator == null)
            mobAnimator = GetComponent<Animator>();

        // CHECK ANIMATOR LOCATION
        if (mobAnimator != null && mobAnimator.gameObject != gameObject)
        {
            Debug.LogWarning(
                $"{name}: MobAttackAnimator must be on the same GameObject as the Animator!"
            );
        }
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        playTimer += Time.deltaTime;

        // SAFETY TIMEOUT
        if (playTimer >= maxAttackDuration)
        {
            Debug.LogWarning($"{name}: Attack animation timed out. Force finishing.");
            ForceFinish();
        }
    }

    public bool PlayAttack()
    {
        if (mobAnimator == null || string.IsNullOrEmpty(attackTrigger))
            return false;

        if (isPlaying)
        {
            Debug.Log($"{name}: Attack blocked. Animation is still playing.");
            return false;
        }

        isPlaying = true;
        hasStarted = false;
        playTimer = 0f;

        mobAnimator.ResetTrigger(attackTrigger);
        mobAnimator.SetTrigger(attackTrigger);

        return true;
    }

    // ANIMATION EVENT: ATTACK START
    public void OnAttackStarted()
    {
        if (!isPlaying || hasStarted)
            return;

        hasStarted = true;
        AttackStarted?.Invoke();
    }

    // ANIMATION EVENT: HIT FRAME
    public void OnAttackHit()
    {
        if (!isPlaying)
            return;

        AttackHit?.Invoke();
    }

    // ANIMATION EVENT: ATTACK FINISHED
    public void OnAttackFinished()
    {
        ForceFinish();
    }

    // CANCEL ATTACK
    public void CancelAttack()
    {
        if (mobAnimator != null)
            mobAnimator.ResetTrigger(attackTrigger);

        ForceFinish();
    }

    // FORCE FINISH
    private void ForceFinish()
    {
        if (!isPlaying)
            return;

        isPlaying = false;
        hasStarted = false;
        playTimer = 0f;

        AttackFinished?.Invoke();
    }

    private void OnDisable()
    {
        ForceFinish();
    }
}