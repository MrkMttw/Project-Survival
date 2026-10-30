using UnityEngine;

public class GateController : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    public bool TryInteract()
    {
        if (player == null)
            return false;

        GateBehavior[] gates =
            FindObjectsByType<GateBehavior>(FindObjectsSortMode.None);

        GateBehavior nearestGate = null;
        float nearestDistance = float.MaxValue;

        foreach (GateBehavior gate in gates)
        {
            if (gate == null ||
                !gate.IsPlayerInRange(player.position))
            {
                continue;
            }

            float distance =
                Vector2.Distance(player.position, gate.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestGate = gate;
            }
        }

        if (nearestGate == null)
            return false;

        nearestGate.ToggleGate();
        return true;
    }
}