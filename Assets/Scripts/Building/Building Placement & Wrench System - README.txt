# Building Placement & Wrench System

This system allows the player to:

* Place buildings from buildable items.
* See a transparent ghost preview before placing.
* Prevent buildings from being placed in blocked areas.
* Relocate already-placed buildings using a wrench.
* Retrieve buildings using a wrench.
* Automatically return retrieved buildings to the hotbar first, then the inventory.

---

# Scripts

This system uses three scripts:

| Script                   | Purpose                                                                    |
| ------------------------ | -------------------------------------------------------------------------- |
| `BuildingObject.cs`      | Stores the Item ID of a placed building.                                   |
| `PlacementController.cs` | Handles building placement, ghost previews, grid snapping, and relocation. |
| `WrenchFunction.cs`      | Handles relocating and retrieving buildings with the wrench.               |

---

# 1. BuildingObject.cs

## Purpose

`BuildingObject` is attached to the **world/building prefab**.

It stores the Item ID of the item that created the building.

```csharp
using UnityEngine;

public class BuildingObject : MonoBehaviour
{
    public int itemID;
}
```

## Setup

Add `BuildingObject.cs` to your building prefab.

Example:

```text
Campfire Prefab
├── Sprite Renderer
├── Collider2D
└── BuildingObject
```

You do **not** need to manually enter the `itemID` when placing buildings.

`PlacementController` automatically assigns the Item ID when the building is placed.

---

# 2. PlacementController.cs

## Purpose

`PlacementController` handles the building placement system.

It is responsible for:

* Creating the building ghost.
* Making the ghost transparent.
* Following the mouse.
* Snapping the building to the grid.
* Checking if the location is blocked.
* Placing the building.
* Assigning the building's Item ID.
* Relocating existing buildings.

---

## Setup

Create a GameObject for your game controllers.

Example:

```text
GameController
├── PlacementController
├── ItemFunctionController
└── ...
```

Attach:

```text
PlacementController.cs
```

to the `PlacementController` GameObject.

---

## Inspector Setup

### Grid

```text
Use Grid
Grid Size
```

### Ghost

```text
Ghost Alpha
```

`Ghost Alpha` controls how transparent the building preview is.

For example:

```text
0.5 = 50% transparent
```

### Placement

```text
Blocking Layers
```

Select the layers that should prevent buildings from being placed.

For example:

```text
Ground
Buildings
Obstacles
```

### Building Parent

Assign a Transform where placed buildings should be stored.

Recommended hierarchy:

```text
GameController
└── Buildings
```

Then assign:

```text
Building Parent → Buildings
```

---

# Buildable Item Setup

The item that represents a building must have:

```text
Item
```

attached to it.

Enable:

```text
Is Buildable ✓
```

Then assign:

```text
Building Prefab → Your building prefab
```

Example:

```text
Campfire Item
├── Item
│   ├── Is Buildable ✓
│   └── Building Prefab → Campfire
```

The `buildingPrefab` field must contain the **world building prefab**, not the item prefab.

---

# How Placement Works

When the player selects a buildable item:

```text
Hotbar
   ↓
Item
   ↓
PlacementController.StartPlacement()
   ↓
Building Ghost
   ↓
Follow Mouse
   ↓
Check Placement
   ↓
Left Click
   ↓
Building Placed
```

When the building is placed, the controller automatically does:

```csharp
buildingObject.itemID = currentItem.ID;
```

This allows the game to remember which item should be returned when the building is retrieved.

---

# 3. WrenchFunction.cs

## Purpose

`WrenchFunction` allows the player to interact with placed buildings while holding the wrench.

The wrench supports two actions:

| Key | Action            |
| --- | ----------------- |
| `E` | Relocate building |
| `F` | Retrieve building |

The instruction UI appears when the cursor is over a building while the wrench is equipped.

---

# Wrench Setup

Create a GameObject for your item functions.

Example:

```text
GameController
├── PlacementController
├── ItemFunctionController
│   └── WrenchFunction
└── ...
```

Create:

```text
ItemFunctionController
```

as a GameObject.

Then attach:

```text
WrenchFunction.cs
```

to it.

> `WrenchFunction` is **not attached to the wrench item prefab**.

It acts as a separate controller that manages the wrench's functionality.

---

# Wrench Inspector Setup

On the `ItemFunctionController` GameObject, find:

```text
WrenchFunction
```

Assign:

### Wrench Item ID

Enter your wrench ID to:

```text
Wrench Item ID
```

Example:

```text
Wrench Item ID = 24 → Wrench
```

### Instruction UI

Create a UI GameObject containing the instructions.

For example:

```text
Canvas
└── WrenchInstruction
    └── TMP Text
```

Then drag:

