using UnityEngine;

[System.Serializable]
public class BuildingSaveData
{
    public string buildingID;
    public int itemID;
    public Vector3 position;
    public Quaternion rotation;
    public Vector2Int chunkCoordinate;
    public int directionalPresetIndex = -1;

    public int grownPrefabIndex = -1;

    [System.NonSerialized]
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

    [Header("Campfire")]
    public bool hasCampfireState;
    public int campfireCurrentWood;
    public int campfireLevel;
    public float campfireFuelDecayTimer;
    public float campfireFuelDecayDelayTimer;

    [Header("Radar")]
    public bool hasRadarState;
    public int radarProgressionIron;
    public int radarLevel;

    [Header("Gate")]
    public bool hasGateState;
    public bool gateIsOpen;
}