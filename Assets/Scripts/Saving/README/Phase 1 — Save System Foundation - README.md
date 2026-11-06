# Phase 1 — Save System Foundation

## Overview

The Save System Foundation establishes the basic file management and data protection system for Project Survival.

It handles the creation, loading, updating, validation, and recovery of save files using JSON. It also provides automatic backup creation to help protect existing save data from corruption or failed save operations.

This phase focuses on preparing the foundation for the later implementation of player data, building persistence, and world persistence.

## 1. System Components

The foundation consists of five main components:

| Script | Responsibility |
|---|---|
| `SaveFileHandler.cs` | Handles JSON saving, loading, file checking, and deletion. |
| `SaveBackupHandler.cs` | Creates, rotates, and restores save backups. |
| `WorldSaveData.cs` | Defines the data structure for world metadata and the world catalog. |
| `WorldSaveManager.cs` | Initializes and manages the world catalog and individual world metadata. |
| `WorldSaveManagerTester.cs` | Tests save system initialization and registered world retrieval. |
| `SaveFileTester.cs` | Tests backup restoration and JSON loading. |

## 2. Save File Storage Location

The save system uses Unity's `Application.persistentDataPath` as its root storage location.

The `WorldSaveManager` automatically creates a `Worlds` directory inside this location.

Expected directory structure:

```text
Application.persistentDataPath/
└── Worlds/
    ├── worlds.json
    ├── [World ID]/
    │   ├── world.json
    │   └── Backups/
    │       ├── world.json.backup1
    │       └── world.json.backup2
    └── [Another World ID]/
        ├── world.json
        └── Backups/
            ├── world.json.backup1
            └── world.json.backup2
```

### File Descriptions

- `worlds.json` — Contains the global list of registered worlds.
- `world.json` — Contains the metadata of an individual world.
- `Backups/` — Stores backup copies of the corresponding save file.
- `backup1` — Contains the latest available backup.
- `backup2` — Contains the previous backup.

The directory structure is generated automatically. There is no need to manually create these folders.

## 3. SaveFileHandler.cs

### Purpose

`SaveFileHandler` is responsible for the basic JSON file operations used throughout the save system.

It provides four main methods:

| Method | Description |
|---|---|
| `SaveToJson<T>()` | Converts data into formatted JSON and saves it. |
| `LoadFromJson<T>()` | Reads and converts JSON into an object. |
| `SaveExists()` | Checks whether a save file exists. |
| `DeleteSave()` | Deletes an existing save file. |

### Saving Process

The `SaveToJson()` method follows this sequence:

1. Creates the parent directory if it does not exist.
2. Converts the supplied data into formatted JSON.
3. Writes the JSON into a temporary `.tmp` file.
4. If the destination already exists, creates a backup before replacing it.
5. Replaces the existing save file or moves the temporary file into place.
6. Cleans up any remaining temporary file.

The temporary file helps prevent an incomplete write from directly replacing the existing save.

**Important:** If backup creation fails while updating an existing file, the save operation is cancelled instead of replacing the previous save.

### Loading Process

The `LoadFromJson()` method:

1. Checks whether the file exists.
2. Reads its contents.
3. Checks whether the file is empty.
4. Attempts to deserialize the JSON.
5. Returns the loaded object if successful.

If loading fails, the method returns `null` and reports the error through the Unity Console.

## 4. SaveBackupHandler.cs

### Purpose

`SaveBackupHandler` protects existing save files by maintaining up to two backup copies.

```csharp
private const int MAX_BACKUPS = 2;
```

### Backup Creation

The `CreateBackup()` method performs the following operations:

1. Checks whether the original save file exists.
2. Creates a `Backups` directory.
3. Copies the current save into a temporary backup.
4. Removes the oldest backup if necessary.
5. Moves the latest backup into the second backup position.
6. Promotes the temporary backup into the first backup position.

This creates a simple two-generation backup system.

### Backup Rotation

