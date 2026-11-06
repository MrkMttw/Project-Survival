using System;
using System.Collections.Generic;

[Serializable]
public class WorldSaveData
{
    public string worldID;
    public string worldName;

    public int worldSeed;
    public int saveVersion = 1;

    public string creationDate;
    public string lastPlayedDate;
}

[Serializable]
public class WorldCatalogData
{
    public List<WorldSaveData> worlds = new List<WorldSaveData>();
}