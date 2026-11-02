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

    private float sprintTimer;
    private bool isExhausted;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private PlayerAnimationBehavior animationBehavior;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animationBehavior = GetComponent<PlayerAnimationBehavior>();

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

        sprintBar.fillAmount = sprintTimer / sprintDuration;
        sprintBar.gameObject.SetActive(sprintTimer < sprintDuration);

        rb.linearVelocity = moveInput * currentSpeed;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}