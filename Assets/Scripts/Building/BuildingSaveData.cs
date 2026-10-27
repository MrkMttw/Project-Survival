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

    public bool generatorIsActive;
    public float generatorRemainingTime;

    [Header("Furnace")]
    public int furnaceInputItemID;
    public int furnaceInputQuantity;

    public int furnaceFuelItemID;
    public int furnaceFuelQuantity;

    public int furnaceOutputItemID;
    public int furnaceOutputQuantity;

    public bool furnaceIsCooking;
    public float furnaceSmeltProgress;
    public float furnaceFuelRemaining;
}