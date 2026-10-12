using UnityEngine;

public class GameOverController : MonoBehaviour
{
    [Header("References")]
    public HealthController healthController;
    public GameObject gameOverPanel;
    public Transform player;

    private bool isGameOver;
    private Vector3 respawnPosition;
    private Rigidbody2D playerRigidbody;

    private void Awake()
    {
        gameOverPanel.SetActive(false);
    }

    private void Start()
    {
        // Remember the player's starting position.
        if (player != null)
        {
            respawnPosition = player.position;
            playerRigidbody = player.GetComponent<Rigidbody2D>();
        }
    }

    private void OnEnable()
    {
        if (healthController != null)
            healthController.OnDeath += ShowGameOver;
    }

    private void OnDisable()
    {
        if (healthController != null)
            healthController.OnDeath -= ShowGameOver;
    }

    private void ShowGameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;
        gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public void Respawn()
    {
        if (!isGameOver)
            return;

        if (player == null || healthController == null)
        {
            Debug.LogError(
                "GameOverController: Player or HealthController is missing."
            );
            return;
        }

        // Restore the player's position.
        player.position = respawnPosition;

        // Stop any remaining Rigidbody2D movement.
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.angularVelocity = 0f;
        }

        // Restore health and allow death to trigger again.
        healthController.RespawnHealth();

        // Close Game Over and resume gameplay.
        isGameOver = false;
        gameOverPanel.SetActive(false);

        Time.timeScale = 1f;
    }
}