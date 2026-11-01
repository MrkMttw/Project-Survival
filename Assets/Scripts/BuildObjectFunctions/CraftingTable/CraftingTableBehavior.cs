using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CraftingTableBehavior : MonoBehaviour
{
    private BuildingObject building;
    private BoxCollider2D interactionCollider;

    private void Awake()
    {
        building = GetComponentInChildren<BuildingObject>();
        interactionCollider = GetComponent<BoxCollider2D>();
    }

    // CHECK PLAYER RANGE
    public bool IsPlayerInRange(Vector2 playerPosition)
    {
        return building != null &&
               interactionCollider != null &&
               interactionCollider.OverlapPoint(playerPosition);
    }
}