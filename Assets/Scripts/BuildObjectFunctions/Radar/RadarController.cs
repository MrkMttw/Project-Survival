using System;
using System.Collections.Generic;
using UnityEngine;

public class RadarController : MonoBehaviour
{
    public enum LocationSource
    {
        Campfire,
        Dungeon
    }

    [Serializable]
    public class CoordinateEntry
    {
        public string structureName;

        [Min(0)]
        public int requiredLevel = 1;

        public LocationSource locationSource;
        public int campfireItemID;
        public StructureSpawnData dungeonData;
    }

    [Header("Iron")]
    public GameObject ironItemPrefab;

    [Header("Radar Level")]
    public int progressionIron = 0;

    [SerializeField]
    private int radarLevel = 0;

    [Header("Structure Coordinates")]
    public WorldGenerator worldGenerator;
    public StructureSpawner structureSpawner;

    public List<CoordinateEntry> coordinateEntries =
        new List<CoordinateEntry>();

    private RadarUI radarUI;
    
    private BuildingSaveData savedData;

    private BuildingObject buildingObject;

    private bool radarPlaced = false;

    public int RadarLevel
    {
        get { return radarLevel; }
    }

    public float RadarRadius
    {
        get
        {
            switch (radarLevel)
            {
                case 1: return 30f;
                case 2: return 60f;
                case 3: return 90f;
                case 4: return 120f;
                case 5: return 150f;
                default: return 0f;
            }
        }
    }

    private void Update()
    {
        SaveRadarStateToMemory();
    }

    public void AddIron(int amount)
    {
        if (amount <= 0 || radarLevel >= 5)
            return;

        progressionIron += amount;

        UpdateRadarLevel();
        UpdateRadarUI();

        SaveRadarStateToMemory(savedData);
    }

    public bool IsIronItem(Item item)
    {
        if (item == null || ironItemPrefab == null)
            return false;

        Item ironItem = ironItemPrefab.GetComponent<Item>();

        if (ironItem == null)
            return false;

        return item.ID == ironItem.ID;
    }

    private void UpdateRadarLevel()
    {
        if (radarLevel >= 5)
            return;

        int requiredIron = GetNextLevelRequirement();

        if (progressionIron >= requiredIron)
        {
            progressionIron -= requiredIron;
            radarLevel++;

            UpdateRadarLevel();
        }
    }

    private int GetNextLevelRequirement()
    {
        switch (radarLevel)
        {
            case 0: return 3;
            case 1: return 6;
            case 2: return 9;
            case 3: return 12;
            case 4: return 15;
            case 5: return 15;
            default: return 3;
        }
    }

    public void RegisterRadarUI(RadarUI ui)
    {
        radarUI = ui;

        radarUI.SetCoordinatesVisible(radarPlaced);

        UpdateRadarUI();
    }

    public void RegisterRadarPlaced()
    {
        radarPlaced = true;

        if (radarUI != null)
            radarUI.SetCoordinatesVisible(true);
    }

    private void UpdateRadarUI()
    {
        if (radarUI == null)
            return;

        int requiredIron = GetNextLevelRequirement();
        int displayedIron = Mathf.Min(progressionIron, requiredIron);

        radarUI.UpdateUI(
            radarLevel,
            displayedIron,
            requiredIron
        );
    }

    public string GetUnlockedCoordinatesText()
    {
        List<string> lines = new List<string>();

        foreach (CoordinateEntry entry in coordinateEntries)
        {
            if (entry == null ||
                entry.requiredLevel > radarLevel ||
                string.IsNullOrWhiteSpace(entry.structureName))
            {
                continue;
            }

            Vector2Int coordinates;

            if (entry.locationSource == LocationSource.Campfire)
            {
                if (worldGenerator == null ||
                    !worldGenerator.TryGetPlacedBuildingPosition(
                        entry.campfireItemID,
                        out coordinates))
                {
                    continue;
                }
            }
            else
            {
                if (structureSpawner == null ||
                    entry.dungeonData == null ||
                    !structureSpawner.TryGetStructurePosition(
                        entry.dungeonData,
                        out coordinates))
                {
                    continue;
                }
            }

            lines.Add(
                entry.structureName + ": " +
                coordinates.x + ", " + coordinates.y
            );
        }

        return string.Join("\n", lines);
    }

        public void RefreshCoordinateUI()
    {
        if (radarUI == null)
            return;

        radarUI.RefreshCoordinatesUI();
    }
    
    public void ConnectToBuildingSaveData(BuildingObject building)
    {
        if (building == null)
        {
            Debug.LogError("[RADAR] BuildingObject is NULL!");
            return;
        }

        buildingObject = building;

        worldGenerator = FindFirstObjectByType<WorldGenerator>();

        if (worldGenerator == null)
        {
            Debug.LogError("[RADAR] WorldGenerator NOT FOUND!");
            return;
        }

        if (!worldGenerator.TryGetBuildingSaveData(
            buildingObject, out savedData))
        {
            Debug.LogError(
                "[RADAR] Building save data NOT FOUND! ID: " +
                buildingObject.buildingID
            );

            return;
        }

        if (savedData.hasRadarState)
        {
            progressionIron = savedData.radarProgressionIron;
            radarLevel = savedData.radarLevel;

            Debug.Log(
                $"[RADAR] STATE RESTORED | Level: {radarLevel} | Iron: {progressionIron}"
            );
        }

        radarPlaced = true;

        SaveRadarStateToMemory();
        UpdateRadarUI();

        if (radarUI != null)
            radarUI.SetCoordinatesVisible(true);
    }

    public void SaveRadarStateToMemory()
    {
        if (savedData == null)
            return;

        savedData.hasRadarState = true;
        savedData.radarProgressionIron = progressionIron;
        savedData.radarLevel = radarLevel;
    }

    public void SaveRadarStateToMemory(BuildingSaveData data)
    {
        if (data == null)
            return;

        data.hasRadarState = true;
        data.radarProgressionIron = progressionIron;
        data.radarLevel = radarLevel;
    }
}