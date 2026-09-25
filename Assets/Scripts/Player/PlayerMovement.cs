using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float sprintDuration = 3f;
    [SerializeField] private float sprintRegeneration = 0.3f;

    [SerializeField] private Image sprintBar;

    private float sprintTimer;
    private bool isExhausted;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        sprintBar.gameObject.SetActive(false);
        sprintTimer = sprintDuration;
    }

    void Update()
    {
        float currentSpeed = moveSpeed;

        bool isSprinting =
            Input.GetKey(KeyCode.LeftShift) &&
            !isExhausted &&
            sprintTimer > 0 &&
            moveInput != Vector2.zero;

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

        sprintBar.fillAmount = sprintTimer / sprintDuration;
        sprintBar.gameObject.SetActive(sprintTimer < sprintDuration);
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