# Container System

## Overview

The Container System allows players to interact with different storage objects in the world. Containers use a shared storage architecture, allowing multiple container variants with different storage capacities.

Container contents are saved through the existing building persistence system, allowing stored items to remain available after saving and loading the world.

## Features

- Multiple containers with configurable storage capacities.
- Dynamically generated storage slots.
- Item transfer between the player inventory and containers.
- Item stacking and swapping.
- Item quantity and durability persistence.
- Automatic saving of container contents.
- Integration with the existing building persistence system.
- Container interaction through the E key.
- Container-to-inventory transition through the B key.
- Automatic hotbar resizing and horizontal centering while a container is open.
- Restoration of the hotbar's original position and scale when closing a container.

## Architecture

### 1. ContainerBehavior

Responsible for managing the storage data of an individual container.

Main responsibilities:
- Initialize container storage.
- Restore saved container contents.
- Load saved items into UI slots.
- Synchronize UI slots with saved data.
- Save container capacity and contents.
- Restore item quantities and durability.

Each container has its own configurable capacity.

### 2. ContainerController

Manages the player's interaction with containers and the container UI.

Main responsibilities:
- Detect nearby containers using their interaction hitboxes.
- Handle container interaction.
- Dynamically generate storage slots.
- Display container contents.
- Save contents when closing the container.
- Refresh container slots when necessary.
- Manage the hotbar's position and scale.

The controller also provides `CloseForMenu()` so other UI systems can close the container safely.

### 3. ItemDragHandler

Handles item movement between inventory and storage slots.

Main responsibilities:
- Handle item dragging.
- Transfer items between slots.
- Stack compatible items.
- Swap items between occupied slots.
- Validate item movement.
- Integrate with furnace slot restrictions.
- Notify relevant systems when slot contents change.

### 4. Slot

Represents an individual inventory or container slot.

Each slot maintains a reference to its current item through `currentItem`.

### 5. BuildingSaveData

Stores persistent container information alongside the existing building save data.

Container-related fields:

- `hasContainerState`
- `containerItems`
- `containerCapacity`

Container contents use the existing `ItemSlotSaveData` class rather than creating a separate save-data structure.

## Container Interaction

Players interact with a container by entering its interaction hitbox and pressing E.

When opened:
1. The controller identifies the nearby container.
2. Storage slots are generated according to its capacity.
3. Saved container contents are loaded.
4. The container panel becomes visible.
5. The hotbar is resized and horizontally centered.

Pressing E again closes the container and saves its latest contents.

## Menu Integration

### B Key Behavior

When a container is open, pressing B:
1. Saves the current container contents.
2. Closes the container panel.
3. Opens the inventory page (`pages[1]`).

This allows players to transition directly from container storage to the inventory without losing their current storage state.

The existing furnace and crafting menu behaviors remain intact.

## Hotbar Behavior

The hotbar automatically changes its layout when interacting with a container.

When opening a container:
- The hotbar scale changes to the configured container scale.
- Its horizontal position is set to the center (`X = 0`).
- Its original vertical position is preserved.

When closing a container:
- The original hotbar scale is restored.
- The original anchored position is restored.

The `MenuController` avoids overriding the container's hotbar positioning while the container is open.

## World Persistence

Container storage is integrated into the existing building persistence system.

Each container uses its associated `BuildingObject` to retrieve its `BuildingSaveData`.

When initializing:
- Existing container data is restored.
- New containers receive initialized storage data.
- Saved capacity is restored when available.

When synchronizing:
- Existing saved container items are cleared.
- Occupied slots are converted into `ItemSlotSaveData`.
- Item IDs, quantities, durability, and slot indexes are recorded.

This allows different containers to maintain independent storage contents.

## Unity Setup

### Container Prefab

Each container prefab should have:

- `ContainerBehavior`
- `BuildingObject`
- `BoxCollider2D` for player interaction

The `BoxCollider2D` is used as the interaction hitbox.

Configure the container capacity through the `ContainerBehavior` Inspector.

### Container UI

The Container UI requires:

- Container Panel
- Slot Parent
- Slot Prefab

The `ContainerController` generates the required number of slots dynamically.

### ContainerController References

Assign the following references in the Inspector:

| Field | Reference |
|---|---|
| Player | Player Transform |
| Container Panel | Container UI Panel |
| Slot Parent | Container Slot Parent |
| Slot Prefab | Inventory Slot Prefab |
| Hotbar Rect | Hotbar RectTransform |

### MenuController References

Assign the `ContainerController` reference in the MenuController Inspector.

This allows the menu system to detect an open container and handle the B-key transition correctly.

## Testing Results

The following functionality has been tested:

- Multiple containers with different capacities.
- A 30-slot container.
- A 60-slot container.
- Opening and closing containers.
- Loading saved container contents.
- Saving container contents.
- Item transfer between inventory and containers.
- Item stacking and swapping.
- Persistence after saving and loading the world.
- B-key transition from container to inventory.
- Hotbar horizontal centering when opening a container.
- Hotbar position and scale restoration when closing.
- Existing furnace and crafting menu behavior.

## Design Notes

- The system uses the name `Container` instead of `Chest` to support future storage variants.
- Container storage is tied to the existing building persistence architecture.
- The existing `ItemSlotSaveData` class is reused.
- Container capacity is configurable per prefab.
- Container slots are generated dynamically instead of requiring manually configured slot objects.
- Container storage and UI management are separated into different controllers.

## Future Improvements

Possible future extensions include:

- Additional container variants.
- Different storage capacities.
- Specialized storage restrictions.
- Container-specific UI layouts.
- Additional storage interactions.