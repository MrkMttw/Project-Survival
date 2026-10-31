using System.Collections;
using TMPro;
using UnityEngine;

public class GameOverController : MonoBehaviour
{
    [Header("References")]
    public HealthController healthController;
    public GameObject gameOverPanel;
    public Transform player;

    [Header("Death Drop")]
    public HotbarController hotbarController;

    [SerializeField] private float scatterRadius = 2f;

    [Header("Respawn Notification")]
    public TMP_Text respawnNotificationText;
    public float notificationDuration = 2.5f;
    
    private bool isGameOver;
    private Vector3 respawnPosition;
    private Rigidbody2D playerRigidbody;
    private Coroutine notificationCoroutine;

    private void Awake()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (respawnNotificationText != null)
        {
            respawnNotificationText.text = "";
            respawnNotificationText.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        if (hotbarController == null)
        {
            hotbarController = FindFirstObjectByType<HotbarController>();
        }

        if (hotbarController == null)
        {
            Debug.LogError("GameOverController: HotbarController not found!");
        }

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

        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);
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

    // UPDATE RESPAWN COORDINATES
    public void SetRespawnPosition(Vector2 newPosition)
    {
        respawnPosition = new Vector3(
            newPosition.x,
            newPosition.y,
            player != null ? player.position.z : 0f
        );

        Debug.Log("Respawn point updated: " + respawnPosition);
    }

    // DISPLAY RESPAWN NOTIFICATION
    public void ShowRespawnNotification(string message)
    {
        if (respawnNotificationText == null)
            return;

        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);

        notificationCoroutine = StartCoroutine(
            RespawnNotificationRoutine(message)
        );
    }

    private IEnumerator RespawnNotificationRoutine(string message)
    {
        respawnNotificationText.text = message;
        respawnNotificationText.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(notificationDuration);

        respawnNotificationText.text = "";
        respawnNotificationText.gameObject.SetActive(false);

        notificationCoroutine = null;
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

        // DROP ALL INVENTORY ITEMS
        if (InventoryController.instance != null)
        {
            InventoryController.instance.DropAllItems(
                player.position,
                scatterRadius
            );
        }

        // DROP ALL HOTBAR ITEMS
        if (hotbarController != null)
        {
            hotbarController.DropAllItems(
                player.position,
                scatterRadius
            );
        }

        // RESTORE PLAYER POSITION
        player.position = respawnPosition;

        // RESET MOVEMENT
        if (playerRigidbody != null)
        {
            playerRigidbody.position = respawnPosition;
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.angularVelocity = 0f;
        }

        // RESTORE HEALTH
        healthController.RespawnHealth();

        // CLOSE GAME OVER
        isGameOver = false;
        gameOverPanel.SetActive(false);

        Time.timeScale = 1f;
    }
}