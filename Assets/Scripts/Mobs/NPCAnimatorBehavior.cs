using UnityEngine;

public class NPCAnimatorBehavior : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float movementThreshold = 0.01f;

    private Vector2 lastDirection = Vector2.down;
    private Vector3 lastPosition;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        lastPosition = transform.position;
    }

    private void Update()
    {
        if (animator == null)
            return;

        Vector2 movement =
            (Vector2)(transform.position - lastPosition);

        Vector2 velocity = movement / Time.deltaTime;

        bool isWalking = velocity.sqrMagnitude >
                         movementThreshold * movementThreshold;

        animator.SetBool("isWalking", isWalking);

        if (isWalking)
        {
            lastDirection = velocity.normalized;

            animator.SetFloat("InputX", lastDirection.x);
            animator.SetFloat("InputY", lastDirection.y);
        }
        else
        {
            animator.SetFloat("InputX", 0f);
            animator.SetFloat("InputY", 0f);
        }

        animator.SetFloat("LastInputX", lastDirection.x);
        animator.SetFloat("LastInputY", lastDirection.y);

        lastPosition = transform.position;
    }
}