```text
WrenchInstruction
```

into:

```text
Instruction UI
```

The script automatically hides this UI when the wrench cannot be used.

---

# Wrench Controls

When the wrench is equipped:

### Hover over a building

The instruction UI appears.

```text
[E] Relocate     [F] Retrieve
```

### Press E

The building enters relocation mode.

The building follows the mouse like a normal building placement ghost.

Click the left mouse button to place it in the new location.

### Press F

The building is removed from the world.

The system attempts to return the building's item:

```text
Hotbar
   ↓
Inventory
```

If both are full, the building is **not destroyed**.

---

# How Building Retrieval Works

Every placed building contains:

```csharp
BuildingObject
```

with an Item ID.

For example:

```text
Campfire
    ↓
BuildingObject
    ↓
itemID = Campfire Item ID
```

When the wrench retrieves the building:

```text
BuildingObject.itemID
        ↓
ItemDictionary
        ↓
Find matching Item prefab
        ↓
Try Hotbar
        ↓
If full → Try Inventory
        ↓
If both full → Cancel retrieval
```

This means the game can determine which item to give back to the player.

---

# Important Prefab Relationships

The system has two different prefabs for a buildable object.

## Item Prefab

This is the object stored in the inventory/hotbar.

Example:

```text
Campfire Item
└── Item.cs
```

Its `Item` component contains:

```text
Is Buildable ✓
Building Prefab → Campfire
```

---

## Building Prefab

This is the actual object placed into the world.

Example:

```text
Campfire
├── Sprite Renderer
├── Collider2D
└── BuildingObject
```

The relationship is:

```text
Campfire Item
      │
      │ buildingPrefab
      ↓
Campfire Building
      │
      │ BuildingObject.itemID
      ↓
Campfire Item ID
```

---

# Recommended Hierarchy

A basic setup should look like:

```text
GameController
├── PlacementController
├── ItemFunctionController
│   └── WrenchFunction
├── ItemDictionary
└── ...

Canvas
└── WrenchInstruction

Player
└── PlayerHeldItem

Hotbar
└── Slots

Buildings
├── Campfire
├── Chest
├── Furnace
└── ...
```

---

# Troubleshooting

## Building cannot be placed

Check:

* The Item has `Is Buildable` enabled.
* `Building Prefab` is assigned.
* `PlacementController` exists.
* `Building Parent` is assigned.
* `Blocking Layers` are configured correctly.
* The building prefab has the appropriate `Collider2D` if collision checking is required.

---

## Wrench does nothing

Check:

* `PlayerHeldItem` exists.
* The wrench is actually equipped.
* `Wrench Item Prefab` is assigned in `WrenchFunction`.
* The placed building has `BuildingObject`.
* The building has a valid `itemID`.
* The `ItemDictionary` contains the corresponding item prefab.

---

## Instruction UI does not appear

Check:

* `Instruction UI` is assigned.
* The UI GameObject is active in the scene.
* The building has a `Collider2D`.
* The mouse is actually positioned over the building.
* The wrench is currently equipped.

---

## Retrieved building does not return to inventory

Check:

* The building has a valid `itemID`.
* `ItemDictionary` contains the corresponding item.
* The hotbar has space.
* The inventory has space.

The system tries the **hotbar first**, then the **inventory**.

---

# Script Flow

The complete system works like this:

```text
                BUILDING ITEM
                     │
                     ↓
             Is Buildable?
                     │
                    YES
                     ↓
          PlacementController
                     │
                     ↓
             Create Ghost
                     │
                     ↓
            Move With Mouse
                     │
                     ↓
             Check Collision
                     │
                     ↓
                Left Click
                     │
                     ↓
             Place Building
                     │
                     ↓
             BuildingObject
             stores Item ID
                     │
                     │
                     ▼
                  WRENCH
                     │
                     ↓
              Hover Building
                     │
                     ↓
             Show Instructions
                 /       \
                /         \
              E             F
              ↓             ↓
          Relocate       Retrieve
              ↓             ↓
        Placement      ItemDictionary
        Controller           ↓
              ↓         Find Item Prefab
          Reposition          ↓
              ↓         Hotbar → Inventory
          Left Click
              ↓
         New Position
```

---

# Summary

`BuildingObject` identifies what item a placed building came from.

`PlacementController` handles placing and relocating buildings.

`WrenchFunction` handles wrench interactions and uses the building's Item ID to retrieve the correct item.

The important relationship is:

```text
Item Prefab
    ↓
buildingPrefab
    ↓
Building Prefab
    ↓
BuildingObject.itemID
    ↓
ItemDictionary
    ↓
Item Prefab
```

This allows buildings to be placed, relocated, and retrieved while keeping them connected to their original inventory items.
