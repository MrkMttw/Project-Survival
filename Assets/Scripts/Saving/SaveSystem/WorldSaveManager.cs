using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public static class WorldSaveManager
{
    private static string rootDirectory;
    private static string catalogPath;

    private static WorldCatalogData worldCatalog;

    // INITIALIZE SAVE SYSTEM
    public static bool Initialize()
    {
        rootDirectory = Path.Combine(
            Application.persistentDataPath,
            "Worlds"
        );

        catalogPath = Path.Combine(
            rootDirectory,
            "worlds.json"
        );

        try
        {
            Directory.CreateDirectory(rootDirectory);

            // LOAD EXISTING WORLD CATALOG
            if (SaveFileHandler.SaveExists(catalogPath))
            {
                worldCatalog = SaveFileHandler.LoadFromJson<WorldCatalogData>(catalogPath);

                // ATTEMPT BACKUP RECOVERY
                if (worldCatalog == null)
                {
                    Debug.LogWarning("Catalog is invalid. Attempting backup recovery.");

                    if (SaveBackupHandler.RestoreLatestBackup<WorldCatalogData>(
                        catalogPath,
                        data => data.worlds != null
                    ))
                    {
                        worldCatalog = SaveFileHandler.LoadFromJson<WorldCatalogData>(catalogPath);
                    }
                }

                // DO NOT OVERWRITE A CORRUPTED CATALOG
                if (worldCatalog == null)
                {
                    Debug.LogError("Catalog recovery failed. Existing world data was preserved.");
                    return false;
                }
            }
            else
            {
                // CREATE NEW CATALOG ONLY IF NONE EXISTS
                worldCatalog = new WorldCatalogData();

                if (!SaveFileHandler.SaveToJson(catalogPath, worldCatalog))
                {
                    Debug.LogError("Failed to initialize world catalog.");
                    return false;
                }
            }

            // ENSURE WORLD LIST EXISTS
            if (worldCatalog.worlds == null)
            {
                worldCatalog.worlds = new List<WorldSaveData>();

                if (!SaveFileHandler.SaveToJson(catalogPath, worldCatalog))
                    return false;
            }

            // VALIDATE REGISTERED WORLDS
            if (!ValidateWorldCatalog())
            {
                Debug.LogError("World catalog validation failed.");
                return false;
            }

            Debug.Log("World Save System Initialized.");
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("World Save Initialization Failed: " + exception.Message);
            return false;
        }
    }

    // VALIDATE WORLD CATALOG
    private static bool ValidateWorldCatalog()
    {
        List<WorldSaveData> validWorlds = new List<WorldSaveData>();

        HashSet<string> registeredIDs = new HashSet<string>();

        bool catalogChanged = false;

        foreach (WorldSaveData catalogWorld in worldCatalog.worlds)
        {
            // CHECK NULL ENTRIES
            if (catalogWorld == null)
            {
                catalogChanged = true;
                continue;
            }

            // CHECK WORLD ID
            if (!IsValidWorldID(catalogWorld.worldID))
            {
                Debug.LogWarning("Invalid world ID found. Entry skipped.");
                catalogChanged = true;
                continue;
            }

            // CHECK DUPLICATE IDS
            if (!registeredIDs.Add(catalogWorld.worldID))
            {
                Debug.LogWarning("Duplicate world ID found: " + catalogWorld.worldID);
                catalogChanged = true;
                continue;
            }

            // CHECK WORLD NAME
            if (string.IsNullOrWhiteSpace(catalogWorld.worldName))
            {
                Debug.LogWarning("Invalid world name: " + catalogWorld.worldID);
                catalogChanged = true;
                continue;
            }

            string worldDirectory = GetWorldDirectory(catalogWorld.worldID);

            // CHECK WORLD DIRECTORY
            if (!Directory.Exists(worldDirectory))
            {
                Debug.LogWarning("World directory missing: " + catalogWorld.worldName);

                // PRESERVE CATALOG ENTRY
                validWorlds.Add(catalogWorld);
                continue;
            }

            string worldFilePath = GetWorldFilePath(catalogWorld.worldID);

            WorldSaveData savedWorld = SaveFileHandler.LoadFromJson<WorldSaveData>(
                worldFilePath
            );

            // ATTEMPT WORLD METADATA BACKUP RECOVERY
            if (savedWorld == null && File.Exists(worldFilePath))
            {
                Debug.LogWarning("Invalid world metadata. Attempting backup recovery.");

                if (SaveBackupHandler.RestoreLatestBackup<WorldSaveData>(
                    worldFilePath,
                    data => data.worldID == catalogWorld.worldID &&
                            !string.IsNullOrWhiteSpace(data.worldName) &&
                            data.worldSeed >= 0
                ))
                {
                    savedWorld = SaveFileHandler.LoadFromJson<WorldSaveData>(
                        worldFilePath
                    );
                }
            }

            // RECREATE MISSING WORLD METADATA FROM CATALOG
            if (savedWorld == null && !File.Exists(worldFilePath))
            {
                Debug.LogWarning("World metadata missing. Recreating from catalog.");

                savedWorld = catalogWorld;

                if (!SaveFileHandler.SaveToJson(worldFilePath, savedWorld))
                {
                    Debug.LogError("Failed to recreate world metadata.");
                    return false;
                }
            }

            // SKIP INVALID WORLD METADATA
            if (savedWorld == null ||
                savedWorld.worldID != catalogWorld.worldID ||
                string.IsNullOrWhiteSpace(savedWorld.worldName))
            {
                Debug.LogWarning("World metadata could not be validated: " + catalogWorld.worldID);
                catalogChanged = true;
                continue;
            }

            // USE INDIVIDUAL WORLD METADATA AS THE SOURCE OF TRUTH
            if (!WorldDataMatches(catalogWorld, savedWorld))
            {
                catalogChanged = true;
            }

            validWorlds.Add(savedWorld);
        }

        // CHECK IF ENTRIES WERE REMOVED
        if (validWorlds.Count != worldCatalog.worlds.Count)
        {
            catalogChanged = true;
        }

        if (catalogChanged)
        {
            List<WorldSaveData> previousWorlds = worldCatalog.worlds;

            worldCatalog.worlds = validWorlds;

            if (!SaveFileHandler.SaveToJson(catalogPath, worldCatalog))
            {
                worldCatalog.worlds = previousWorlds;
                Debug.LogError("Failed to save validated world catalog.");
                return false;
            }

            Debug.Log("World catalog validation completed. Invalid entries were handled.");
        }

        return true;
    }

    // CHECK IF TWO WORLD DATA OBJECTS MATCH
    private static bool WorldDataMatches(
        WorldSaveData first,
        WorldSaveData second)
    {
        return first.worldID == second.worldID &&
               first.worldName == second.worldName &&
               first.worldSeed == second.worldSeed &&
               first.saveVersion == second.saveVersion &&
               first.creationDate == second.creationDate &&
               first.lastPlayedDate == second.lastPlayedDate;
    }

    // CHECK WORLD ID FORMAT
    private static bool IsValidWorldID(string worldID)
    {
        Guid parsedID;

        return !string.IsNullOrWhiteSpace(worldID) &&
               Guid.TryParseExact(worldID, "N", out parsedID);
    }

    // CREATE NEW WORLD
    public static WorldSaveData CreateWorld(string worldName, int worldSeed)
    {
        if (worldCatalog == null)
        {
            Debug.LogError("World Save Manager is not initialized.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(worldName))
        {
            Debug.LogError("World name cannot be empty.");
            return null;
        }

        WorldSaveData newWorld = new WorldSaveData
        {
            worldID = Guid.NewGuid().ToString("N"),
            worldName = worldName.Trim(),
            worldSeed = worldSeed,
            saveVersion = 1,
            creationDate = DateTime.UtcNow.ToString("o"),
            lastPlayedDate = DateTime.UtcNow.ToString("o")
        };

        string worldDirectory = GetWorldDirectory(newWorld.worldID);

        try
        {
            Directory.CreateDirectory(worldDirectory);

            // SAVE INDIVIDUAL WORLD METADATA
            string worldFilePath = GetWorldFilePath(newWorld.worldID);

            if (!SaveFileHandler.SaveToJson(worldFilePath, newWorld))
            {
                Directory.Delete(worldDirectory, true);
                return null;
            }

            // REGISTER WORLD IN GLOBAL CATALOG
            worldCatalog.worlds.Add(newWorld);

            if (!SaveFileHandler.SaveToJson(catalogPath, worldCatalog))
            {
                worldCatalog.worlds.Remove(newWorld);
                Directory.Delete(worldDirectory, true);
                return null;
            }

            Debug.Log("World created: " + newWorld.worldName);
            return newWorld;
        }
        catch (Exception exception)
        {
            Debug.LogError("World creation failed: " + exception.Message);
            return null;
        }
    }

    // GET WORLD DIRECTORY
    public static string GetWorldDirectory(string worldID)
    {
        return Path.Combine(rootDirectory, worldID);
    }

    // GET WORLD METADATA FILE PATH
    public static string GetWorldFilePath(string worldID)
    {
        return Path.Combine(
            GetWorldDirectory(worldID),
            "world.json"
        );
    }

    // GET WORLD STATE FILE PATH
    public static string GetWorldStateFilePath(string worldID)
    {
        return Path.Combine(
            GetWorldDirectory(worldID),
            "world_state.json"
        );
    }

    // LOAD WORLD STATE
    public static WorldStateData LoadWorldState(string worldID)
    {
        if (worldCatalog == null || GetWorldByID(worldID) == null)
        {
            Debug.LogError("Cannot load world state. World not found.");
            return null;
        }

        string filePath = GetWorldStateFilePath(worldID);

        // CREATE EMPTY DATA FOR WORLDS WITHOUT A STATE FILE
        if (!File.Exists(filePath))
        {
            return new WorldStateData
            {
                worldID = worldID
            };
        }

        WorldStateData stateData =
            SaveFileHandler.LoadFromJson<WorldStateData>(filePath);

        // ATTEMPT BACKUP RECOVERY
        if (stateData == null ||
            stateData.worldID != worldID ||
            stateData.destroyedObjects == null)
        {
            Debug.LogWarning("World state is invalid. Attempting backup recovery.");

            if (SaveBackupHandler.RestoreLatestBackup<WorldStateData>(
                filePath,
                data => data != null &&
                        data.worldID == worldID &&
                        data.destroyedObjects != null))
            {
                stateData =
                    SaveFileHandler.LoadFromJson<WorldStateData>(filePath);
            }
        }

        // DO NOT OVERWRITE INVALID DATA
        if (stateData == null ||
            stateData.worldID != worldID ||
            stateData.destroyedObjects == null)
        {
            Debug.LogError("World state could not be recovered.");
            return null;
        }

        Debug.Log("World state loaded: " + worldID);

        return stateData;
    }

    // SAVE WORLD STATE
    public static bool SaveWorldState(WorldStateData stateData)
    {
        if (worldCatalog == null || stateData == null)
            return false;

        if (!IsValidWorldID(stateData.worldID) ||
            GetWorldByID(stateData.worldID) == null)
        {
            Debug.LogError("Cannot save world state. Invalid world ID.");
            return false;
        }

        if (stateData.destroyedObjects == null)
        {
            stateData.destroyedObjects =
                new List<DestroyedObjectSaveData>();
        }

        string filePath = GetWorldStateFilePath(stateData.worldID);

        bool saved = SaveFileHandler.SaveToJson(filePath, stateData);

        if (saved)
            Debug.Log("World state saved: " + stateData.worldID);

        return saved;
    }

    // GET WORLD CATALOG
    public static WorldCatalogData GetWorldCatalog()
    {
        return worldCatalog;
    }

    // GET ALL WORLDS
    public static List<WorldSaveData> GetAllWorlds()
    {
        if (worldCatalog == null)
            return new List<WorldSaveData>();

        return new List<WorldSaveData>(worldCatalog.worlds);
    }

    // FIND WORLD BY ID
    public static WorldSaveData GetWorldByID(string worldID)
    {
        if (worldCatalog == null || string.IsNullOrEmpty(worldID))
            return null;

        return worldCatalog.worlds.Find(world => world.worldID == worldID);
    }

    // RENAME WORLD
    public static bool RenameWorld(string worldID, string newName)
    {
        if (worldCatalog == null || string.IsNullOrWhiteSpace(newName))
            return false;

        WorldSaveData world = GetWorldByID(worldID);

        if (world == null)
            return false;

        string previousName = world.worldName;
        world.worldName = newName.Trim();

        if (!SaveFileHandler.SaveToJson(GetWorldFilePath(worldID), world))
        {
            world.worldName = previousName;
            return false;
        }

        if (!SaveFileHandler.SaveToJson(catalogPath, worldCatalog))
        {
            world.worldName = previousName;
            SaveFileHandler.SaveToJson(GetWorldFilePath(worldID), world);
            return false;
        }

        Debug.Log("World renamed to: " + world.worldName);
        return true;
    }

    // UPDATE LAST PLAYED DATE
    public static bool UpdateLastPlayedDate(string worldID)
    {
        WorldSaveData world = GetWorldByID(worldID);

        if (world == null)
            return false;

        string previousDate = world.lastPlayedDate;
        world.lastPlayedDate = DateTime.UtcNow.ToString("o");

        if (!SaveFileHandler.SaveToJson(GetWorldFilePath(worldID), world))
        {
            world.lastPlayedDate = previousDate;
            return false;
        }

        if (!SaveFileHandler.SaveToJson(catalogPath, worldCatalog))
        {
            world.lastPlayedDate = previousDate;
            SaveFileHandler.SaveToJson(GetWorldFilePath(worldID), world);
            return false;
        }

        return true;
    }

    // DELETE WORLD
    public static bool DeleteWorld(string worldID)
    {
        if (worldCatalog == null)
            return false;

        if (string.IsNullOrWhiteSpace(worldID))
            return false;

        // PREVENT INVALID DIRECTORY PATHS
        if (Path.GetFileName(worldID) != worldID)
            return false;

        WorldSaveData world = GetWorldByID(worldID);

        if (world == null)
        {
            Debug.LogWarning("World not found.");
            return false;
        }

        string worldDirectory = GetWorldDirectory(worldID);

        if (!Directory.Exists(worldDirectory))
        {
            Debug.LogWarning("World directory does not exist.");
            return false;
        }

        string temporaryDirectory = Path.Combine(
            rootDirectory,
            ".deleting_" + worldID
        );

        try
        {
            // TEMPORARILY MOVE WORLD DIRECTORY
            Directory.Move(worldDirectory, temporaryDirectory);

            // REMOVE WORLD FROM CATALOG
            worldCatalog.worlds.Remove(world);

            // SAVE UPDATED CATALOG
            if (!SaveFileHandler.SaveToJson(catalogPath, worldCatalog))
            {
                worldCatalog.worlds.Add(world);
                Directory.Move(temporaryDirectory, worldDirectory);

                Debug.LogError("World deletion cancelled because catalog saving failed.");
                return false;
            }

            // PERMANENTLY DELETE WORLD DIRECTORY
            try
            {
                Directory.Delete(temporaryDirectory, true);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    "World was removed from the catalog, but temporary folder cleanup failed: "
                    + exception.Message
                );
            }

            Debug.Log("World deleted: " + world.worldName);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("World deletion failed: " + exception.Message);

            // ATTEMPT TO RESTORE DIRECTORY IF NECESSARY
            if (Directory.Exists(temporaryDirectory) &&
                !Directory.Exists(worldDirectory))
            {
                try
                {
                    Directory.Move(temporaryDirectory, worldDirectory);
                }
                catch (Exception restoreException)
                {
                    Debug.LogError("World restoration failed: " + restoreException.Message);
                }
            }

            return false;
        }
    }
}