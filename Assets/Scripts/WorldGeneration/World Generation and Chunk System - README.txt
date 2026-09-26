# World Generation and Chunk System

## Overview

The `WorldGenerator` controls procedural world generation, chunk loading/unloading, natural object persistence, building persistence, and world seed generation.

The world is divided into square chunks. Only chunks near the player are loaded at a time.

The system currently supports:

* Procedural natural object generation
* Deterministic world generation using a world seed
* Chunk loading and unloading
* Persistent natural object positions
* Persistent destroyed natural objects
* Persistent player-built objects
* Building relocation between chunks
* Building retrieval/removal
* Separate chunk parents for natural objects and player buildings
* Optional spacing validation for spawned objects

The current persistence system is **runtime-only**. Data is kept while the game is running but is not yet saved to disk.

---

# Hierarchy

The world is organized using the following structure:

```text
OverWorld
├── NaturalObjects
│   ├── Chunk 0, 0
│   ├── Chunk 1, 0
│   ├── Chunk 0, 1
│   └── ...
│
└── PlayerBuilds
    ├── Chunk 0, 0
    ├── Chunk 1, 0
    ├── Chunk 0, 1
    └── ...
```

`NaturalObjects` contains procedurally generated world objects such as trees, rocks, and decorations.

`PlayerBuilds` contains objects placed by the player.

Each chunk is created dynamically when it is needed.

---

# WorldGenerator

The `WorldGenerator` is responsible for managing the entire chunk system.

## Inspector References

### Player

```text
player
```

The player's `Transform`.

The player's position determines which chunk is currently active.

### World Parent

```text
overWorld
naturalObjectsParent
buildingParent
```

These references control where world objects are organized.

* `overWorld` is the main world parent.
* `naturalObjectsParent` contains generated natural-object chunks.
* `buildingParent` contains player-building chunks.

### Item Dictionary

```text
itemDictionary
```

Used when restoring player-built objects.

The system uses the building's saved `itemID` to find the corresponding item and its `buildingPrefab`.

### Chunk Settings

```text
chunkSize
loadDistance
```

`chunkSize` determines the size of each chunk.

Default:

```text
16
```

`loadDistance` determines how many chunks around the player's current chunk remain loaded.

For example:

```text
loadDistance = 1
```

loads a 3 × 3 area around the player.

### World

```text
worldSeed
worldData
```

`worldSeed` controls deterministic natural-world generation.

The same seed produces the same generated world.

`WorldSaveData` currently stores the seed value in memory, but a complete disk save/load system has not been implemented yet.

### Spawn Data

```text
spawnData
```

Contains the `WorldSpawnData` presets used to determine what objects can naturally spawn.

---

# Chunk Coordinates

Chunks use `Vector2Int` coordinates.

The player's world position is converted into a chunk coordinate using:

```text
world position / chunk size
```

For example, with a chunk size of `16`:

```text
Chunk 0, 0
X: 0 to 16
Y: 0 to 16
```

The next chunk is:

```text
Chunk 1, 0
X: 16 to 32
Y: 0 to 16
```

Negative coordinates are also supported.

---

# Chunk Loading

When the game starts, `WorldGenerator` determines the player's current chunk.

It then loads all required chunks around the player.

When the player moves into another chunk, the required chunk area is recalculated.

Chunks that are no longer inside the load distance are unloaded.

The system keeps separate dictionaries for:

```text
loadedChunks
loadedBuildingChunks
```

This allows natural objects and player buildings to be managed independently.

---

# Natural Object Generation

Natural objects are generated using `WorldSpawnData`.

Each spawn preset can contain:

```text
Prefabs
Generation
    Min Spawn Count
    Max Spawn Count
    Min Spacing
    Ignore Spacing Validation
```

The generator determines how many objects to create and randomly selects their positions and prefabs.

Each generated object receives a `WorldObjectIdentity`.

---

# WorldObjectIdentity

`WorldObjectIdentity` identifies a naturally generated object.

It stores:

```text
chunkCoordinate
spawnDataIndex
objectIndex
```

These values allow the generator to identify the exact object later.

The object ID is constructed using:

```text
spawnDataIndex_objectIndex
```

Example:

```text
2_5
```

This means:

```text
Spawn Data Index = 2
Object Index = 5
```

---

# Persistent Natural Objects

The generator remembers the positions and prefabs of generated natural objects.

This prevents a chunk from generating completely different object positions after being unloaded and loaded again.

The system stores:

```text
persistentObjectPositions
persistentObjectPrefabs
```

When a chunk is loaded for the first time:

```text
GenerateChunk()
```

is used.

When the chunk has already generated objects:

```text
GenerateChunkFromSavedPositions()
```

restores those objects using their previously stored positions and prefabs.

---

# Destroyed Natural Objects

Destroyed natural objects are remembered using:

```text
destroyedObjects
```

When a natural object is destroyed, its `WorldObjectIdentity` is sent to:

```text
MarkObjectDestroyed()
```

The object ID is stored for its chunk.

When the chunk is loaded again, the generator checks:

```text
IsObjectDestroyed()
```

Destroyed objects are skipped instead of being spawned again.

This allows trees, rocks, and other destructible natural objects to remain destroyed while their chunk is unloaded and reloaded.

---

# Spacing Validation

Natural objects normally use minimum spacing validation.

The generator checks:

* Other objects in the same chunk
* Objects in neighboring chunks

This helps prevent objects from spawning too close to each other across chunk boundaries.

The check can be disabled per `WorldSpawnData` preset with:

```text
Ignore Spacing Validation
```

When enabled, the preset skips spacing validation completely.

