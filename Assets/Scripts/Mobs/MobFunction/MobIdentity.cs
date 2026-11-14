using System;
using UnityEngine;

public class MobIdentity : MonoBehaviour
{
    [Header("Identity")]
    public string mobID;
    public string prefabID;
    public MobType mobType;

    public Vector2Int originalChunkCoordinate;

    // INITIALIZATION

    public void Initialize(
        MobType type,
        string mobPrefabID,
        Vector2Int chunkCoordinate)
    {
        if (string.IsNullOrEmpty(mobID))
        {
            mobID = Guid.NewGuid().ToString("N");
        }

        mobType = type;
        prefabID = mobPrefabID;
        originalChunkCoordinate = chunkCoordinate;
    }
}