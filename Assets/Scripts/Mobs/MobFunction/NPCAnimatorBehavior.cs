using UnityEngine;

public class NPCAnimationBehavior : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;

    [Header("Movement Detection")]
    [SerializeField] private float movementThreshold = 0.02f;

    private Vector2 lastPosition;
    private Vector2 facingDirection = Vector2.down;
    private bool isMoving;
    private string currentState;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (rb == null)
            Debug.LogError("NPCAnimationBehavior: Rigidbody2D is missing!", this);

        if (animator == null)
            Debug.LogError("NPCAnimationBehavior: Animator is missing!", this);
    }

    private void Start()
    {
        if (rb == null || animator == null)
            return;

        lastPosition = rb.position;

        UpdateAnimatorParameters();
        PlayState("NPCIdleDown");
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
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
            {
                facingDirection = movement.x > 0f
                    ? Vector2.right
                    : Vector2.left;
            }
            else
            {
                facingDirection = movement.y > 0f
                    ? Vector2.up
                    : Vector2.down;
            }
        }

        UpdateAnimatorParameters();

        string stateName = GetStateName();

        if (stateName != currentState)
            PlayState(stateName);
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
            ? "NPCWalk" + direction
            : "NPCIdle" + direction;
    }

    private void PlayState(string stateName)
    {
        currentState = stateName;
        animator.Play(stateName, 0, 0f);
    }
}