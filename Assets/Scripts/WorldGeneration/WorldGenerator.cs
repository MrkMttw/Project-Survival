using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("World Parent")]
    public Transform overWorld;
    public Transform naturalObjectsParent;
    public Transform buildingParent;

    [Header("Item Dictionary")]
    public ItemDictionary itemDictionary;

    [Header("Chunk Settings")]
    public float chunkSize = 16f;
    public int loadDistance = 1;

    [Header("World")]
    public int worldSeed = 12345;

    public WorldSaveData worldData;

    [Header("Spawn Data")]
    public WorldSpawnData[] spawnData;

    [Header("Structure Spawning")]
    public StructureSpawner structureSpawner;

    private Dictionary<Vector2Int, HashSet<string>> destroyedObjects =
        new Dictionary<Vector2Int, HashSet<string>>();

    private Vector2Int currentPlayerChunk;

    private Dictionary<Vector2Int, GameObject> loadedChunks =
        new Dictionary<Vector2Int, GameObject>();

    private Dictionary<Vector2Int, GameObject> loadedBuildingChunks =
        new Dictionary<Vector2Int, GameObject>();

    private Dictionary<Vector2Int, List<Vector3>> generatedPositions =
        new Dictionary<Vector2Int, List<Vector3>>();

    private Dictionary<Vector2Int, Dictionary<string, Vector3>> persistentObjectPositions =
        new Dictionary<Vector2Int, Dictionary<string, Vector3>>();

    private Dictionary<Vector2Int, Dictionary<string, GameObject>> persistentObjectPrefabs =
        new Dictionary<Vector2Int, Dictionary<string, GameObject>>();

    private Dictionary<Vector2Int, Dictionary<string, BuildingSaveData>> savedBuildings =
        new Dictionary<Vector2Int, Dictionary<string, BuildingSaveData>>();

    private struct StructureClearArea
    {
        public Vector3 center;
        public float radius;
    }

    private List<StructureClearArea> structureClearAreas =
        new List<StructureClearArea>();

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

    public Vector2Int GetChunkCoordinate(Vector3 worldPosition)
    {
        int chunkX = Mathf.FloorToInt(worldPosition.x / chunkSize);
        int chunkY = Mathf.FloorToInt(worldPosition.y / chunkSize);

        return new Vector2Int(chunkX, chunkY);
    }

    public void SetWorldSeed(int seed)
    {
        worldSeed = seed;

        if (worldData == null)
        {
            worldData = new WorldSaveData();
        }

        worldData.worldSeed = seed;
    }

    public int GenerateNewWorldSeed()
    {
        worldSeed = Random.Range(
            int.MinValue,
            int.MaxValue
        );

        if (worldData == null)
        {
            worldData = new WorldSaveData();
        }

        worldData.worldSeed = worldSeed;

        return worldSeed;
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

                if (!loadedBuildingChunks.ContainsKey(chunkCoordinate))
                {
                    LoadBuildingChunk(chunkCoordinate);
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
            UnloadBuildingChunk(chunkCoordinate);
        }

        if (structureSpawner != null)
        {
            structureSpawner.UpdateStructureLoading(currentPlayerChunk);
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

        if (structureSpawner != null)
        {
            structureSpawner.ProcessChunk(chunkCoordinate);
        }
    }

    private void LoadBuildingChunk(
        Vector2Int chunkCoordinate)
    {
        if (loadedBuildingChunks.ContainsKey(
            chunkCoordinate))
        {
            return;
        }

        Transform chunkParent =
            GetBuildingChunkParent(chunkCoordinate);

        loadedBuildingChunks.Add(
            chunkCoordinate,
            chunkParent.gameObject
        );

        RestoreBuildingsInChunk(
            chunkCoordinate
        );
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

                if (IsInsideStructureClearArea(spawnPosition))
                {
                    MarkObjectIDDestroyed(chunkCoordinate, objectID);
                    continue;
                }

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

            if (IsInsideStructureClearArea(spawnPosition))
            {
                MarkObjectIDDestroyed(chunkCoordinate, objectID);
                continue;
            }

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
        
        if (structureSpawner != null)
        {
            structureSpawner.UnloadChunk(chunkCoordinate);
        }

        Destroy(chunkObject);

        loadedChunks.Remove(chunkCoordinate);
        generatedPositions.Remove(chunkCoordinate);
    }

    private void UnloadBuildingChunk(
        Vector2Int chunkCoordinate)
    {
        if (!loadedBuildingChunks.TryGetValue(
            chunkCoordinate,
            out GameObject chunkObject))
        {
            return;
        }

        // Keep NPCs alive when their building chunk unloads
        foreach (AssistantNPCBehavior npc in
                chunkObject.GetComponentsInChildren<AssistantNPCBehavior>())
        {
            npc.transform.SetParent(overWorld);
        }

        Destroy(chunkObject);

        loadedBuildingChunks.Remove(
            chunkCoordinate
        );
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

    public void RegisterBuilding(BuildingObject building)
    {
        if (building == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(building.buildingID))
        {
            return;
        }

        Vector2Int chunkCoordinate =
            GetChunkCoordinate(building.transform.position);

        if (!savedBuildings.TryGetValue(
            chunkCoordinate,
            out Dictionary<string, BuildingSaveData> buildings))
        {
            buildings =
                new Dictionary<string, BuildingSaveData>();

            savedBuildings.Add(
                chunkCoordinate,
                buildings
            );
        }

        BuildingSaveData data =
            new BuildingSaveData();

        data.buildingID =
            building.buildingID;

        data.itemID =
            building.itemID;

        data.position =
            building.transform.position;

        data.rotation =
            building.transform.rotation;

        data.chunkCoordinate =
            chunkCoordinate;

        buildings[data.buildingID] = data;
    }

    public void RegisterSaplingGrowth(
        BuildingObject building,
        float growthDeadline)
    {
        if (building == null || string.IsNullOrEmpty(building.buildingID))
            return;

        Vector2Int chunkCoordinate =
            GetChunkCoordinate(building.transform.position);

        if (!savedBuildings.TryGetValue(
            chunkCoordinate,
            out Dictionary<string, BuildingSaveData> buildings))
        {
            return;
        }

        if (!buildings.TryGetValue(
            building.buildingID,
            out BuildingSaveData data))
        {
            return;
        }

        data.growthDeadline = growthDeadline;
        data.hasGrown = false;
    }

    public int CountPlacedBuildingsWithItemIDs(int[] itemIDs)
    {
        if (itemIDs == null || itemIDs.Length == 0)
            return 0;

        int count = 0;

        foreach (var chunk in savedBuildings)
        {
            foreach (BuildingSaveData building in chunk.Value.Values)
            {
                for (int i = 0; i < itemIDs.Length; i++)
                {
                    if (building.itemID == itemIDs[i])
                    {
                        count++;
                        break;
                    }
                }
            }
        }

        return count;
    }

    public void UpdateBuilding(BuildingObject building)
    {
        if (building == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(building.buildingID))
        {
            return;
        }

        Vector2Int newChunkCoordinate =
            GetChunkCoordinate(building.transform.position);

        foreach (
            KeyValuePair<Vector2Int, Dictionary<string, BuildingSaveData>>
            chunk in savedBuildings)
        {
            if (chunk.Value.Remove(building.buildingID))
            {
                break;
            }
        }

        if (!savedBuildings.TryGetValue(
            newChunkCoordinate,
            out Dictionary<string, BuildingSaveData> buildings))
        {
            buildings =
                new Dictionary<string, BuildingSaveData>();

            savedBuildings.Add(
                newChunkCoordinate,
                buildings
            );
        }

        BuildingSaveData data =
            new BuildingSaveData();

        data.buildingID =
            building.buildingID;

        data.itemID =
            building.itemID;

        data.position =
            building.transform.position;

        data.rotation =
            building.transform.rotation;

        data.chunkCoordinate =
            newChunkCoordinate;

        buildings[data.buildingID] = data;
    }

    public void RemoveBuilding(BuildingObject building)
    {
        if (building == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(building.buildingID))
        {
            return;
        }

        foreach (
            KeyValuePair<Vector2Int, Dictionary<string, BuildingSaveData>>
            chunk in savedBuildings)
        {
            if (chunk.Value.Remove(building.buildingID))
            {
                break;
            }
        }
    }
    
    public void RestoreBuildingsInChunk(
        Vector2Int chunkCoordinate)
    {
        if (!savedBuildings.TryGetValue(
            chunkCoordinate,
            out Dictionary<string, BuildingSaveData> buildings))
        {
            return;
        }

        Transform chunkParent =
            GetBuildingChunkParent(chunkCoordinate);

        foreach (
            KeyValuePair<string, BuildingSaveData> entry
            in buildings)
        {
            BuildingSaveData data = entry.Value;

            GameObject prefabToRestore = null;

            if (data.hasGrown && data.grownPrefab != null)
            {
                // Restore the grown tree.
                prefabToRestore = data.grownPrefab;
            }
            else
            {
                // Restore the original planted sapling.
                GameObject itemPrefab =
                    itemDictionary.GetItemPrefab(data.itemID);

                if (itemPrefab == null)
                    continue;

                Item item = itemPrefab.GetComponent<Item>();

                if (item == null || item.buildingPrefab == null)
                    continue;

                prefabToRestore = item.buildingPrefab;
            }

            GameObject restoredObject = Instantiate(
                prefabToRestore,
                data.position,
                data.rotation,
                chunkParent
            );

            BuildingObject building =
                restoredObject.GetComponentInChildren<BuildingObject>();

            if (building == null)
            {
                building =
                    restoredObject.AddComponent<BuildingObject>();
            }

            building.itemID = data.itemID;
            building.buildingID = data.buildingID;
        }
    }

    public Transform GetBuildingChunkParent(Vector2Int chunkCoordinate)
    {
        string chunkName =
            "Chunk " +
            chunkCoordinate.x +
            ", " +
            chunkCoordinate.y;

        Transform existingChunk =
            buildingParent.Find(chunkName);

        if (existingChunk != null)
        {
            return existingChunk;
        }

        GameObject chunkObject =
            new GameObject(chunkName);

        chunkObject.transform.SetParent(buildingParent);

        return chunkObject.transform;
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
    
    public void RegisterStructureClearArea(Vector3 center, float radius)
    {
        if (radius <= 0f)
            return;

        // Avoid registering the same area more than once.
        foreach (StructureClearArea area in structureClearAreas)
        {
            if (area.center == center && area.radius == radius)
                return;
        }

        structureClearAreas.Add(new StructureClearArea
        {
            center = center,
            radius = radius
        });

        // Mark previously generated natural objects in this area as destroyed.
        foreach (var chunk in persistentObjectPositions)
        {
            foreach (var obj in chunk.Value)
            {
                if (Vector2.Distance(center, obj.Value) <= radius)
                {
                    MarkObjectIDDestroyed(chunk.Key, obj.Key);
                }
            }
        }

        // Remove affected objects that are currently loaded.
        foreach (GameObject chunkObject in loadedChunks.Values)
        {
            if (chunkObject == null)
                continue;

            WorldObjectIdentity[] identities =
                chunkObject.GetComponentsInChildren<WorldObjectIdentity>();

            foreach (WorldObjectIdentity identity in identities)
            {
                if (identity == null)
                    continue;

                string objectID =
                    identity.spawnDataIndex + "_" + identity.objectIndex;

                if (IsObjectDestroyed(identity.chunkCoordinate, objectID))
                {
                    Destroy(identity.gameObject);
                }
            }
        }
    }

    private void MarkObjectIDDestroyed(
        Vector2Int chunkCoordinate,
        string objectID)
    {
        if (!destroyedObjects.TryGetValue(
            chunkCoordinate, out HashSet<string> destroyed))
        {
            destroyed = new HashSet<string>();
            destroyedObjects.Add(chunkCoordinate, destroyed);
        }

        destroyed.Add(objectID);
    }

    private bool IsInsideStructureClearArea(Vector3 position)
    {
        foreach (StructureClearArea area in structureClearAreas)
        {
            if (Vector2.Distance(area.center, position) <= area.radius)
                return true;
        }

        return false;
    }

    public bool TryGetBuildingSaveData(
        BuildingObject building,
        out BuildingSaveData data)
    {
        data = null;

        if (building == null ||
            string.IsNullOrEmpty(building.buildingID))
        {
            return false;
        }

        foreach (var chunk in savedBuildings)
        {
            if (chunk.Value.TryGetValue(
                building.buildingID,
                out data))
            {
                return true;
            }
        }

        return false;
    }

    public void StartSaplingGrowth(
        BuildingObject building,
        float minimumTime,
        float maximumTime,
        GameObject treePrefab)
    {
        if (!TryGetBuildingSaveData(
            building,
            out BuildingSaveData data))
        {
            return;
        }

        if (data.hasGrown)
        {
            return;
        }

        if (data.growthDeadline <= 0f)
        {
            data.growthDeadline =
                Time.realtimeSinceStartup
                + Random.Range(minimumTime, maximumTime);
        }

        // Keep the selected tree type for this sapling.
        if (data.grownPrefab == null)
        {
            data.grownPrefab = treePrefab;
        }

        RegisterSaplingGrowth(building, data.growthDeadline);

        // RegisterSaplingGrowth doesn't modify grownPrefab,
        // so the selected prefab remains stored in the save data.
        data.grownPrefab = treePrefab;
    }

    public void MarkSaplingGrown(
        BuildingObject building,
        GameObject treePrefab)
    {
        if (!TryGetBuildingSaveData(
            building,
            out BuildingSaveData data))
        {
            return;
        }

        data.hasGrown = true;
        data.grownPrefab = treePrefab;
        data.growthDeadline = 0f;
    }
}