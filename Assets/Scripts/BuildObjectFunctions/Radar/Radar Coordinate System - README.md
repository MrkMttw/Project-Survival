# Radar Coordinate System

## Overview

The Radar Coordinate System allows players to discover important world locations as they upgrade their radar. Coordinates are displayed in a single TextMeshPro text box and use whole numbers for readability.

The system supports:
- Campfire location tracking.
- Dungeon location tracking.
- Radar-level-based coordinate unlocking.
- Automatic coordinate refresh when buildings are placed or relocated.
- Coordinate removal when a building is retrieved.
- Structure coordinates that remain available when their chunks are unloaded.

## Features

### Radar Level Progression

| Radar Level | Iron Required | Unlocked Coordinates |
|---|---:|---|
| Level 0 | 3 | None |
| Level 1 | 6 | Campfire |
| Level 2 | 9 | Campfire, Dungeon 1 |
| Level 3 | 12 | Campfire, Dungeons 1–2 |
| Level 4 | 15 | Campfire, Dungeons 1–3 |
| Level 5 | Max level | Campfire, Dungeons 1–4 |

Iron requirements represent the cost to advance from the current level to the next level. Radar Level 5 is the maximum level.

### Coordinate Display

Coordinates are displayed using the following format:

`Campfire: 125, -42`

The X and Y coordinates are rounded to the nearest whole number. Parentheses are not used.

Only locations unlocked by the current radar level are displayed.

## Main Scripts

### RadarController.cs

Manages radar progression and coordinate information.

Responsibilities:
- Tracks radar level and iron progression.
- Determines which coordinates are unlocked.
- Retrieves campfire coordinates from saved building positions.
- Retrieves dungeon coordinates from the structure-spawning system.
- Sends coordinate updates to the radar UI.

### RadarUI.cs

Manages the radar interface.

Responsibilities:
- Displays the current radar level.
- Displays iron progression.
- Updates the progress bar.
- Displays the unlocked location coordinates.

The coordinate display is refreshed when requested by the radar controller rather than being checked continuously every frame.

### WorldGenerator.cs

Manages world chunks and placed-building data.

Responsibilities:
- Registers newly placed buildings.
- Stores building IDs, item IDs, positions, rotations, and chunk coordinates.
- Updates saved building positions when buildings are relocated.
- Removes building records when buildings are retrieved.
- Restores saved buildings when their chunks are loaded again.

The radar reads the saved position of the placed campfire instead of relying on its prefab asset.

### StructureSpawner.cs

Manages generated structures and their world positions.

Responsibilities:
- Generates the deterministic structure layout.
- Tracks structure positions independently of whether their GameObjects are currently loaded.
- Provides structure coordinates to the radar.

### PlacementController.cs

Handles building placement and relocation.

Responsibilities:
- Registers newly placed buildings with the world generator.
- Updates saved building data after relocation.
- Requests a radar coordinate refresh after placement or relocation.

### WrenchFunction.cs

Handles building relocation and retrieval through the wrench tool.

Responsibilities:
- Starts building relocation.
- Removes the building's saved record when retrieval succeeds.
- Requests a radar coordinate refresh after retrieval.

## Unity Setup

### 1. RadarController

Assign the following references in the Inspector:

- **World Generator:** The scene's `WorldGenerator` component.
- **Structure Spawner:** The scene's `StructureSpawner` component.
- **Coordinate Entries:** Configure each location's name, required radar level, and location source.

Configure the coordinate entries as follows:

| Structure Name | Required Level | Location Source |
|---|---:|---|
| Campfire | 1 | Campfire |
| Dungeon 1 | 2 | Dungeon |
| Dungeon 2 | 3 | Dungeon |
| Dungeon 3 | 4 | Dungeon |
| Dungeon 4 | 5 | Dungeon |

For the Campfire entry, assign the campfire's actual item ID. Do not use the prefab asset as the location source.

For each dungeon entry, assign its corresponding `StructureSpawnData` asset.

### 2. RadarUI

Assign the radar level text, iron progression text, and progress bar.

The coordinate text uses a TextMeshPro UI object named `CoordinatesText`. If the reference is not assigned directly, the script can locate the object by its configured name.

### 3. Building System

Ensure that:
- Placeable structures have a `BuildingObject` component.
- Building placement assigns the correct item ID and a unique building ID.
- Relocation updates the building's saved position.
- Retrieval removes the corresponding building record.
- The radar refresh method is available and called after the relevant building operations.

## How It Works

1. The player collects iron and upgrades the radar.
2. The radar level determines which coordinate entries are eligible for display.
3. The radar retrieves each unlocked location's position.
4. Campfire coordinates come from the world generator's saved building data.
5. Dungeon coordinates come from the structure spawner's generated layout.
6. The radar formats the coordinates and displays them in the UI.
7. When a relevant building operation occurs, the coordinate display is refreshed.

## Performance Considerations

The system does not need to search for coordinates every frame.

Instead, coordinate updates are requested after relevant operations, such as placing, relocating, or retrieving a building. Dungeon positions can be retrieved without requiring their GameObjects to remain loaded.

## Current Limitations

- Building records are currently stored in runtime memory. Persistent saving across closing and reopening the game requires a separate disk-based save system.
- Campfire lookup by item ID assumes only one matching campfire exists.
- Coordinates are rounded to whole numbers, so they may differ slightly from the building's exact world position.
- Coordinate visibility depends on radar progression and the availability of the corresponding saved or generated location data.

## Future Improvements

- Persist building coordinates across game sessions.
- Support multiple campfires using unique building IDs.
- Add additional discoverable structures.
- Add map navigation or waypoint indicators for unlocked locations.
