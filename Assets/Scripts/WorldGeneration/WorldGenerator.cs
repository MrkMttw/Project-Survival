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

    private WorldStateData worldStateData;

    private TreeController treeController;

    private Vector2Int currentPlayerChunk;

    private Dictionary<Vector2Int, GameObject> loadedChunks =
        new Dictionary<Vector2Int, GameObject>();

    private MobSpawnerController mobSpawnerController;

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

    private struct NaturalObjectCandidate
    {
        public Vector2Int chunkCoordinate;
        public int dataIndex;
        public int objectIndex;

        public Vector3 position;
        public GameObject prefab;
        public float priority;

        public WorldSpawnData data;
    }

    private List<StructureClearArea> structureClearAreas =
        new List<StructureClearArea>();

    private void Start()
    {
        treeController = FindFirstObjectByType<TreeController>();

        // LOAD SELECTED WORLD DATA
        if (SelectedWorldData.world != null)
        {
            worldData = SelectedWorldData.world;
            worldSeed = worldData.worldSeed;

            Debug.Log("WORLD LOADED: " + worldData.worldName);
            Debug.Log("WORLD SEED APPLIED: " + worldSeed);
            
            // LOAD SAVED WORLD STATE
            if (!LoadDestroyedObjectData())
            {
                Debug.LogError(
                    "WorldGenerator: Failed to load world state. Generation cancelled."
                );
                return;
            }

            // INITIALIZE DROPPED LOOT
            if (DropLootController.Instance != null)
            {
                DropLootController.Instance.InitializeWorldLoot(worldStateData);
            }
            else
            {
                Debug.LogError(
                    "WorldGenerator: DropLootController is missing."
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "WorldGenerator: No selected world found. Using default seed."
            );
        }

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

        mobSpawnerController =
            FindFirstObjectByType<MobSpawnerController>();

        if (mobSpawnerController == null)
        {
            Debug.LogError(
                "WorldGenerator: MobSpawnerController not found."
            );
        }

        currentPlayerChunk = GetChunkCoordinate(player.position);

        // Generate structure layout and register clear areas first.
        if (structureSpawner != null)
        {
            structureSpawner.UpdateStructureLoading(currentPlayerChunk);
        }

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

        // Initialize the regular mob spawner for this chunk.
        if (mobSpawnerController != null)
        {
            ChunkMobSpawner chunkSpawner =
                chunkObject.AddComponent<ChunkMobSpawner>();

            chunkSpawner.Initialize(
                mobSpawnerController,
                chunkCoordinate,
                chunkSize,
                player
            );
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

    // LOAD DESTROYED OBJECT AND BUILDING DATA
    private bool LoadDestroyedObjectData()
    {
        if (worldData == null)
            return true;

        worldStateData =
            WorldSaveManager.LoadWorldState(worldData.worldID);

        if (worldStateData == null)
            return false;

        destroyedObjects.Clear();
        savedBuildings.Clear();

        // RESTORE DESTROYED OBJECTS
        foreach (DestroyedObjectSaveData data in
                worldStateData.destroyedObjects)
        {
            if (data == null || string.IsNullOrEmpty(data.objectID))
                continue;

            Vector2Int chunkCoordinate = new Vector2Int(
                data.chunkX,
                data.chunkY
            );

            if (!destroyedObjects.TryGetValue(
                chunkCoordinate,
                out HashSet<string> destroyed))
            {
                destroyed = new HashSet<string>();
                destroyedObjects.Add(chunkCoordinate, destroyed);
            }

            destroyed.Add(data.objectID);
        }

        // RESTORE BUILDING REGISTRY
        foreach (BuildingSaveData data in worldStateData.buildings)
        {
            if (data == null || string.IsNullOrEmpty(data.buildingID))
                continue;

            Vector2Int chunkCoordinate = data.chunkCoordinate;

            if (!savedBuildings.TryGetValue(
                chunkCoordinate,
                out Dictionary<string, BuildingSaveData> buildings))
            {
                buildings = new Dictionary<string, BuildingSaveData>();

                savedBuildings.Add(
                    chunkCoordinate,
                    buildings
                );
            }

            if (buildings.ContainsKey(data.buildingID))
            {
                Debug.LogWarning(
                    "Duplicate building ID skipped: " + data.buildingID
                );

                continue;
            }

            // RESTORE THE GROWN TREE PREFAB REFERENCE
            if (data.grownPrefabIndex >= 0)
            {
                if (treeController != null &&
                    treeController.treePrefabs != null &&
                    data.grownPrefabIndex < treeController.treePrefabs.Length)
                {
                    data.grownPrefab =
                        treeController.treePrefabs[data.grownPrefabIndex];
                }
                else
                {
                    Debug.LogWarning(
                        "Could not resolve grown tree prefab for building: " +
                        data.buildingID
                    );
                }
            }

            buildings.Add(data.buildingID, data);
        }

        Debug.Log(
            "Loaded destroyed objects: " +
            worldStateData.destroyedObjects.Count
        );

        Debug.Log(
            "Loaded building records: " +
            worldStateData.buildings.Count
        );

        return true;
    }

    private void GenerateChunk(
        Vector2Int chunkCoordinate,
        Transform chunkParent)
    {
        List<NaturalObjectCandidate> candidates =
            GenerateCandidatesForChunk(chunkCoordinate);

        foreach (NaturalObjectCandidate candidate in candidates)
        {
            string objectID =
                candidate.dataIndex + "_" + candidate.objectIndex;

            if (IsInsideStructureClearArea(candidate.position))
            {
                MarkObjectIDDestroyed(chunkCoordinate, objectID);
                continue;
            }

            if (!IsCandidateValid(candidate))
            {
                continue;
            }

            if (IsObjectDestroyed(chunkCoordinate, objectID))
            {
                continue;
            }

            GameObject spawnedObject = Instantiate(
                candidate.prefab,
                candidate.position,
                Quaternion.identity,
                chunkParent
            );

            WorldObjectIdentity identity =
                spawnedObject.AddComponent<WorldObjectIdentity>();

            identity.chunkCoordinate = chunkCoordinate;
            identity.spawnDataIndex = candidate.dataIndex;
            identity.objectIndex = candidate.objectIndex;

            generatedPositions[chunkCoordinate].Add(
                candidate.position
            );

            persistentObjectPositions[chunkCoordinate].Add(
                objectID,
                candidate.position
            );

            persistentObjectPrefabs[chunkCoordinate].Add(
                objectID,
                candidate.prefab
            );
        }
    }

    private List<NaturalObjectCandidate> GenerateCandidatesForChunk(
        Vector2Int chunkCoordinate)
    {
        List<NaturalObjectCandidate> candidates =
            new List<NaturalObjectCandidate>();

        if (spawnData == null)
        {
            return candidates;
        }

        // Preserve the current global Unity random state.
        Random.State previousRandomState = Random.state;

        try
        {
            for (int dataIndex = 0; dataIndex < spawnData.Length; dataIndex++)
            {
                WorldSpawnData data = spawnData[dataIndex];

                if (data == null ||
                    data.prefabs == null ||
                    data.prefabs.Length == 0)
                {
                    continue;
                }

                int chunkSeed =
                    worldSeed +
                    chunkCoordinate.x * 73856093 +
                    chunkCoordinate.y * 19349663 +
                    dataIndex * 83492791;

                Random.InitState(chunkSeed);

                int spawnCount = Random.Range(
                    data.minSpawnCount,
                    data.maxSpawnCount + 1
                );

                for (int i = 0; i < spawnCount; i++)
                {
                    Vector3 position = new Vector3(
                        chunkCoordinate.x * chunkSize +
                        Random.Range(0f, chunkSize),

                        chunkCoordinate.y * chunkSize +
                        Random.Range(0f, chunkSize),

                        0f
                    );

                    GameObject prefab = data.prefabs[
                        Random.Range(0, data.prefabs.Length)
                    ];

                    float priority = Random.value;

                    NaturalObjectCandidate candidate =
                        new NaturalObjectCandidate();

                    candidate.chunkCoordinate = chunkCoordinate;
                    candidate.dataIndex = dataIndex;
                    candidate.objectIndex = i;
                    candidate.position = position;
                    candidate.prefab = prefab;
                    candidate.priority = priority;
                    candidate.data = data;

                    candidates.Add(candidate);
                }
            }
        }
        finally
        {
            // Prevent world generation from changing other systems' RNG.
            Random.state = previousRandomState;
        }

        return candidates;
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

    private bool IsCandidateValid(
        NaturalObjectCandidate candidate)
    {
        if (candidate.data.ignoreSpacingValidation)
        {
            return true;
        }

        int chunkRadius = Mathf.Max(
            0,
            Mathf.CeilToInt(candidate.data.minSpacing / chunkSize)
        );

        for (int x = -chunkRadius; x <= chunkRadius; x++)
        {
            for (int y = -chunkRadius; y <= chunkRadius; y++)
            {
                Vector2Int neighborChunk = new Vector2Int(
                    candidate.chunkCoordinate.x + x,
                    candidate.chunkCoordinate.y + y
                );

                List<NaturalObjectCandidate> neighborCandidates =
                    GenerateCandidatesForChunk(neighborChunk);

                foreach (NaturalObjectCandidate other in neighborCandidates)
                {
                    // Don't compare a candidate against itself.
                    if (other.chunkCoordinate == candidate.chunkCoordinate &&
                        other.dataIndex == candidate.dataIndex &&
                        other.objectIndex == candidate.objectIndex)
                    {
                        continue;
                    }

                    // Candidates inside structure clear areas cannot block others.
                    if (IsInsideStructureClearArea(other.position))
                    {
                        continue;
                    }

                    if (Vector3.Distance(
                        candidate.position,
                        other.position) < candidate.data.minSpacing)
                    {
                        if (HasHigherPriority(other, candidate))
                        {
                            return false;
                        }
                    }
                }
            }
        }

        return true;
    }   

    private bool HasHigherPriority(
        NaturalObjectCandidate first,
        NaturalObjectCandidate second)
    {
        if (first.priority != second.priority)
        {
            return first.priority > second.priority;
        }

        // Stable tie-breakers.
        if (first.chunkCoordinate.x != second.chunkCoordinate.x)
        {
            return first.chunkCoordinate.x < second.chunkCoordinate.x;
        }

        if (first.chunkCoordinate.y != second.chunkCoordinate.y)
        {
            return first.chunkCoordinate.y < second.chunkCoordinate.y;
        }

        if (first.dataIndex != second.dataIndex)
        {
            return first.dataIndex < second.dataIndex;
        }

        return first.objectIndex < second.objectIndex;
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
        
        // Clean up regular mobs before destroying the chunk.
        ChunkMobSpawner chunkSpawner =
            chunkObject.GetComponent<ChunkMobSpawner>();

        if (chunkSpawner != null)
        {
            chunkSpawner.CleanupMobs();
        }

        // Preserve existing structure unloading.
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

        // UPDATE WORLD STATE DATA
        if (worldStateData != null)
        {
            bool alreadySaved = worldStateData.destroyedObjects.Exists(
                data =>
                    data.chunkX == identity.chunkCoordinate.x &&
                    data.chunkY == identity.chunkCoordinate.y &&
                    data.objectID == objectID
            );

            if (!alreadySaved)
            {
                worldStateData.destroyedObjects.Add(
                    new DestroyedObjectSaveData
                    {
                        chunkX = identity.chunkCoordinate.x,
                        chunkY = identity.chunkCoordinate.y,
                        objectID = objectID
                    }
                );
            }
        }
    }
   
    public void RegisterBuilding(BuildingObject building)
    {
        if (building == null || string.IsNullOrEmpty(building.buildingID))
        {
            return;
        }

        Vector2Int chunkCoordinate =
            GetChunkCoordinate(building.transform.position);

        BuildingSaveData data = null;

        if (savedBuildings.TryGetValue(
            chunkCoordinate,
            out Dictionary<string, BuildingSaveData> buildings))
        {
            buildings.TryGetValue(building.buildingID, out data);
        }

        if (data == null)
        {
            data = new BuildingSaveData();
        }

        data.buildingID = building.buildingID;
        data.itemID = building.itemID;
        data.directionalPresetIndex =
            building.directionalPresetIndex;
        data.position = building.transform.position;
        data.rotation = building.transform.rotation;
        data.chunkCoordinate = chunkCoordinate;

        // SAVE INITIAL GATE STATE
        GateBehavior gate =
            building.GetComponentInChildren<GateBehavior>(true);

        if (gate != null)
        {
            data.hasGateState = true;
            data.gateIsOpen = gate.IsOpen;
        }

        if (!savedBuildings.TryGetValue(
            chunkCoordinate,
            out buildings))
        {
            buildings = new Dictionary<string, BuildingSaveData>();
            savedBuildings.Add(chunkCoordinate, buildings);
        }

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
        if (building == null ||
            string.IsNullOrEmpty(building.buildingID))
        {
            return;
        }

        TryGetBuildingSaveData(building, out BuildingSaveData data);

        foreach (var chunk in savedBuildings)
        {
            chunk.Value.Remove(building.buildingID);
        }

        Vector2Int newChunkCoordinate =
            GetChunkCoordinate(building.transform.position);

        if (data == null)
        {
            data = new BuildingSaveData();
        }

        data.buildingID = building.buildingID;
        data.itemID = building.itemID;
        data.directionalPresetIndex =
            building.directionalPresetIndex;
        data.position = building.transform.position;
        data.rotation = building.transform.rotation;
        data.chunkCoordinate = newChunkCoordinate;

        if (!savedBuildings.TryGetValue(
            newChunkCoordinate,
            out Dictionary<string, BuildingSaveData> buildings))
        {
            buildings = new Dictionary<string, BuildingSaveData>();
            savedBuildings.Add(newChunkCoordinate, buildings);
        }

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

            // DO NOT RESTORE A GROWN SAPLING AS AN UNGROWN SAPLING
            if (data.hasGrown && data.grownPrefab == null)
            {
                Debug.LogError(
                    "Grown tree prefab could not be restored for building: " +
                    data.buildingID
                );

                continue;
            }

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

                if (item == null)
                    continue;

                if (item.useDirectionalPrefabs)
                {
                    int presetIndex = data.directionalPresetIndex;

                    if (presetIndex < 0 ||
                        presetIndex >= item.buildingPresets.Count)
                    {
                        continue;
                    }

                    prefabToRestore =
                        item.buildingPresets[presetIndex].buildingPrefab;
                }
                else
                {
                    prefabToRestore = item.buildingPrefab;
                }

                if (prefabToRestore == null)
                    continue;
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
            building.directionalPresetIndex =
                data.directionalPresetIndex;

            // RESTORE GATE STATE
            GateBehavior gate =
                restoredObject.GetComponentInChildren<GateBehavior>(true);

            if (gate != null && data.hasGateState)
            {
                gate.SetGateState(data.gateIsOpen);
            }
        }
    }

    // SAVE CURRENT WORLD STATE
    public bool SaveCurrentWorldState()
    {
        if (worldStateData == null)
            return false;

        worldStateData.buildings.Clear();

        foreach (var chunk in savedBuildings)
        {
            foreach (BuildingSaveData data in chunk.Value.Values)
            {
                if (data == null)
                    continue;

                // STORE THE GROWN TREE'S PREFAB INDEX
                if (data.grownPrefab != null)
                {
                    if (treeController == null)
                    {
                        treeController =
                            FindFirstObjectByType<TreeController>();
                    }

                    if (treeController != null &&
                        treeController.treePrefabs != null)
                    {
                        data.grownPrefabIndex =
                            System.Array.IndexOf(
                                treeController.treePrefabs,
                                data.grownPrefab
                            );
                    }

                    if (data.grownPrefabIndex < 0)
                    {
                        Debug.LogWarning(
                            "Could not find grown tree prefab index for: " +
                            data.buildingID
                        );
                    }
                }

                worldStateData.buildings.Add(data);

                if (data.hasCampfireState || data.itemID == 9)
                {
                    Debug.Log(
                        $"CAMPFIRE SAVE CHECK | ID: {data.buildingID} | " +
                        $"Has State: {data.hasCampfireState} | " +
                        $"Wood: {data.campfireCurrentWood} | " +
                        $"Level: {data.campfireLevel}"
                    );
                }
            }
        }

        Debug.Log(
            "Saving building records: " +
            worldStateData.buildings.Count
        );

        return WorldSaveManager.SaveWorldState(worldStateData);
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

    public bool TryGetPlacedBuildingPosition(
        int itemID,
        out Vector2Int coordinates)
    {
        coordinates = Vector2Int.zero;

        foreach (var chunk in savedBuildings)
        {
            foreach (BuildingSaveData data in chunk.Value.Values)
            {
                if (data.itemID != itemID)
                    continue;

                coordinates = new Vector2Int(
                    Mathf.RoundToInt(data.position.x),
                    Mathf.RoundToInt(data.position.y)
                );

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