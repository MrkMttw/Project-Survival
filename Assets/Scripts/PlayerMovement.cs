using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player movement including walking and sprinting mechanics.
/// Manages sprint duration, cooldown, and updates animator parameters for character animation.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    /// <summary>
    /// Base movement speed for the player.
    /// </summary>
    [SerializeField] private float moveSpeed = 5f;

    /// <summary>
    /// Multiplier applied to movement speed when sprinting.
    /// </summary>
    [SerializeField] private float sprintMultiplier = 2f;

    /// <summary>
    /// Maximum duration of a sprint in seconds.
    /// </summary>
    [SerializeField] private float sprintDuration = 3f;

    /// <summary>
    /// Cooldown period in seconds before sprint can be used again.
    /// </summary>
    [SerializeField] private float sprintCooldown = 5f;

    /// <summary>
    /// Remaining time for the current sprint.
    /// </summary>
    private float sprintTimer;

    /// <summary>
    /// Remaining time before sprint can be used again.
    /// </summary>
    private float cooldownTimer;

    /// <summary>
    /// Rigidbody2D component for physics-based movement.
    /// </summary>
    private Rigidbody2D rb;

    /// <summary>
    /// Current movement input vector from the player.
    /// </summary>
    private Vector2 moveInput;

    /// <summary>
    /// Animator component for controlling character animations.
    /// </summary>
    private Animator animator;

    /// <summary>
    /// Initializes player movement components and sets initial sprint state.
    /// Called before the first frame update.
    /// </summary>
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        sprintTimer = sprintDuration;
    }

    /// <summary>
    /// Updates sprint timers and applies movement based on input and sprint state.
    /// Called once per frame.
    /// </summary>
    void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0)
            {
                cooldownTimer = 0;
                sprintTimer = sprintDuration;
            }
        }

        float currentSpeed = moveSpeed;

        if (Input.GetKey(KeyCode.LeftShift) &&
            sprintTimer > 0 &&
            cooldownTimer <= 0)
        {
            currentSpeed *= sprintMultiplier;

            sprintTimer -= Time.deltaTime;

            if (sprintTimer <= 0)
            {
                sprintTimer = 0;
                cooldownTimer = sprintCooldown;
            }
        }

        rb.linearVelocity = moveInput * currentSpeed;
    }

    /// <summary>
    /// Input System callback for movement input.
    /// Updates animator parameters based on movement direction and state.
    /// </summary>
    /// <param name="context">Callback context containing input data.</param>
    public void OnMove(InputAction.CallbackContext context)
    {
        animator.SetBool("isWalking", true);

        if (context.canceled)
        {
            animator.SetBool("isWalking", false);
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }

        moveInput = context.ReadValue<Vector2>();

        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
    }
}