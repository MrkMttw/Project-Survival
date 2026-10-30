# Structure Generator & Runtime Persistence

## Overview

The Structure Generator is responsible for procedurally placing structures throughout the world using deterministic generation.

It also manages structure loading and unloading based on the player's chunk position.

The system includes runtime persistence for:
- Destroyed objects inside generated structures.
- Defeated bosses inside generated structures.
- Previously generated structure positions.

Progress is preserved during the current Play Mode session, even when structures unload and reload.

**Note:** This system does not currently save progress to disk. All persistence data is stored in runtime memory and resets when the game session ends.

---

## Scripts

### 1. StructureObjectIdentity.cs

Identifies objects belonging to a generated structure.

**Fields:**

| Field | Description |
|---|---|
| `structureID` | Identifies the generated structure an object belongs to. |
| `objectID` | Uniquely identifies an object inside that structure. |
| `isBoss` | Determines whether the object is a persistent boss. |

**Usage:**

- Destructible objects receive this component automatically when their structure is instantiated.
- Bosses must have this component manually added to the same GameObject as their `EnemyController`.
- Ordinary enemies do not need this component.

For a boss, configure:

- `Object ID`: `boss`
- `Is Boss`: Enabled
- `Structure ID`: Leave empty. It is assigned automatically.

---

### 2. StructureSpawnData.cs

A ScriptableObject that defines the generation settings for a structure.

**Fields:**

| Field | Description |
|---|---|
| `prefab` | Structure prefab to generate. |
| `maxWorldCount` | Maximum number of instances allowed in the world. |
| `minSpawnDistance` | Minimum distance from the world origin where the structure can spawn. |
| `clearRadius` | Radius around the structure where natural objects are removed. |

**Creating an asset:**

1. Right-click inside the Project window.
2. Select `Create`.
3. Select `World > Structure Spawn Data`.
4. Configure the structure prefab and generation settings.

---

### 3. StructureSpawner.cs

The main controller responsible for structure generation, loading, unloading, and runtime persistence.

#### Generation

Uses deterministic generation based on:
- World seed.
- Chunk coordinates.
- Structure data index.
- Structure asset name.

Candidate positions are generated and sorted by priority before structures are selected.

This ensures that exploration order does not determine structure placement.

Generation settings:

| Field | Description |
|---|---|
| `worldGenerator` | Reference to the WorldGenerator. |
| `structureParent` | Parent Transform for generated structures. |
| `structures` | List of StructureSpawnData assets. |
| `spawnAttempts` | Number of candidate positions generated per structure type per chunk. |
| `maxGenerationDistance` | Maximum generation distance from the world origin. |
| `structureLoadDistance` | Number of chunks around the player where structures remain loaded. |

#### Structure Identification

Every generated structure receives a unique runtime ID based on its structure data index and chunk coordinates.

Format:

`dataIndex_chunkX_chunkY`

Example:

`0_5_-2`

This ID is used to associate destroyed objects and defeated bosses with their original structure.

#### Structure Loading and Unloading

`ProcessChunk()` processes structure records when a world chunk loads.

`UpdateStructureLoading()` checks the player's chunk position and determines which structures should remain loaded.

Structures outside the configured loading distance are destroyed from the scene, but their records remain in memory.

When the player returns, the same structures are instantiated at their original positions.

#### Destroyed Object Persistence

The system uses:

`HashSet<string> destroyedStructureObjects`

When a structure loads, its destructible objects are identified and assigned runtime IDs.

When an object is destroyed, its ID is recorded through:

`MarkStructureObjectDestroyed()`

When the structure reloads, previously destroyed objects are removed instead of being restored.

#### Boss Defeat Persistence

The system uses:

`HashSet<string> defeatedStructureBosses`

Bosses are identified using `StructureObjectIdentity`.

When a boss dies, `MarkBossDefeated()` records its defeat.

When the structure reloads, the system checks whether the boss was previously defeated. If so, the boss is removed instead of being restored.

#### Important Methods

| Method | Purpose |
|---|---|
| `ProcessChunk()` | Processes structures associated with a loaded chunk. |
| `GenerateDeterministicLayout()` | Generates the structure layout. |
| `SpawnRecord()` | Instantiates a structure and restores its runtime progress. |
| `UnloadChunk()` | Allows a chunk to be processed again after unloading. |
| `UpdateStructureLoading()` | Controls structure loading and unloading. |
| `MarkStructureObjectDestroyed()` | Records destroyed structure objects. |
| `MarkBossDefeated()` | Records defeated structure bosses. |
| `TryGetStructurePosition()` | Retrieves a generated structure's coordinates. |

---

## Integration With Other Systems

### BreakSystem

When a destructible object breaks, `BreakSystem` checks for `StructureObjectIdentity`.

If one exists, it notifies `StructureSpawner` through:

`MarkStructureObjectDestroyed()`

The existing `WorldObjectIdentity` handling remains unchanged, preserving natural-object destruction persistence.

### EnemyController

When an enemy dies, `EnemyController` checks whether it has a `StructureObjectIdentity` component.

If the identity exists and `isBoss` is enabled, it notifies `StructureSpawner` through:

`MarkBossDefeated()`

Ordinary enemies continue using the existing death behavior.

---

## Unity Hierarchy

Recommended hierarchy:

```text
OverWorld
├── NaturalObjects
├── PlayerBuilds
└── Structures
```

The `StructureSpawner` should reference:
- The `WorldGenerator` component.
- The `Structures` Transform as its `structureParent`.
- The configured StructureSpawnData assets.

---

## Boss Prefab Setup

The boss must have both components on the same GameObject:

```text
Boss
├── EnemyController
├── StructureObjectIdentity
├── Rigidbody2D
├── Collider2D
└── Other Components
```

Configure `StructureObjectIdentity`:

| Field | Value |
|---|---|
| Structure ID | Empty |
| Object ID | boss |
| Is Boss | Enabled |

The StructureSpawner automatically assigns the structure ID during instantiation.

---

## Runtime Persistence Behavior

| Event | Result |
|---|---|
| Structure is generated | Position is recorded. |
| Structure unloads | Structure GameObject is destroyed. |
| Structure reloads | Original structure is instantiated. |
| Destructible object was broken | Object remains absent. |
| Boss was defeated | Boss remains absent. |
| Object was not destroyed | Object is restored normally. |
| Play Mode ends | Runtime persistence data is lost. |

---

## Current Limitations

- No permanent disk saving or loading.
- Only destructible objects and designated bosses have persistence.
- Ordinary enemies are not tracked as defeated.
- Destructible object IDs currently depend on their order in the `BreakableMaterial` array returned by Unity.
- Changes to other interactable objects are not currently preserved.
- Boss drops are generated when the boss dies and are not independently saved.

---

## Future Improvements

Potential extensions:
- Persistent loot containers.
- Persistent doors and switches.
- Individual enemy defeat tracking.
- Structure interior state persistence.
- Permanent world saving and loading.
- Stable object IDs independent of hierarchy ordering.