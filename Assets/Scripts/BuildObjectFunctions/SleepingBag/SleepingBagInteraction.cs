using UnityEngine;

public class SleepingBagInteraction : MonoBehaviour
{
    [Header("Sleeping Position")]
    public Transform sleepPosition;

    private GameOverController gameOverController;

    private void Awake()
    {
        gameOverController =
            FindFirstObjectByType<GameOverController>();
    }

    public void Interact(SleepTransitionController sleepController)
    {
        if (gameOverController == null)
        {
            Debug.LogError(
                "SleepingBagInteraction: GameOverController not found."
            );
            return;
        }

        // SET RESPAWN POINT USING WORLD COORDINATES
        gameOverController.SetRespawnPosition(transform.position);

        // CHECK SLEEP CONTROLLER
        if (sleepController == null)
        {
            Debug.LogError(
                "SleepingBagInteraction: Sleep Controller is not assigned."
            );
            return;
        }

        // CHECK IF IT IS NIGHTTIME
        if (!sleepController.IsSleepTimeAllowedNow())
        {
            gameOverController.ShowRespawnNotification(
                "Respawn Point Set, You Can Only Sleep at Night"
            );

            return;
        }

        if (sleepPosition == null)
        {
            Debug.LogError(
                "SleepingBagInteraction: Sleep Position is not assigned.",
                this
            );
            return;
        }

        // START SLEEPING
        sleepController.TryStartSleep(sleepPosition, 1);
    }
}