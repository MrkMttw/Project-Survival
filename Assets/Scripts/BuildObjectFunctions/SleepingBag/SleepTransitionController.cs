using System.Collections;
using TMPro;
using UnityEngine;

public class SleepTransitionController : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;
    public GameClock gameClock;
    public HungerController hungerController;
    public ScreenFadeController screenFadeController;
    
    [Header("Sleep Text")]
    public TMP_Text sleepText;
    public float characterInterval = 0.5f;

    [Header("Sleep Time Range")]
    [Tooltip("Earliest hour the player can sleep (24-hour format).")]
    [Range(0, 23)]
    public int sleepStartHour = 18;

    [Tooltip("Hour when the sleep window ends (24-hour format).")]
    [Range(0, 23)]
    public int sleepEndHour = 6;

    private bool isSleeping;
    private bool movementWasEnabled;
    private Coroutine sleepCoroutine;

    private Rigidbody2D playerRigidbody;

    private void Awake()
    {
        if (playerMovement != null)
        {
            playerRigidbody =
                playerMovement.GetComponent<Rigidbody2D>();
        }

        if (sleepText != null)
        {
            sleepText.text = "";
            sleepText.gameObject.SetActive(false);
        }
    }

    public bool TryStartSleep(
        Transform sleepPosition,
        int daysToAdvance = 1)
    {
        if (isSleeping)
            return false;

        if (!IsSleepTimeAllowed())
        {
            Debug.Log("You can only sleep during the configured sleep hours.");
            return false;
        }

        if (sleepPosition == null ||
            playerMovement == null ||
            gameClock == null ||
            hungerController == null ||
            screenFadeController == null)
        {
            Debug.LogError(
                "SleepTransitionController: One or more references are missing."
            );
            return false;
        }

        if (sleepText == null)
        {
            Debug.LogError(
                "SleepTransitionController: Sleep Text is not assigned."
            );
            return false;
        }

        if (daysToAdvance <= 0)
            return false;

        isSleeping = true;
        sleepCoroutine = StartCoroutine(
            SleepRoutine(sleepPosition, daysToAdvance)
        );

        return true;
    }

    private IEnumerator SleepRoutine(
        Transform sleepPosition,
        int daysToAdvance)
    {
        movementWasEnabled = playerMovement.enabled;

        try
        {
            // Prevent movement during sleep.
            playerMovement.enabled = false;

            if (playerRigidbody != null)
                playerRigidbody.linearVelocity = Vector2.zero;

            // Fade to black.
            yield return screenFadeController.FadeOut();

            // Display the sleep text one character at a time.
            yield return AnimateSleepText();

            // Hide the text before revealing morning.
            sleepText.gameObject.SetActive(false);
            sleepText.text = "";

            // Move the player to the configured sleeping position.
            playerMovement.transform.position = sleepPosition.position;

            if (playerRigidbody != null)
            {
                playerRigidbody.position = sleepPosition.position;
                playerRigidbody.linearVelocity = Vector2.zero;
            }

            // Advance to morning and apply the hunger penalty once.
            gameClock.AdvanceToMorning(daysToAdvance);
            hungerController.HalveCurrentHunger();

            // Fade back in to reveal morning.
            yield return screenFadeController.FadeIn();
        }
        finally
        {
            RestoreAfterSleep();
        }
    }

    private IEnumerator AnimateSleepText()
    {
        string sleepMessage = "Sleeping Through the Night...";

        sleepText.text = "";
        sleepText.gameObject.SetActive(true);

        foreach (char character in sleepMessage)
        {
            sleepText.text += character;
            yield return new WaitForSecondsRealtime(
                Mathf.Max(0f, characterInterval)
            );
        }
    }

    private void RestoreAfterSleep()
    {
        if (playerRigidbody != null)
            playerRigidbody.linearVelocity = Vector2.zero;

        if (playerMovement != null)
            playerMovement.enabled = movementWasEnabled;

        if (sleepText != null)
        {
            sleepText.text = "";
            sleepText.gameObject.SetActive(false);
        }

        if (screenFadeController != null)
            screenFadeController.SetImmediate(0f);

        isSleeping = false;
        sleepCoroutine = null;
    }

    private void OnDisable()
    {
        if (!isSleeping)
            return;

        if (sleepCoroutine != null)
            StopCoroutine(sleepCoroutine);

        RestoreAfterSleep();
    }

    private bool IsSleepTimeAllowed()
    {
        int currentMinutes =
            (gameClock.GetHour() * 60) + gameClock.GetMinute();

        int startMinutes = sleepStartHour * 60;
        int endMinutes = sleepEndHour * 60;

        // Same start and end means sleeping is allowed all day.
        if (startMinutes == endMinutes)
            return true;

        // Normal range, such as 08:00–18:00.
        if (startMinutes < endMinutes)
        {
            return currentMinutes >= startMinutes &&
                currentMinutes < endMinutes;
        }

        // Overnight range, such as 18:00–06:00.
        return currentMinutes >= startMinutes ||
            currentMinutes < endMinutes;
    }
}