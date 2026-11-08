using System;
using System.Collections.Generic;

// WORLD STATE DATA
[Serializable]
public class WorldStateData
{
    public string worldID;

    public List<DestroyedObjectSaveData> destroyedObjects =
        new List<DestroyedObjectSaveData>();

    public List<DroppedLootSaveData> droppedLoot =
        new List<DroppedLootSaveData>();

    public List<BuildingSaveData> buildings =
        new List<BuildingSaveData>();
}

// DESTROYED OBJECT DATA
[Serializable]
public class DestroyedObjectSaveData
{
    public int chunkX;
    public int chunkY;
    public string objectID;
}

// DROPPED LOOT DATA
[Serializable]
public class DroppedLootSaveData
{
    public string lootID;

    public int itemID;
    public int quantity;

    public bool hasDurability;
    public int currentDurability;

    public float positionX;
    public float positionY;
    public float positionZ;

    public int chunkX;
    public int chunkY;
}