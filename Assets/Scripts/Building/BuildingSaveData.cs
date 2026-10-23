using UnityEngine;

[System.Serializable]
public class BuildingSaveData
{
    public string buildingID;
    public int itemID;
    public Vector3 position;
    public Quaternion rotation;
    public Vector2Int chunkCoordinate;

    public GameObject grownPrefab;

    public float growthDeadline;
    public bool hasGrown;
}