This is useful for decorative objects that are allowed to spawn close together.

---

# World Seed

The world seed controls procedural generation.

The generator uses the seed together with:

```text
chunk X
chunk Y
spawn data index
```

to initialize the random generator.

This means the same:

```text
World Seed
+
Chunk Coordinate
+
Spawn Data
```

produces the same natural-world generation.

Changing the seed produces a different world.

## Setting a Seed

The seed can be changed with:

```csharp
SetWorldSeed(int seed)
```

## Generating a New Seed

A random seed can be generated with:

```csharp
GenerateNewWorldSeed()
```

The generated value becomes the new `worldSeed`.

---

# Player Building Persistence

Player-built objects are stored separately from natural objects.

The generator uses:

```text
savedBuildings
```

to remember buildings by chunk.

Each building is represented by `BuildingSaveData`.

The saved information includes:

```text
buildingID
itemID
position
rotation
chunkCoordinate
```

---

# Building Chunks

Player buildings are organized under:

```text
OverWorld
└── PlayerBuilds
    ├── Chunk X, Y
    └── ...
```

The method:

```csharp
GetBuildingChunkParent()
```

creates the required chunk parent if it does not already exist.

For example:

```text
PlayerBuilds
└── Chunk 2, -1
```

contains buildings currently saved inside chunk `2, -1`.

---

# Registering Buildings

When a building is created, it can be registered using:

```csharp
RegisterBuilding()
```

The generator determines the building's current chunk using its world position.

The building's:

```text
ID
Item ID
Position
Rotation
Chunk
```

are then stored.

---

# Updating Buildings

When a building is relocated, the generator uses:

```csharp
UpdateBuilding()
```

The old building record is removed.

The building is then stored under its new chunk.

This allows a building to be moved from:

```text
Chunk 0, 0
```

to:

```text
Chunk 1, 0
```

without losing its saved data.

---

# Removing Buildings

When a building is retrieved or removed, the system uses:

```csharp
RemoveBuilding()
```

The building's `buildingID` is searched for and removed from the saved building data.

This prevents the building from returning when its chunk is loaded again.

---

# Restoring Buildings

When a building chunk is loaded:

```csharp
RestoreBuildingsInChunk()
```

checks the saved building data.

The system:

1. Finds the saved `itemID`.
2. Gets the corresponding item from `ItemDictionary`.
3. Gets the item's `buildingPrefab`.
4. Instantiates the building.
5. Restores its position.
6. Restores its rotation.
7. Restores its `itemID`.
8. Restores its `buildingID`.

This allows buildings to survive chunk unloading and loading.

---

# Runtime Persistence

The current system stores persistence information in memory.

For example:

```text
destroyedObjects
persistentObjectPositions
persistentObjectPrefabs
savedBuildings
```

These dictionaries remain available while the game is running.

Therefore:

```text
Chunk unload
    ↓
Data remains in memory
    ↓
Chunk loads again
    ↓
Objects are restored
```

However:

```text
Close Game
    ↓
Runtime memory is cleared
```

The current system does **not** yet save this information to a file.

A proper save/load system can be added later.

---

# Gizmos

`WorldGenerator` draws chunk boundaries in the Unity Editor.

Each visible chunk displays:

```text
Chunk X, Y
```

This makes it easier to see chunk boundaries and determine which chunk the player is currently inside.

---

# Related Scripts

The chunk system works together with:

```text
WorldGenerator
WorldSpawnData
WorldObjectIdentity
WorldSaveData
BuildingSaveData
BuildingObject
ItemDictionary
Item
PlacementController
WrenchFunction
```

### WorldGenerator

Controls chunk loading, generation, persistence, seeds, and building restoration.

### WorldSpawnData

Defines what natural objects can spawn and how they are generated.

### WorldObjectIdentity

Identifies individual generated natural objects.

### WorldSaveData

Currently stores the world seed data structure.

### BuildingSaveData

Stores the information required to restore a player-built object.

### BuildingObject

Identifies and stores information about player-built objects.

### ItemDictionary

Allows saved buildings to find their original item and `buildingPrefab`.

### PlacementController

Handles player building placement and relocation.

### WrenchFunction

Handles retrieving and relocating existing buildings.

---

# Current System Flow

## Natural Objects

```text
Player enters chunk
        ↓
WorldGenerator loads chunk
        ↓
Check persistent data
        ↓
First visit?
   ┌────┴────┐
  Yes        No
   ↓          ↓
Generate    Restore
objects     saved objects
   ↓          ↓
Store       Instantiate
positions   saved objects
```

## Destroyed Objects

```text
Natural object destroyed
        ↓
WorldObjectIdentity
        ↓
MarkObjectDestroyed()
        ↓
Store object ID
        ↓
Chunk unloads
        ↓
Chunk reloads
        ↓
Destroyed object is skipped
```

## Player Buildings

```text
Place building
      ↓
RegisterBuilding()
      ↓
Save building data in chunk
      ↓
Chunk unloads
      ↓
Building GameObject destroyed
      ↓
Chunk reloads
      ↓
RestoreBuildingsInChunk()
      ↓
Building recreated
```

## Relocated Building

```text
Building relocated
      ↓
New world position
      ↓
New chunk calculated
      ↓
UpdateBuilding()
      ↓
Old chunk data removed
      ↓
New chunk data added
      ↓
Building moves with its new chunk
```

# Current Limitations

The current system does not yet provide permanent save files.

The following data is currently runtime-only:

* World seed
* Destroyed natural objects
* Natural object positions
* Natural object prefab selections
* Player buildings
* Building positions
* Building rotations

Permanent world saving can be implemented later without changing the basic chunk-generation concept.