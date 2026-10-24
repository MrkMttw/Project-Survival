using System.Collections.Generic;
using UnityEngine;

public class StructureSpawner : MonoBehaviour
{
    [Header("References")]
    public WorldGenerator worldGenerator;
    public Transform structureParent;

    [Header("Structures")]
    public StructureSpawnData[] structures;

    [Header("Generation")]
    [Min(1)]
    public int spawnAttempts = 30;

    [Min(1f)]
    public float maxGenerationDistance = 200f;

    [Header("Structure Loading")]
    [Min(1)]
    public int structureLoadDistance = 3;

    private class StructureRecord
    {
        public StructureSpawnData data;
        public Vector2Int chunk;
        public Vector3 position;
        public GameObject instance;
    }

    private class StructureCandidate
    {
        public StructureSpawnData data;
        public int dataIndex;
        public Vector2Int chunk;
        public Vector3 position;
        public double priority;
        public int attemptIndex;
    }

    private readonly List<StructureRecord> structureRecords =
        new List<StructureRecord>();

    private readonly HashSet<Vector2Int> processedChunks =
        new HashSet<Vector2Int>();

    private bool layoutGenerated;

    public void ProcessChunk(Vector2Int chunkCoordinate)
    {
        if (worldGenerator == null)
            return;

        if (structures == null || structures.Length == 0)
            return;

        if (!layoutGenerated)
            GenerateDeterministicLayout();

        if (!processedChunks.Add(chunkCoordinate))
            return;

        foreach (StructureRecord record in structureRecords)
        {
            if (record.chunk == chunkCoordinate &&
                record.instance == null)
            {
                SpawnRecord(record);
            }
        }
    }

    private void GenerateDeterministicLayout()
    {
        if (worldGenerator == null)
            return;

        if (worldGenerator.chunkSize <= 0f)
        {
            Debug.LogError(
                "StructureSpawner: chunkSize must be greater than zero."
            );
            return;
        }

        if (structures == null || structures.Length == 0)
            return;

        List<StructureCandidate> candidates =
            new List<StructureCandidate>();

        float chunkSize = worldGenerator.chunkSize;

        int minChunk = Mathf.FloorToInt(
            -maxGenerationDistance / chunkSize
        );

        int maxChunk = Mathf.FloorToInt(
            maxGenerationDistance / chunkSize
        );

        // Generate candidates for every chunk intersecting
        // the configured generation area.
        for (int chunkX = minChunk; chunkX <= maxChunk; chunkX++)
        {
            for (int chunkY = minChunk; chunkY <= maxChunk; chunkY++)
            {
                Vector2Int chunkCoordinate =
                    new Vector2Int(chunkX, chunkY);

                for (int dataIndex = 0;
                     dataIndex < structures.Length;
                     dataIndex++)
                {
                    StructureSpawnData data = structures[dataIndex];

                    if (data == null ||
                        data.prefab == null ||
                        data.maxWorldCount <= 0)
                    {
                        continue;
                    }

                    int seed = GetCandidateSeed(
                        worldGenerator.worldSeed,
                        chunkCoordinate,
                        data,
                        dataIndex
                    );

                    System.Random random = new System.Random(seed);

                    for (int attempt = 0;
                         attempt < spawnAttempts;
                         attempt++)
                    {
                        Vector3 position = new Vector3(
                            chunkX * chunkSize
                                + (float)random.NextDouble() * chunkSize,
                            chunkY * chunkSize
                                + (float)random.NextDouble() * chunkSize,
                            0f
                        );

                        float distanceFromSpawn =
                            Vector2.Distance(Vector2.zero, position);

                        if (distanceFromSpawn < data.minSpawnDistance)
                            continue;

                        if (distanceFromSpawn > maxGenerationDistance)
                            continue;

                        candidates.Add(new StructureCandidate
                        {
                            data = data,
                            dataIndex = dataIndex,
                            chunk = chunkCoordinate,
                            position = position,
                            priority = random.NextDouble(),
                            attemptIndex = attempt
                        });
                    }
                }
            }
        }

        // Sort all candidates deterministically so exploration
        // order cannot decide which structures get selected.
        candidates.Sort(CompareCandidates);

        Dictionary<StructureSpawnData, int> structureCounts =
            new Dictionary<StructureSpawnData, int>();

        HashSet<string> occupiedStructureChunks =
            new HashSet<string>();

        foreach (StructureCandidate candidate in candidates)
        {
            StructureSpawnData data = candidate.data;

            if (!structureCounts.TryGetValue(data, out int count))
                count = 0;

            if (count >= data.maxWorldCount)
                continue;

            // Preserve the original rule: at most one copy
            // of each structure type per chunk.
            string chunkKey =
                candidate.dataIndex + ":" +
                candidate.chunk.x + ":" +
                candidate.chunk.y;

            if (occupiedStructureChunks.Contains(chunkKey))
                continue;

            if (IsTooCloseToStructure(
                candidate.position,
                data))
            {
                continue;
            }

            StructureRecord record = new StructureRecord
            {
                data = data,
                chunk = candidate.chunk,
                position = candidate.position,
                instance = null
            };

            structureRecords.Add(record);
            structureCounts[data] = count + 1;
            occupiedStructureChunks.Add(chunkKey);

            Debug.Log(
                $"[StructureSpawner] Structure planned: '{data.prefab.name}' " +
                $"at {record.position}, chunk {record.chunk}."
            );

            // Register every selected structure's clear area
            // before future natural-object chunks are generated.
            worldGenerator.RegisterStructureClearArea(
                candidate.position,
                data.clearRadius
            );
        }

        layoutGenerated = true;
    }

