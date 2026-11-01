using UnityEngine;

public class ChunkMobSpawner : MonoBehaviour
{
    [Header("Chunk Settings")]
    public Vector2Int chunkCoordinate;
    public float chunkSize = 16f;

    [Header("References")]
    public MobSpawnerController mobSpawnerController;

    private bool isInitialized;

    // INITIALIZATION

    public void Initialize(
        MobSpawnerController manager,
        Vector2Int coordinate,
        float size,
        Transform playerTransform)
    {
        if (isInitialized)
            return;

        mobSpawnerController = manager;
        chunkCoordinate = coordinate;
        chunkSize = size;

        if (mobSpawnerController == null)
        {
            Debug.LogError(
                "ChunkMobSpawner: MobSpawnerController is missing."
            );

            return;
        }

        mobSpawnerController.RegisterChunk(this);

        isInitialized = true;
    }

    // CHUNK CLEANUP

    public void CleanupMobs()
    {
        if (!isInitialized)
            return;

        if (mobSpawnerController != null)
        {
            mobSpawnerController.UnregisterChunk(this);
        }

        isInitialized = false;

        // EXISTING MOBS ARE NOT DESTROYED.
    }

    private void OnDestroy()
    {
        CleanupMobs();
    }
}