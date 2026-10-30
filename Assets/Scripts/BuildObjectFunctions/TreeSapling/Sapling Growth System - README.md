# Sapling Growth System

## Overview

The Sapling Growth System allows players to plant saplings using the buildable item system. After placement, each sapling starts a growth timer and eventually transforms into a randomly selected tree prefab.

The system integrates with `WorldGenerator` to preserve sapling and tree data when building chunks unload and reload.

## Features

- Plant saplings using buildable items.
- Automatically start a growth timer after planting.
- Randomly select a tree prefab for each sapling.
- Transform grown saplings into trees.
- Preserve building identity when replacing saplings with trees.
- Restore saplings or grown trees when their chunks reload.
- Keep each sapling's growth deadline and selected tree prefab in runtime save data.

## Scripts

### `SaplingBehavior.cs`

**Purpose:** Controls an individual sapling's growth.

Responsibilities:
- Finds the associated `BuildingObject` in the prefab hierarchy.
- Locates the scene's `WorldGenerator` and `TreeController`.
- Retrieves the sapling's `BuildingSaveData`.
- Starts the growth timer if the sapling has not already grown.
- Checks whether the growth deadline has been reached.
- Instantiates the selected tree prefab.
- Preserves the original `itemID` and `buildingID`.
- Destroys the original sapling after successful replacement.

### `TreeController.cs`

**Purpose:** Provides tree prefabs and growth-time settings.

Inspector fields:

| Field | Description |
|---|---|
| `treePrefabs` | Array of tree prefabs that saplings can grow into. |
| `treeParent` | Optional tree parent reference; not currently used by `SaplingBehavior`. |
| `minimumGrowthTime` | Minimum growth duration in real seconds. |
| `maximumGrowthTime` | Maximum growth duration in real seconds. |

The default growth duration is between 60 and 180 real seconds.

`GetRandomTreePrefab()` selects a prefab from the assigned array.

### `WorldGenerator.cs`

**Purpose:** Manages building data and chunk restoration.

Relevant methods:

- `RegisterBuilding()` creates runtime save data for a newly placed building.
- `TryGetBuildingSaveData()` retrieves a building's saved data.
- `StartSaplingGrowth()` initializes the growth deadline and selected tree prefab.
- `RegisterSaplingGrowth()` updates the growth deadline.
- `MarkSaplingGrown()` marks a sapling as grown and records its tree prefab.
- `RestoreBuildingsInChunk()` restores the original sapling or grown tree, depending on the saved growth state.

### `BuildingObject.cs`

**Purpose:** Identifies placed buildings and grown trees.

| Field | Description |
|---|---|
| `itemID` | Identifies the original buildable item. |
| `buildingID` | Unique identity used to associate the object with its saved data. |

### `BuildingSaveData.cs`

**Purpose:** Stores runtime building and sapling growth information.

Relevant fields:

| Field | Description |
|---|---|
| `buildingID` | Unique identity of the placed object. |
| `itemID` | Original buildable item's ID. |
| `position` | Saved world position. |
| `rotation` | Saved world rotation. |
| `chunkCoordinate` | Chunk containing the object. |
| `grownPrefab` | Selected tree prefab for the sapling. |
| `growthDeadline` | Time at which the sapling should finish growing. |
| `hasGrown` | Indicates whether the sapling has transformed into a tree. |

## Unity Setup

### 1. Sapling Building Prefab

The prefab instantiated when the player plants a sapling should contain:

- `BuildingObject`
- `SaplingBehavior`
- Appropriate sprite renderers and colliders

`BuildingObject` may be on the root or a child because `SaplingBehavior` searches the prefab hierarchy for it.

### 2. Buildable Item

Configure the sapling item in the existing item system:

- Enable `isBuildable`.
- Assign the sapling building prefab to `buildingPrefab`.
- Assign the placement preview to `ghostPrefab`.
- Ensure the item has the correct item ID through the existing `ItemDictionary` setup.

### 3. Tree Controller

Place `TreeController` on a scene object that remains available while chunks load and unload.

Configure:
- `treePrefabs`: Assign the regular tree prefabs that saplings may become.
- `minimumGrowthTime`: Set the minimum growth duration in seconds.
- `maximumGrowthTime`: Set the maximum growth duration in seconds.

Ensure the array contains valid prefabs.

### 4. World Generator

The scene must contain an active `WorldGenerator` with the existing building chunk and item dictionary configuration.

The `PlacementController` registers a newly planted sapling before its growth behavior attempts to retrieve the corresponding save data.

## Growth Process

1. The player selects the sapling item.
2. The player places the sapling in the world.
3. `PlacementController` assigns its `itemID` and unique `buildingID`.
4. `WorldGenerator.RegisterBuilding()` creates the sapling's runtime save data.
5. `SaplingBehavior` finds the associated `BuildingObject` and initializes growth.
6. `TreeController` provides a randomly selected tree prefab.
7. `WorldGenerator` stores the growth deadline and selected prefab.
8. Once the deadline is reached, `SaplingBehavior` instantiates the selected tree.
9. The tree retains the original building identity.
10. The sapling is destroyed, and the saved data is marked as grown.

## Chunk Unloading and Restoration

When a building chunk unloads, its saved building data remains in the `WorldGenerator`'s runtime dictionaries.

When the chunk reloads:

- If `hasGrown` is `false`, the original sapling building prefab is restored.
- If `hasGrown` is `true` and `grownPrefab` is available, the grown tree prefab is restored instead.

The restored object receives the saved `itemID` and `buildingID`.

This prevents grown trees from reverting to saplings during chunk unloading and reloading within the same play session.

## Important Limitations

- **Runtime-only persistence:** Growth data is currently stored in memory. Closing or restarting the game does not guarantee that sapling growth or grown trees will persist.
- **Real-time growth:** The growth deadline uses `Time.realtimeSinceStartup`. The timer continues independently of `Time.timeScale`, but the stored deadline is not suitable for persistence across game restarts.
- **Prefab configuration:** Missing or invalid tree prefabs can prevent successful growth.
- **Tree identity:** Grown trees retain the original sapling's building identity so the existing building save system can track them.

## Testing Checklist

- [ ] Plant a sapling successfully.
- [ ] Confirm that the growth timer starts.
- [ ] Confirm that the sapling becomes a tree.
- [ ] Plant multiple saplings and verify that different tree prefabs can be selected.
- [ ] Unload and reload a chunk containing an ungrown sapling.
- [ ] Unload and reload a chunk containing a grown tree.
- [ ] Confirm that grown trees do not revert to saplings.
- [ ] Confirm that restored objects retain their building IDs.
- [ ] Check the Unity Console for errors.

## Future Improvements

- Persist growth deadlines across game restarts using the world save system.
- Add visual growth stages.
- Add environmental conditions that affect growth.
- Add sapling removal and harvesting interactions.