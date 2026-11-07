using System;
using System.Collections.Generic;

// WORLD STATE DATA
[Serializable]
public class WorldStateData
{
    public string worldID;

    public List<DestroyedObjectSaveData> destroyedObjects =
        new List<DestroyedObjectSaveData>();
}

// DESTROYED OBJECT DATA
[Serializable]
public class DestroyedObjectSaveData
{
    public int chunkX;
    public int chunkY;
    public string objectID;
}