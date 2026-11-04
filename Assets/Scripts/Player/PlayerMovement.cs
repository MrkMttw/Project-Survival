using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 2f;

    [Header("Sprint")]
    [SerializeField] private float sprintDuration = 3f;
    [SerializeField] private float sprintRegeneration = 0.3f;
    [SerializeField] private Image sprintBar;

    [Header("Knockback")]
    [SerializeField] private float knockbackDamping = 15f;

    private float sprintTimer;
    private bool isExhausted;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private PlayerAnimationBehavior animationBehavior;

    // KNOCKBACK
    private Vector2 knockbackVelocity;
    private float knockbackTimer;
    private bool isKnockedBack;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animationBehavior = GetComponent<PlayerAnimationBehavior>();

        if (sprintBar != null)
            sprintBar.gameObject.SetActive(false);

        sprintTimer = sprintDuration;
    }

    private void Update()
    {
        float currentSpeed = moveSpeed;

        bool isSprinting =
            Input.GetKey(KeyCode.LeftShift) &&
            !isExhausted &&
            sprintTimer > 0 &&
            moveInput != Vector2.zero;

        animationBehavior.SetSprinting(isSprinting);
        animationBehavior.SetMovementInput(moveInput);

        if (isSprinting)
        {
            currentSpeed *= sprintMultiplier;

            sprintTimer -= Time.deltaTime;

            if (sprintTimer <= 0)
            {
                sprintTimer = 0;
                isExhausted = true;
            }
        }
        else
        {
            sprintTimer += sprintRegeneration * Time.deltaTime;

            if (sprintTimer >= sprintDuration)
            {
                sprintTimer = sprintDuration;
                isExhausted = false;
            }
        }

        if (sprintBar != null)
        {
            sprintBar.fillAmount = sprintTimer / sprintDuration;
            sprintBar.gameObject.SetActive(sprintTimer < sprintDuration);
        }

        // NORMAL MOVEMENT
        if (!isKnockedBack)
        {
            rb.linearVelocity = moveInput * currentSpeed;
        }
    }

    private void FixedUpdate()
    {
        if (!isKnockedBack)
            return;

        // APPLY KNOCKBACK
        rb.linearVelocity = knockbackVelocity;

        knockbackVelocity = Vector2.MoveTowards(
            knockbackVelocity,
            Vector2.zero,
            knockbackDamping * Time.fixedDeltaTime
        );

        knockbackTimer -= Time.fixedDeltaTime;

        // END KNOCKBACK
        if (knockbackTimer <= 0f ||
            knockbackVelocity.sqrMagnitude <= 0.01f)
        {
            knockbackVelocity = Vector2.zero;
            knockbackTimer = 0f;
            isKnockedBack = false;

            rb.linearVelocity = Vector2.zero;
        }
    }

    // APPLY KNOCKBACK
    public void ApplyKnockback(
        Vector2 direction,
        float strength,
        float duration
    )
    {
        if (direction.sqrMagnitude <= 0.001f)
            return;

        if (strength <= 0f || duration <= 0f)
            return;

        direction.Normalize();

        knockbackVelocity = direction * strength;
        knockbackTimer = duration;
        isKnockedBack = true;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}