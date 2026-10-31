using UnityEngine;

public class GateController : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    private GateBehavior nearestGate;

    private void Update()
    {
        UpdateGatePrompt();
    }

    // FIND NEAREST GATE
    private GateBehavior FindNearestGate()
    {
        if (player == null)
            return null;

        GateBehavior[] gates =
            FindObjectsByType<GateBehavior>(FindObjectsSortMode.None);

        GateBehavior nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (GateBehavior gate in gates)
        {
            if (gate == null ||
                !gate.IsPlayerInRange(player.position))
                continue;

            float distance =
                Vector2.Distance(player.position, gate.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = gate;
            }
        }

        return nearest;
    }

    // UPDATE GATE PROMPT
    private void UpdateGatePrompt()
    {
        nearestGate = FindNearestGate();

        if (PromptController.Instance == null)
            return;

        if (nearestGate != null)
        {
            PromptController.Instance.RequestPrompt(
                PromptType.E, this);
        }
        else
        {
            PromptController.Instance.ReleasePrompt(
                PromptType.E, this);
        }
    }

    // INTERACTION
    public bool TryInteract()
    {
        if (player == null)
            return false;

        if (nearestGate == null ||
            !nearestGate.IsPlayerInRange(player.position))
        {
            nearestGate = FindNearestGate();
        }

        if (nearestGate == null)
            return false;

        nearestGate.ToggleGate();
        return true;
    }

    private void OnDisable()
    {
        if (PromptController.Instance != null)
        {
            PromptController.Instance.ReleasePrompt(
                PromptType.E, this);
        }
    }
}