    private int GetCandidateSeed(
        int worldSeed,
        Vector2Int chunkCoordinate,
        StructureSpawnData data,
        int dataIndex)
    {
        unchecked
        {
            int seed = worldSeed;

            seed ^= chunkCoordinate.x * 73856093;
            seed ^= chunkCoordinate.y * 19349663;
            seed ^= GetStableHash(data.name);
            seed ^= dataIndex * 83492791;

            return seed;
        }
    }

    private int CompareCandidates(
        StructureCandidate a,
        StructureCandidate b)
    {
        int result = a.priority.CompareTo(b.priority);

        if (result != 0)
            return result;

        result = a.dataIndex.CompareTo(b.dataIndex);

        if (result != 0)
            return result;

        result = a.chunk.x.CompareTo(b.chunk.x);

        if (result != 0)
            return result;

        result = a.chunk.y.CompareTo(b.chunk.y);

        if (result != 0)
            return result;

        return a.attemptIndex.CompareTo(b.attemptIndex);
    }

    private int GetStableHash(string value)
    {
        unchecked
        {
            int hash = 23;

            foreach (char character in value)
                hash = hash * 31 + character;

            return hash;
        }
    }

    private bool IsTooCloseToStructure(
        Vector3 position,
        StructureSpawnData data)
    {
        foreach (StructureRecord record in structureRecords)
        {
            float minimumDistance =
                data.clearRadius + record.data.clearRadius;

            if (Vector2.Distance(position, record.position)
                < minimumDistance)
            {
                return true;
            }
        }

        return false;
    }

    private void SpawnRecord(StructureRecord record)
    {
        record.instance = Instantiate(
            record.data.prefab,
            record.position,
            Quaternion.identity,
            structureParent
        );

        Debug.Log(
            $"[StructureSpawner] Structure instantiated: " +
            $"'{record.data.prefab.name}' at {record.position}, " +
            $"chunk {record.chunk}.",
            record.instance
        );
    }

    public void UnloadChunk(Vector2Int chunkCoordinate)
    {
        // Keep the layout and records.
        // The chunk can be processed again when it reloads.
        processedChunks.Remove(chunkCoordinate);
    }

    public void UpdateStructureLoading(Vector2Int playerChunk)
    {
        foreach (StructureRecord record in structureRecords)
        {
            if (record.instance == null)
                continue;

            int distanceX = Mathf.Abs(
                record.chunk.x - playerChunk.x
            );

            int distanceY = Mathf.Abs(
                record.chunk.y - playerChunk.y
            );

            if (distanceX > structureLoadDistance ||
                distanceY > structureLoadDistance)
            {
                Destroy(record.instance);
                record.instance = null;
            }
        }
    }

        
    public bool TryGetStructurePosition(
        StructureSpawnData target,
        out Vector2Int coordinates)
    {
        coordinates = Vector2Int.zero;

        if (target == null || worldGenerator == null)
            return false;

        if (!layoutGenerated)
            GenerateDeterministicLayout();

        if (!layoutGenerated)
            return false;

        foreach (StructureRecord record in structureRecords)
        {
            if (record.data != target)
                continue;

            coordinates = new Vector2Int(
                Mathf.RoundToInt(record.position.x),
                Mathf.RoundToInt(record.position.y)
            );

            return true;
        }

        return false;
    }
}