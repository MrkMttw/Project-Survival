# Phase 4 – Natural World Persistence

## Overview

Phase 4 implements Natural World Persistence in Project Survival.

The system allows destroyed natural objects, such as trees and rocks, to remain destroyed even after their chunks unload, the game closes, or the player reopens a world.

Each world maintains its own world-state data, ensuring that changes are isolated between different saved worlds.

## Objectives

- Persist destroyed natural objects.
- Restore destroyed-object data when loading a world.
- Prevent destroyed objects from respawning after chunk reloads.
- Keep world-state data separate from world metadata and player data.
- Support multiple independent worlds through `worldID`.
- Preserve the existing deterministic world-generation system.
- Avoid unnecessary save data by storing object identifiers instead of prefab references or world positions.
- Use explicit saving instead of autosaving.

## Architecture

The save system uses separate JSON files for different types of data.

```text
Worlds/
│
├── worlds.json
│
├── World_ID_1/
│   ├── world.json
│   ├── player.json
│   └── world_state.json
│
└── World_ID_2/
    ├── world.json
    ├── player.json
    └── world_state.json
```

### Save File Responsibilities

| File | Responsibility |
|---|---|
| `worlds.json` | Stores the world catalog. |
| `world.json` | Stores individual world metadata and settings. |
| `player.json` | Stores player position, stats, inventory, and hotbar. |
| `world_state.json` | Stores destroyed natural objects. |

This separation makes the save architecture easier to maintain and expand in future phases.

## Implemented Scripts

### 1. WorldStateData.cs

Defines the data structures used to store natural world changes.

**WorldStateData**
- `worldID`: Identifies the world associated with the saved data.
- `destroyedObjects`: Stores the list of destroyed natural objects.

**DestroyedObjectSaveData**
- `chunkX`: X coordinate of the object's chunk.
- `chunkY`: Y coordinate of the object's chunk.
- `objectID`: Unique identifier of the destroyed object.

Only the object's chunk coordinates and identifier are stored. Its position and prefab are not required because the existing deterministic generation system recreates the same natural objects.

### 2. WorldSaveManager.cs

Extended the existing world save manager with world-state functionality.

Implemented methods:

- `GetWorldStateFilePath()`
  - Gets the location of the world's `world_state.json`.

- `LoadWorldState()`
  - Loads the saved world-state data.
  - Creates an empty world-state object when no save file exists.
  - Validates the loaded data.
  - Attempts backup recovery when the data is invalid.

- `SaveWorldState()`
  - Saves the current world-state data.
  - Validates the associated world ID before saving.

World-state data is stored inside the corresponding world's directory.

### 3. WorldGenerator.cs

Integrated destroyed-object persistence into the existing world-generation system.

Implemented functionality:

- Loads destroyed-object data when initializing a selected world.
- Reconstructs the runtime destroyed-object dictionary.
- Updates world-state data when a natural object is destroyed.
- Checks whether generated objects have already been destroyed.
- Provides an explicit method for saving current world-state data.

Important methods:

- `LoadDestroyedObjectData()`
  - Restores previously destroyed-object identifiers.

- `MarkObjectDestroyed()`
  - Marks a natural object as destroyed and updates the in-memory world-state data.

- `SaveCurrentWorldState()`
  - Sends the current world-state data to `WorldSaveManager` for saving.

The existing deterministic candidate-generation algorithm was preserved.

### 4. BreakSystem.cs

The existing destruction workflow was inspected.

No additional modification was necessary because the destruction process already connects to `WorldGenerator` through `WorldObjectIdentity`.

When a natural object is successfully destroyed, `MarkObjectDestroyed()` records its identifier in the world-state data.

### 5. PlayerSaveManager.cs

Integrated world-state saving into the existing Save & Exit functionality.

Added a public `WorldGenerator` reference that can be assigned through the Unity Inspector.

Updated `ExitToMainMenu()` to:

1. Save player data.
2. Save current world-state data.
3. Check whether both operations succeeded.
4. Return to the main menu only when both saves succeed.

If either save fails, the player remains in the game.

## Saving Workflow

```text
Player clicks Save & Exit
          |
          v
     SavePlayer()
          |
          v
 SaveCurrentWorldState()
          |
          v
 Both saves successful?
       /       \
     Yes        No
      |          |
      v          v
 Main Menu   Stay in Game
```

World-state changes are updated in memory when objects are destroyed. They are written to disk when the player uses the explicit Save & Exit action.

**No autosave functionality was introduced.**

## Loading Workflow

When a selected world is initialized:

1. `WorldGenerator` retrieves the selected world's information.
2. The world seed is initialized.
3. `LoadDestroyedObjectData()` loads the corresponding `world_state.json`.
4. The destroyed-object dictionary is reconstructed.
5. Structure layout initialization takes place.
6. Chunk generation begins.
7. Previously destroyed objects are excluded from generation.

This ensures that saved world changes are available before natural objects are generated.

## Compatibility and Preservation

The implementation preserves the existing systems:

- Deterministic natural object generation.
- Chunk loading and unloading.
- World seeds.
- Natural object identifiers.
- Structure clear-area exclusions.
- Existing player save functionality.
- Existing building persistence.
- Multiple-world support.

Structure clear-area exclusions continue to be recalculated during world initialization and are not unnecessarily stored as destroyed natural objects.

## Testing and Results

The following tests were performed:

| Test | Result |
|---|---|
| Destroy a tree | Passed |
| Destroy a rock | Passed |
| Save using Save & Exit | Passed |
| Close and reopen the game | Passed |
| Reload the same world | Passed |
| Verify destroyed objects remain destroyed | Passed |

Destroyed natural objects successfully remained destroyed after reopening the saved world.

## Phase 4 Completion

**Status: COMPLETE**

Natural World Persistence is implemented and tested.

The game can now preserve destroyed natural objects across chunk reloads and game restarts while maintaining separate world-state data for each saved world.

## Future Expansion

The separated world-state architecture allows future systems to be added without unnecessarily expanding `world.json`.

Potential future additions include:

- Harvested resource persistence.
- Environmental changes.
- World object state changes.
- Other persistent natural-world interactions.

These features can be implemented in future phases without changing the core purpose of the current system.