using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Handles transitions between map areas by updating camera boundaries.
/// When the player enters a trigger zone, the camera confinement area is updated to the new map boundary.
/// </summary>
public class MapTransition : MonoBehaviour
{
    /// <summary>
    /// The polygon collider that defines the boundary for the new map area.
    /// This will be applied to the camera confiner when the player enters the trigger.
    /// </summary>
    [SerializeField] PolygonCollider2D mapBoundary;

    /// <summary>
    /// The Cinemachine camera confiner that constrains the camera to a specific area.
    /// This component is updated with the new boundary when transitioning maps.
    /// </summary>
    [SerializeField] CinemachineConfiner2D confiner;

    /// <summary>
    /// Called when another collider enters the trigger zone.
    /// Updates the camera boundary if the player enters the transition area.
    /// </summary>
    /// <param name="collision">The collider that entered the trigger zone.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            confiner.BoundingShape2D = mapBoundary;
            confiner.InvalidateBoundingShapeCache();
        }
    }
}