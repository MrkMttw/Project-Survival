using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("World Parent")]
    public Transform overWorld;
    public Transform naturalObjectsParent;

    [Header("Chunk Settings")]
    public float chunkSize = 16f;
    public int loadDistance = 1;

    [Header("World")]
    public int worldSeed = 12345;

    [Header("Spawn Data")]
    public WorldSpawnData[] spawnData;

    private Dictionary<Vector2Int, HashSet<string>> destroyedObjects =
        new Dictionary<Vector2Int, HashSet<string>>();

    private Vector2Int currentPlayerChunk;

    private Dictionary<Vector2Int, GameObject> loadedChunks =
        new Dictionary<Vector2Int, GameObject>();

    private Dictionary<Vector2Int, List<Vector3>> generatedPositions =
        new Dictionary<Vector2Int, List<Vector3>>();

    private Dictionary<Vector2Int, Dictionary<string, Vector3>> persistentObjectPositions =
        new Dictionary<Vector2Int, Dictionary<string, Vector3>>();

    private Dictionary<Vector2Int, Dictionary<string, GameObject>> persistentObjectPrefabs =
        new Dictionary<Vector2Int, Dictionary<string, GameObject>>();

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("WorldGenerator: Player is not assigned.");
            return;
        }

        if (overWorld == null)
        {
            Debug.LogError("WorldGenerator: OverWorld parent is not assigned.");
            return;
        }

        currentPlayerChunk = GetChunkCoordinate(player.position);

        UpdateChunks();
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        Vector2Int newPlayerChunk = GetChunkCoordinate(player.position);

        if (newPlayerChunk != currentPlayerChunk)
        {
            currentPlayerChunk = newPlayerChunk;
            UpdateChunks();
        }
    }

    private Vector2Int GetChunkCoordinate(Vector3 worldPosition)
    {
        int chunkX = Mathf.FloorToInt(worldPosition.x / chunkSize);
        int chunkY = Mathf.FloorToInt(worldPosition.y / chunkSize);

        return new Vector2Int(chunkX, chunkY);
    }

    private void UpdateChunks()
    {
        HashSet<Vector2Int> requiredChunks = new HashSet<Vector2Int>();

        for (int x = -loadDistance; x <= loadDistance; x++)
        {
            for (int y = -loadDistance; y <= loadDistance; y++)
            {
                Vector2Int chunkCoordinate = new Vector2Int(
                    currentPlayerChunk.x + x,
                    currentPlayerChunk.y + y
                );

                requiredChunks.Add(chunkCoordinate);

                if (!loadedChunks.ContainsKey(chunkCoordinate))
                {
                    LoadChunk(chunkCoordinate);
                }
            }
        }

        List<Vector2Int> chunksToUnload = new List<Vector2Int>();

        foreach (Vector2Int chunkCoordinate in loadedChunks.Keys)
        {
            if (!requiredChunks.Contains(chunkCoordinate))
            {
                chunksToUnload.Add(chunkCoordinate);
            }
        }

        foreach (Vector2Int chunkCoordinate in chunksToUnload)
        {
            UnloadChunk(chunkCoordinate);
        }
    }

    private void LoadChunk(Vector2Int chunkCoordinate)
    {
        GameObject chunkObject = new GameObject(
            "Chunk " + chunkCoordinate.x + ", " + chunkCoordinate.y
        );

        chunkObject.transform.SetParent(naturalObjectsParent);

        loadedChunks.Add(chunkCoordinate, chunkObject);
        
        if (!persistentObjectPositions.ContainsKey(chunkCoordinate))
        {
            persistentObjectPositions.Add(
                chunkCoordinate,
                new Dictionary<string, Vector3>()
            );
        }

        if (!persistentObjectPrefabs.ContainsKey(chunkCoordinate))
        {
            persistentObjectPrefabs.Add(
                chunkCoordinate,
                new Dictionary<string, GameObject>()
            );
        }

        generatedPositions.Add(
            chunkCoordinate,
            new List<Vector3>()
        );

        if (persistentObjectPositions[chunkCoordinate].Count == 0)
        {
            GenerateChunk(
                chunkCoordinate,
                chunkObject.transform
            );
        }
        else
        {
            GenerateChunkFromSavedPositions(
                chunkCoordinate,
                chunkObject.transform
            );
        }
    }

    private void GenerateChunk(
        Vector2Int chunkCoordinate,
        Transform chunkParent)
    {
        for (int dataIndex = 0; dataIndex < spawnData.Length; dataIndex++)
        {
            WorldSpawnData data = spawnData[dataIndex];

            if (data == null)
            {
                continue;
            }

            if (data.prefabs == null || data.prefabs.Length == 0)
            {
                continue;
            }

            Random.InitState(
                worldSeed +
                chunkCoordinate.x * 73856093 +
                chunkCoordinate.y * 19349663 +
                dataIndex * 83492791
            );

            int spawnCount = Random.Range(
                data.minSpawnCount,
                data.maxSpawnCount + 1
            );

            for (int i = 0; i < spawnCount; i++)
            {
                bool validPosition = false;
                Vector3 spawnPosition = Vector3.zero;

                for (int attempt = 0; attempt < 20; attempt++)
                {
                    spawnPosition = new Vector3(
                        chunkCoordinate.x * chunkSize +
                        Random.Range(0f, chunkSize),

                        chunkCoordinate.y * chunkSize +
                        Random.Range(0f, chunkSize),

                        0f
                    );

                    if (IsPositionValid(
                        chunkCoordinate,
                        spawnPosition,
                        data))
                    {
                        validPosition = true;
                        break;
                    }
                }

                if (!validPosition)
                {
                    continue;
                }

                string objectID =
                    dataIndex +
                    "_" +
                    i;

                if (IsObjectDestroyed(
                    chunkCoordinate,
                    objectID))
                {
                    continue;
                }

                GameObject prefab = data.prefabs[
                    Random.Range(0, data.prefabs.Length)
                ];

                GameObject spawnedObject = Instantiate(
                    prefab,
                    spawnPosition,
                    Quaternion.identity,
                    chunkParent
                );

                WorldObjectIdentity identity =
                    spawnedObject.AddComponent<WorldObjectIdentity>();

                identity.chunkCoordinate = chunkCoordinate;
                identity.spawnDataIndex = dataIndex;
                identity.objectIndex = i;

                generatedPositions[chunkCoordinate].Add(
                    spawnPosition
                );

                persistentObjectPositions[chunkCoordinate].Add(
                    objectID,
                    spawnPosition
                );

                persistentObjectPrefabs[chunkCoordinate].Add(
                    objectID,
                    prefab
                );
            }
        }
    }

    private void GenerateChunkFromSavedPositions(
        Vector2Int chunkCoordinate,
        Transform chunkParent)
    {
        foreach (
            KeyValuePair<string, Vector3> savedObject
            in persistentObjectPositions[chunkCoordinate])
        {
            string objectID = savedObject.Key;
            Vector3 spawnPosition = savedObject.Value;

            if (IsObjectDestroyed(
                chunkCoordinate,
                objectID))
            {
                continue;
            }

            GameObject prefab =
                persistentObjectPrefabs[chunkCoordinate][objectID];

            GameObject spawnedObject = Instantiate(
                prefab,
                spawnPosition,
                Quaternion.identity,
                chunkParent
            );

            string[] objectIDParts =
                objectID.Split('_');

            int spawnDataIndex =
                int.Parse(objectIDParts[0]);

            int objectIndex =
                int.Parse(objectIDParts[1]);

            WorldObjectIdentity identity =
                spawnedObject.AddComponent<WorldObjectIdentity>();

            identity.chunkCoordinate = chunkCoordinate;
            identity.spawnDataIndex = spawnDataIndex;
            identity.objectIndex = objectIndex;

            generatedPositions[chunkCoordinate].Add(
                spawnPosition
            );
        }
    }

    private bool IsPositionValid(
    Vector2Int chunkCoordinate,
    Vector3 position,
    WorldSpawnData data)
    {
        if (data.ignoreSpacingValidation)
        {
            return true;
        }

        foreach (Vector3 existingPosition in
                generatedPositions[chunkCoordinate])
        {
            if (Vector3.Distance(
                position,
                existingPosition) < data.minSpacing)
            {
                return false;
            }
        }

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }

                Vector2Int neighborChunk = new Vector2Int(
                    chunkCoordinate.x + x,
                    chunkCoordinate.y + y
                );

                if (!generatedPositions.TryGetValue(
                    neighborChunk,
                    out List<Vector3> neighborPositions))
                {
                    continue;
                }

                foreach (Vector3 existingPosition in neighborPositions)
                {
                    if (Vector3.Distance(
                        position,
                        existingPosition) < data.minSpacing)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }
    
    private bool IsObjectDestroyed(
        Vector2Int chunkCoordinate,
        string objectID)
    {
        if (!destroyedObjects.TryGetValue(
            chunkCoordinate,
            out HashSet<string> destroyed))
        {
            return false;
        }

        return destroyed.Contains(objectID);
    }

    private void UnloadChunk(Vector2Int chunkCoordinate)
    {
        if (!loadedChunks.TryGetValue(
            chunkCoordinate,
            out GameObject chunkObject))
        {
            return;
        }

        Destroy(chunkObject);

        loadedChunks.Remove(chunkCoordinate);
        generatedPositions.Remove(chunkCoordinate);
    }

    public void MarkObjectDestroyed(WorldObjectIdentity identity)
    {
        if (identity == null)
        {
            return;
        }

        if (!destroyedObjects.TryGetValue(
            identity.chunkCoordinate,
            out HashSet<string> destroyed))
        {
            destroyed = new HashSet<string>();

            destroyedObjects.Add(
                identity.chunkCoordinate,
                destroyed
            );
        }

        string objectID =
            identity.spawnDataIndex +
            "_" +
            identity.objectIndex;

        destroyed.Add(objectID);

        Debug.Log(
            "Remembered destroyed object: " +
            identity.chunkCoordinate +
            " / " +
            objectID
        );
    }

    private void OnDrawGizmos()
    {
        if (player == null)
        {
            return;
        }

        Vector2Int playerChunk =
            GetChunkCoordinate(player.position);

        for (int x = -loadDistance; x <= loadDistance; x++)
        {
            for (int y = -loadDistance; y <= loadDistance; y++)
            {
                Vector2Int chunkCoordinate = new Vector2Int(
                    playerChunk.x + x,
                    playerChunk.y + y
                );

                Vector3 chunkCenter = new Vector3(
                    (chunkCoordinate.x + 0.5f) * chunkSize,
                    (chunkCoordinate.y + 0.5f) * chunkSize,
                    0f
                );

                Vector3 chunkSizeVector = new Vector3(
                    chunkSize,
                    chunkSize,
                    0f
                );

                Gizmos.color = Color.white;
                Gizmos.DrawWireCube(
                    chunkCenter,
                    chunkSizeVector
                );

#if UNITY_EDITOR
                UnityEditor.Handles.Label(
                    chunkCenter,
                    "Chunk " +
                    chunkCoordinate.x +
                    ", " +
                    chunkCoordinate.y
                );
#endif
            }
        }
    }
}