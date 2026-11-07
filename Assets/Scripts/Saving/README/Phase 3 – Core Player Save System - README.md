# Phase 3 – Core Player Save System

## Overview

Phase 3 implements the core player data saving and loading system for Project Survival.

Each world has its own player save file, allowing the game to preserve the player's progress when leaving and re-entering a world.

The system uses JSON serialization and integrates with the existing World Save System.

## Features Implemented

### 1. Player Position
- Saves the player's current world position.
- Restores the player's position when entering the saved world.
- Restores the position before the WorldGenerator initializes its chunks.

### 2. Health System
- Saves the player's current health.
- Restores the saved health when loading a world.
- Ensures restored health stays within the player's maximum health.

### 3. Hunger and Saturation
- Saves the player's current hunger.
- Saves the player's current saturation.
- Restores both values when loading a world.
- Updates the corresponding UI after restoration.

### 4. Inventory System
- Saves the items stored in the player's inventory.
- Preserves each item's:
  - Item ID
  - Quantity
  - Slot index
  - Durability
- Restores items to their original inventory slots.

### 5. Hotbar System
- Saves the items stored in the hotbar.
- Preserves item quantities and durability.
- Saves the currently selected hotbar slot.
- Restores the selected slot and equipped item when loading.

### 6. Exit and Save
- Implements a save function before returning to the Main Menu.
- Prevents returning to the Main Menu if saving fails.
- Stores player data separately for each world.

## Scripts and Components

| Script | Responsibility |
|---|---|
| `PlayerSaveManager.cs` | Handles saving, loading, and restoring player data. |
| `PlayerSaveData.cs` | Stores the serializable player save information. |
| `SaveFileHandler.cs` | Handles JSON file creation, reading, and writing. |
| `SaveBackupHandler.cs` | Creates backups and handles save recovery. |
| `ItemDictionary.cs` | Retrieves item prefabs using their IDs. |
| `HealthController.cs` | Restores the player's saved health. |
| `HungerController.cs` | Restores hunger and saturation values. |
| `HotbarController.cs` | Restores the selected hotbar slot and equipped item. |
| `InventoryController.cs` | Provides access to the inventory slots. |

## Save Data Structure

The player save file contains the following information:

| Data | Description |
|---|---|
| `worldID` | Identifies the world associated with the save. |
| `saveVersion` | Identifies the save data version. |
| `playerPosition` | Stores the player's world position. |
| `currentHealth` | Stores current health. |
| `currentHunger` | Stores current hunger. |
| `currentSaturation` | Stores current saturation. |
| `inventoryItems` | Stores inventory item information. |
| `hotbarItems` | Stores hotbar item information. |
| `selectedHotbarSlot` | Stores the currently selected hotbar slot. |

Each inventory and hotbar item entry contains its item ID, quantity, slot index, and durability.

## File Storage

Player data is stored separately inside each world's directory.

```text
Worlds/
├── World_001/
│   ├── world.json
│   ├── player.json
│   ├── player.json.backup1
│   └── player.json.backup2
│
└── World_002/
    ├── world.json
    └── player.json
```

The backup files are managed by `SaveBackupHandler`.

## Saving Process

When the player exits to the Main Menu:

1. `PlayerSaveManager` collects the player's current data.
2. The player's position, health, hunger, and saturation are recorded.
3. Inventory and hotbar slots are inspected.
4. Item IDs, quantities, durability, and slot positions are collected.
5. The selected hotbar slot is recorded.
6. The collected data is serialized into JSON.
7. The save file is written to the selected world's directory.
8. If saving succeeds, the game returns to the Main Menu.

## Loading Process

When entering an existing world:

1. `PlayerSaveManager.Awake()` identifies the selected world.
2. The corresponding `player.json` file is located.
3. The saved player data is loaded and validated.
4. The player's saved position is restored.
5. `WorldGenerator` initializes using the selected world's seed and the restored player position.
6. After inventory initialization, the saved inventory and hotbar items are restored.
7. Health, hunger, and saturation are restored.
8. The selected hotbar slot and equipped item are restored.

If the primary save file is invalid, the system attempts to recover data from its backups.

## Testing and Verification

The following features were implemented and tested:

- [x] Saving player position.
- [x] Restoring player position.
- [x] Saving and restoring health.
- [x] Saving and restoring hunger and saturation.
- [x] Saving and restoring inventory items.
- [x] Preserving item quantities.
- [x] Preserving item durability.
- [x] Restoring original inventory and hotbar slots.
- [x] Restoring the selected hotbar slot.
- [x] Saving before returning to the Main Menu.
- [x] Loading player data when re-entering a world.

### Known Issue

A world generation inconsistency was identified after implementing player position restoration.

Although the player returns to the correct saved position and the world seed remains unchanged, some natural objects may appear in different positions after re-entering the world.

The suspected cause is the neighboring-chunk spacing validation inside `WorldGenerator.IsPositionValid()`, which may depend on chunk loading order.

This issue is scheduled for investigation and correction separately. The working Player Save System should remain unchanged unless the investigation proves otherwise.

## Current Phase Status

**Phase 3 – Core Player Save System: IMPLEMENTED AND TESTED**

The core player data saving and loading functionality is working.

### Not Yet Implemented

- Periodic autosaving.
- Best-effort saving when the application closes.
- Additional persistence systems for other world objects, if required.

These features are reserved for subsequent development phases.