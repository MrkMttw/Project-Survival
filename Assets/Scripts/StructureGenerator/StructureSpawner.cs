using System.Collections.Generic;
using UnityEngine;

public class StructureSpawner : MonoBehaviour
{
    [Header("References")]
    public WorldGenerator worldGenerator;
    public Transform player;
    public Transform structureParent;

    [Header("Structures")]
    public StructureSpawnData[] structures;

    [Header("Generation")]
    [Min(1)]
    public int spawnAttempts = 30;

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

    private readonly List<StructureRecord> structureRecords =
        new List<StructureRecord>();

    private readonly HashSet<Vector2Int> processedChunks =
        new HashSet<Vector2Int>();

    public void ProcessChunk(Vector2Int chunkCoordinate)
    {
        if (worldGenerator == null || player == null)
            return;

        if (structures == null || structures.Length == 0)
            return;

        if (!processedChunks.Add(chunkCoordinate))
            return;

        // Restore structures previously assigned to this chunk.
        foreach (StructureRecord record in structureRecords)
        {
            if (record.chunk == chunkCoordinate &&
                record.instance == null)
            {
                SpawnRecord(record);
            }
        }

        // Try to generate new structures for this chunk.
        foreach (StructureSpawnData data in structures)
        {
            if (data == null || data.prefab == null ||
                data.maxWorldCount <= 0)
                continue;

            int count = 0;

            foreach (StructureRecord record in structureRecords)
            {
                if (record.data == data)
                    count++;
            }

            if (count >= data.maxWorldCount)
                continue;

            System.Random random = new System.Random(
                worldGenerator.worldSeed
                ^ (chunkCoordinate.x * 73856093)
                ^ (chunkCoordinate.y * 19349663)
                ^ GetStableHash(data.name)
            );

            for (int attempt = 0; attempt < spawnAttempts; attempt++)
            {
                Vector3 position = new Vector3(
                    chunkCoordinate.x * worldGenerator.chunkSize
                        + (float)random.NextDouble()
                            * worldGenerator.chunkSize,
                    chunkCoordinate.y * worldGenerator.chunkSize
                        + (float)random.NextDouble()
                            * worldGenerator.chunkSize,
                    0f
                );

                if (Vector2.Distance(player.position, position)
                    < data.minPlayerDistance)
                    continue;

                if (IsTooCloseToStructure(position, data))
                    continue;

                StructureRecord newRecord = new StructureRecord
                {
                    data = data,
                    chunk = chunkCoordinate,
                    position = position,
                    instance = null
                };

                structureRecords.Add(newRecord);

                // Clear natural spawn objects around this structure.
                worldGenerator.RegisterStructureClearArea(
                    position,
                    data.clearRadius
                );

                SpawnRecord(newRecord);

                // At most one new copy of this structure per chunk.
                break;
            }
        }
    }

    private void SpawnRecord(StructureRecord record)
    {
        record.instance = Instantiate(
            record.data.prefab,
            record.position,
            Quaternion.identity,
            structureParent
        );
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
                return true;
        }

        return false;
    }

    
    public void UnloadChunk(Vector2Int chunkCoordinate)
    {
        // Allow the chunk to be processed again when it reloads.
        // Keep its structure alive for now.
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
}