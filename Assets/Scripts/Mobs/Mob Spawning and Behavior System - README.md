# Mob Spawning and Behavior System

## Overview

The Mob Spawning and Behavior System manages hostile and passive mobs throughout the world. It uses a centralized spawning controller that coordinates mob populations across all currently loaded chunks.

Instead of having individual spawning routines for every chunk, the system uses one global controller to manage spawning, population limits, mob registration, and despawning.

## System Components

### 1. MobSpawnerController.cs

The central manager responsible for controlling mob spawning and despawning.

**Responsibilities:**
- Manages hostile and passive mob spawning.
- Uses centralized spawning routines instead of individual chunk routines.
- Maintains separate global population limits for hostile and passive mobs.
- Selects mob prefabs using configurable spawn chances.
- Checks hostile spawning conditions, including day and time requirements.
- Spawns mobs throughout the entire loaded chunk area.
- Prevents mobs from spawning inside the player's protection radius.
- Registers and tracks spawned mobs.
- Automatically despawns mobs outside the loaded chunk area.
- Excludes boss mobs from regular spawning and despawning.

### 2. ChunkMobSpawner.cs

A component attached to loaded chunk objects.

**Responsibilities:**
- Registers loaded chunks with MobSpawnerController.
- Provides chunk coordinates for spawn position calculations.
- Tracks which chunks are currently loaded.
- Unregisters chunks when they are unloaded.
- Does not destroy existing mobs during chunk cleanup.

### 3. MobIdentity.cs

Stores identification information for spawned mobs.

**Fields:**
- `mobType` – Identifies whether the mob is hostile or passive.
- `originalChunkCoordinate` – Stores the coordinates of the chunk where the mob originally spawned.

The original chunk coordinate is identification metadata and does not determine the mob's current location or ownership.

### 4. HostileMobBehavior.cs

Controls hostile mob behavior.

**Responsibilities:**
- Manages enemy health and damage.
- Detects and follows the player.
- Handles attack range and attack cooldowns.
- Calculates critical hits.
- Handles obstacle avoidance.
- Applies weapon knockback.
- Updates the health bar.
- Handles enemy death and item drops.
- Registers boss defeats with StructureSpawner.

### 5. PassiveMobBehavior.cs

Controls passive mob behavior.

**Responsibilities:**
- Manages passive mob movement.
- Handles wandering behavior.
- Responds to attacks by fleeing.
- Handles knockback.
- Manages passive mob drops and death behavior.

## Spawning System

The spawning system operates globally rather than independently per chunk.

**Features:**
- Hostile and passive mobs have separate spawning routines.
- Population limits apply to the entire loaded area.
- Spawn positions are selected from all currently registered loaded chunks.
- Spawn positions follow the current WorldGenerator chunk size.
- Mobs cannot spawn within the configured protection radius around the player.
- Spawn intervals and population limits are configurable through the Inspector.

## Chunk-Based Despawning

The system checks whether each registered mob is still inside any currently loaded chunk.

**Despawning behavior:**
- Mobs remain active while inside the loaded chunk area.
- Mobs are despawned when they move outside all loaded chunks.
- Chunk boundaries are calculated using the current WorldGenerator chunk size.
- Existing mobs are not attached to their original chunks.
- Mobs do not snap back to their original spawn positions when crossing chunk boundaries.

**Important:** Despawning does not call the mob's `Die()` method. It directly unregisters and destroys the mob without triggering death animations, boss defeat logic, or item drops.

Boss mobs are excluded from regular chunk-based despawning.

## Active Mobs Parent

All registered mobs are organized under an automatically created `ActiveMobs` GameObject inside the world's hierarchy.

When a mob is registered, it is reparented under `ActiveMobs` while preserving its world position.

This separates active mobs from the chunks where they originally spawned.

## WorldGenerator Integration

The mob system uses the existing WorldGenerator settings:

- `chunkSize` – Determines the dimensions of each chunk.
- `loadDistance` – Determines how many chunks are loaded around the player's current chunk.
- `player` – Provides the player's position for spawn protection and chunk calculations.

The spawning and despawning systems use the currently registered loaded chunks rather than maintaining their own chunk-loading system.

## Configuration

The following settings can be adjusted through the MobSpawnerController Inspector:

- Maximum hostile mob population.
- Maximum passive mob population.
- Hostile spawn interval.
- Passive spawn interval.
- Hostile spawn protection radius.
- Passive spawn protection radius.
- Hostile spawn chances and day/time requirements.
- Passive mob spawn chances.

The global population limits should be balanced according to the desired world population and performance.

## System Architecture

```text
WorldGenerator
    |
    |-- Loads and unloads chunks
    |
    v
ChunkMobSpawner
    |
    |-- Registers loaded chunks
    |-- Unregisters unloaded chunks
    |
    v
MobSpawnerController
    |
    |-- Global Hostile Spawning
    |-- Global Passive Spawning
    |-- Global Population Limits
    |-- Mob Registration
    |-- Chunk-Based Despawning
    |
    v
ActiveMobs
    |
    |-- HostileMobBehavior
    |-- PassiveMobBehavior
    |-- MobIdentity
```

## Important Notes

- Mob spawning is centralized through MobSpawnerController.
- Population limits are global, not per chunk.
- Spawn positions cover the entire loaded chunk area, excluding the player's protection radius.
- Mobs are independent of their original chunks after spawning.
- Chunk unloading does not automatically destroy all mobs.
- Despawning does not trigger normal death or item-drop behavior.
- Boss mobs are excluded from regular spawning and despawning.
- WorldGenerator remains responsible for loading and unloading chunks.