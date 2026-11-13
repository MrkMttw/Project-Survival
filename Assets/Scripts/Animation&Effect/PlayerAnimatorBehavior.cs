using UnityEngine;

public class PlayerAnimationBehavior : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator animator;

    [Header("Movement Detection")]
    [SerializeField] private float movementThreshold = 0.02f;

    [Header("Direction Detection")]
    [SerializeField] private float verticalDirectionBias = 1.2f;

    [Header("Animation Speed")]
    [SerializeField] private float walkAnimationSpeed = 1f;
    [SerializeField] private float sprintAnimationSpeed = 1.6f;

    private Vector2 facingDirection = Vector2.down;
    private bool isMoving;
    private string currentState;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
            Debug.LogError(
                "PlayerAnimationBehavior: Animator is missing!",
                this
            );
    }

    private void Start()
    {
        if (animator == null)
            return;

        UpdateAnimatorParameters();
        PlayState("PlayerIdleDown");
    }

    public void SetMovementInput(Vector2 movementInput)
    {
        if (animator == null)
            return;

        isMoving = movementInput.sqrMagnitude >
                   movementThreshold * movementThreshold;

        if (isMoving)
        {
            float horizontal = Mathf.Abs(movementInput.x);
            float vertical = Mathf.Abs(movementInput.y);

            // DETERMINE DOMINANT DIRECTION
            if (vertical >= horizontal * verticalDirectionBias)
            {
                facingDirection = movementInput.y > 0
                    ? Vector2.up
                    : Vector2.down;
            }
            else
            {
                facingDirection = movementInput.x > 0
                    ? Vector2.right
                    : Vector2.left;
            }
        }

        UpdateAnimatorParameters();

        string stateName = GetStateName();

        if (stateName != currentState)
            PlayState(stateName);
    }

    public void SetSprinting(bool sprinting)
    {
        if (animator == null)
            return;

        animator.speed = sprinting
            ? sprintAnimationSpeed
            : walkAnimationSpeed;
    }

    private void UpdateAnimatorParameters()
    {
        animator.SetBool("IsMoving", isMoving);
        animator.SetFloat("MoveX", facingDirection.x);
        animator.SetFloat("MoveY", facingDirection.y);
    }

    private string GetStateName()
    {
        string direction;

        if (facingDirection == Vector2.up)
            direction = "Up";
        else if (facingDirection == Vector2.down)
            direction = "Down";
        else if (facingDirection == Vector2.left)
            direction = "Left";
        else
            direction = "Right";

        return isMoving
            ? "PlayerWalk" + direction
            : "PlayerIdle" + direction;
    }

    private void PlayState(string stateName)
    {
        currentState = stateName;
        animator.Play(stateName, 0, 0f);
    }
}