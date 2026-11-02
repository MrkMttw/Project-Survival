using UnityEngine;

public class PlayerAnimationBehavior : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;

    [Header("Movement Detection")]
    [SerializeField] private float movementThreshold = 0.02f;

        [Header("Animation Speed")]
        [SerializeField] private float walkAnimationSpeed = 1f;
        [SerializeField] private float sprintAnimationSpeed = 1.6f;

        private bool isSprinting;

    private Vector2 lastPosition;
    private Vector2 facingDirection = Vector2.down;
    private bool isMoving;
    private string currentState;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (rb == null)
            Debug.LogError("PlayerAnimationBehavior: Rigidbody2D is missing!", this);

        if (animator == null)
            Debug.LogError("PlayerAnimationBehavior: Animator is missing!", this);
    }

    private void Start()
    {
        if (rb == null || animator == null)
            return;

        lastPosition = rb.position;

        UpdateAnimatorParameters();
        PlayState("PlayerIdleDown");
    }

    private void FixedUpdate()
    {
        if (rb == null || animator == null)
            return;

        Vector2 currentPosition = rb.position;
        Vector2 movement = currentPosition - lastPosition;

        lastPosition = currentPosition;

        isMoving = movement.sqrMagnitude >
                   movementThreshold * movementThreshold;

        if (isMoving)
        {
            if (movement.x > movementThreshold)
            {
                facingDirection = Vector2.right;
            }
            else if (movement.x < -movementThreshold)
            {
                facingDirection = Vector2.left;
            }
            else if (movement.y > movementThreshold)
            {
                facingDirection = Vector2.up;
            }
            else if (movement.y < -movementThreshold)
            {
                facingDirection = Vector2.down;
            }
        }

        UpdateAnimatorParameters();

        string stateName = GetStateName();

        if (stateName != currentState)
            PlayState(stateName);
    }

    public void SetSprinting(bool sprinting)
    {
        isSprinting = sprinting;

        animator.speed = isSprinting
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