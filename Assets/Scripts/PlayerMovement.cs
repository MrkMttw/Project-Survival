using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float sprintDuration = 3f;
    [SerializeField] private float sprintCooldown = 5f;

    private float sprintTimer;
    private float cooldownTimer;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Start with a full sprint
        sprintTimer = sprintDuration;
    }

    void Update()
    {
        // Cooldown timer
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;

            // Cooldown finished
            if (cooldownTimer <= 0)
            {
                cooldownTimer = 0;
                sprintTimer = sprintDuration;
            }
        }

        float currentSpeed = moveSpeed;

        // Sprint
        if (Input.GetKey(KeyCode.LeftShift) &&
            sprintTimer > 0 &&
            cooldownTimer <= 0)
        {
            currentSpeed *= sprintMultiplier;

            sprintTimer -= Time.deltaTime;

            // Sprint finished
            if (sprintTimer <= 0)
            {
                sprintTimer = 0;
                cooldownTimer = sprintCooldown;
            }
        }

        rb.linearVelocity = moveInput * currentSpeed;
    }

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