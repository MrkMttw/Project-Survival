
using UnityEngine;

public class SleepingBagInteraction : MonoBehaviour
{
    [Header("Sleeping Position")]
    public Transform sleepPosition;

    public void Interact(SleepTransitionController sleepController)
    {
        if (sleepPosition == null)
        {
            Debug.LogError(
                "SleepingBagInteraction: Sleep Position is not assigned.",
                this
            );
            return;
        }

        if (sleepController == null)
        {
            Debug.LogError(
                "SleepingBagInteraction: Sleep Controller is not assigned.",
                this
            );
            return;
        }

        // Sleeping currently advances exactly one day.
        sleepController.TryStartSleep(sleepPosition, 1);
    }
}