| Backup | Stored Data |
|---|---|
| `backup1` | Most recent backup |
| `backup2` | Previous backup |

Every successful update to an existing save creates a new backup before replacing the original file.

### Backup Restoration

The generic `RestoreLatestBackup<T>()` method attempts to restore the most recent valid backup.

It:

1. Checks the available backup files.
2. Reads each backup.
3. Attempts to deserialize its JSON.
4. Passes the resulting object through a validation function.
5. Selects the first valid backup.
6. Copies it into a temporary restoration file.
7. Replaces the original file or creates it if missing.

The validator allows different save data types to define their own validation requirements.

For example:

```csharp
data => data.worlds != null
```

This checks whether a recovered world catalog contains a valid world list.

## 5. WorldSaveData.cs

### Purpose

`WorldSaveData` defines the metadata stored for each individual world.

It also includes `WorldCatalogData`, which stores the collection of registered worlds.

### World Metadata

```csharp
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
```

| Field | Description |
|---|---|
| `worldID` | Unique identifier assigned to the world. |
| `worldName` | Display name of the world. |
| `worldSeed` | Seed used for procedural world generation. |
| `saveVersion` | Version identifier for the world save format. |
| `creationDate` | Date and time when the world was created. |
| `lastPlayedDate` | Date and time when the world was last played. |

### World Catalog

```csharp
[Serializable]
public class WorldCatalogData
{
    public List<WorldSaveData> worlds = new List<WorldSaveData>();
}
```

The world catalog stores the registered world metadata in a single collection.

It is saved separately from individual world metadata.

## 6. WorldSaveManager.cs

### Purpose

`WorldSaveManager` manages the global world catalog and individual world metadata files.

It is a static class, meaning its methods can be accessed without attaching it to a GameObject.

### Initialization

The `Initialize()` method prepares the save system.

Its responsibilities include:

1. Establishing the root save directory.
2. Creating the `Worlds` directory if necessary.
3. Loading the existing world catalog.
4. Attempting backup recovery if the catalog cannot be loaded.
5. Creating a new catalog only when one does not exist.
6. Ensuring the world list is initialized.
7. Validating registered worlds.
8. Returning whether initialization succeeded.

The manager does not intentionally overwrite an existing catalog when loading and backup recovery both fail.

### World Catalog Validation

`ValidateWorldCatalog()` checks registered worlds for:

- Null entries.
- Invalid world IDs.
- Duplicate world IDs.
- Empty world names.
- Missing world directories.
- Missing or invalid world metadata.
- Differences between catalog metadata and individual world metadata.

It also attempts to recover invalid metadata from backups.

If individual metadata is missing but the world remains registered, the manager attempts to recreate it using the catalog entry.

Invalid entries are skipped, while missing world directories do not automatically cause their catalog entries to be removed.

### World Management Methods

| Method | Description |
|---|---|
| `CreateWorld()` | Creates a new world directory and saves its metadata. |
| `GetWorldDirectory()` | Returns the directory path of a world. |
| `GetWorldFilePath()` | Returns the individual world metadata path. |
| `GetWorldCatalog()` | Returns the current world catalog. |
| `GetAllWorlds()` | Returns a copy of the registered world list. |
| `GetWorldByID()` | Finds a registered world using its ID. |
| `RenameWorld()` | Updates the world name in both metadata files. |
| `UpdateLastPlayedDate()` | Updates the world's last-played timestamp. |
| `DeleteWorld()` | Removes a world from the catalog and deletes its directory. |

### World Creation

When `CreateWorld()` is called:

1. A unique GUID is generated for the world.
2. The supplied world name and seed are assigned.
3. Creation and last-played dates are recorded.
4. A directory is created using the generated world ID.
5. Individual world metadata is saved.
6. The world is registered in the global catalog.

If either save operation fails, the manager attempts to roll back the creation.

### World Renaming

`RenameWorld()` updates both the individual world metadata and the global catalog.

