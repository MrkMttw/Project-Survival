using UnityEngine;

public class MapController : MonoBehaviour
{
    [Header("Map")]
    public GameObject mapPage;

    [Header("Player")]
    public Transform player;

    private RadarController radarController;

    private void Start()
    {
        radarController =
            FindFirstObjectByType<RadarController>();

        UpdateMapState();
    }

    private void Update()
    {
        UpdateMapState();
    }

    private void UpdateMapState()
    {
        if (radarController == null)
            return;

        if (radarController.RadarLevel >= 1)
        {
            UnlockMap();
        }
        else
        {
            LockMap();
        }
    }

    private void UnlockMap()
    {
        // Map functionality will be added here.
    }

    private void LockMap()
    {
        // Map functionality will be added here.
    }

    public Vector2 WorldToMapPosition(Vector3 worldPosition)
    {
        if (player == null)
            return Vector2.zero;

        if (radarController == null)
            return Vector2.zero;

        float radarRadius = radarController.RadarRadius;

        if (radarRadius <= 0f)
            return Vector2.zero;

        Vector2 relativePosition =
            (Vector2)worldPosition - (Vector2)player.position;

        return relativePosition / radarRadius;
    }
}