If either save operation fails, it attempts to restore the previous name.

### Last Played Date

`UpdateLastPlayedDate()` records the current UTC timestamp.

The date is stored in round-trip format using:

```csharp
DateTime.UtcNow.ToString("o")
```

### World Deletion

`DeleteWorld()` uses a temporary directory to make the deletion process safer.

Its general sequence is:

1. Validates the supplied world ID.
2. Finds the registered world.
3. Temporarily moves the world directory.
4. Removes the world from the catalog.
5. Saves the updated catalog.
6. Permanently deletes the temporary directory.

If saving the updated catalog fails, the manager attempts to restore the world entry and directory.

## 7. Testing Setup

Two temporary tester components were created to verify the foundation.

### WorldSaveManagerTester.cs

**Purpose:** Tests the initialization of the world save system and retrieves registered worlds.

The tester:

1. Calls `WorldSaveManager.Initialize()`.
2. Checks whether initialization succeeds.
3. Retrieves all registered worlds.
4. Prints the total number of worlds.
5. Prints each world's name, ID, and seed.

Expected Console output:

```text
World Save System Initialized.
INITIALIZATION SUCCESSFUL
TOTAL REGISTERED WORLDS: [number]

WORLD: [World Name] | ID: [World ID] | SEED: [World Seed]
```

### SaveFileTester.cs

**Purpose:** Tests backup restoration and JSON loading using a separate test file.

The tester:

1. Attempts to restore the latest valid backup of `testWorld.json`.
2. Prints whether recovery succeeded.
3. Loads the restored JSON file.
4. Prints the world name and seed if loading succeeds.

This test uses its own `TestData` class and does not directly modify the actual world catalog.

## 8. Testing Procedures

The following tests were performed during the foundation implementation.

| Test | Expected Result |
|---|---|
| Normal initialization | Existing catalog loads successfully. |
| Missing catalog | A new catalog is created. |
| Missing world directory | Catalog entry is preserved. |
| Missing world metadata | Metadata is recreated from the catalog. |
| Backup creation | Existing save receives a backup before replacement. |
| Backup rotation | Only two backup generations are retained. |
| Corrupted latest backup | Recovery attempts the next valid backup. |
| Successful restoration | Restored JSON can be loaded. |
| Registered world retrieval | World metadata is printed in the Console. |

The tests confirmed that the foundation could initialize, maintain registered world metadata, create backups, and recover from the tested corrupted-backup scenario.

## 9. Troubleshooting

### World Save System Initialized appears multiple times

Check whether multiple objects are calling `WorldSaveManager.Initialize()`.

For example, the main menu may initialize the system while a temporary tester in the Game Scene also calls initialization.

Disable the tester component after testing to avoid unnecessary duplicate initialization logs.

### Catalog recovery failed

The catalog may be invalid and no backup may pass its validation function.

Check the `Worlds` directory and its `worlds.json` backup files.

The manager intentionally avoids replacing an existing unrecoverable catalog with a new empty catalog.

### Backup creation failed

Check whether the application can access the save directory and whether the existing file can be copied and replaced.

A failed backup prevents the existing save from being overwritten.

### World metadata could not be validated

Check whether the individual `world.json` file matches the expected world ID and contains a valid world name.

The manager may attempt backup recovery before skipping invalid metadata.

## 10. Completion Status

**Phase 1 — Save System Foundation: COMPLETE**

Implemented components:

- JSON save and load handling.
- Temporary file replacement.
- Automatic backup creation.
- Two-generation backup rotation.
- Generic backup restoration with validation.
- World catalog initialization.
- World metadata validation.
- Missing metadata recovery.
- World catalog and metadata management.
- Basic initialization and recovery testing.

### Next Phase

**Phase 2 — Multi-World Management**

The foundation established here supports the multi-world system, allowing players to create, register, select, rename, and delete individual worlds.

Later phases will extend the system to save actual gameplay progress, including player data, buildings, and world objects.