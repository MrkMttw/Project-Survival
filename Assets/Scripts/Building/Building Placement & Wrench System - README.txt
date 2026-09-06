# Building Placement & Wrench System

This system allows the player to:

* Place buildings from buildable items.
* See a transparent ghost preview before placing.
* Change the ghost color depending on whether placement is valid.
* Prevent buildings from being placed in blocked areas.
* Snap buildings to a configurable grid.
* Consume one building item when placing.
* Relocate already-placed buildings using a wrench.
* Retrieve buildings using a wrench.
* Automatically return retrieved buildings to the hotbar first, then the inventory.
* Prevent retrieved buildings from being destroyed when both storage systems are full.

---

# Scripts

This system uses three scripts:

| Script                   | Purpose                                                                                                          |
| ------------------------ | ---------------------------------------------------------------------------------------------------------------- |
| `BuildingObject.cs`      | Stores the Item ID of a placed building.                                                                         |
| `PlacementController.cs` | Handles building placement, ghost previews, grid snapping, collision checking, item consumption, and relocation. |
| `WrenchFunction.cs`      | Handles wrench interactions, building relocation, and building retrieval.                                        |

---

# 1. BuildingObject.cs

## Purpose

`BuildingObject` is attached to the world/building prefab.

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

`BuildingObject` may also be located on a child object because `PlacementController` searches the placed building's children for it.

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
* Showing valid and invalid placement colors.
* Placing the building.
* Assigning the building's Item ID.
* Consuming one building item.
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

# Inspector Setup

## Grid

```text
Use Grid
Grid Size
```

### Use Grid

Controls whether buildings snap to the grid.

Example:

```text
Use Grid = true
```

### Grid Size

Controls the size of each grid cell.

Example:

```text
Grid Size = 1
```

A building will snap to the nearest grid position.

---

## Ghost

```text
Ghost Alpha
```

`Ghost Alpha` controls how transparent the building preview is.

For example:

```text
0.5 = 50% alpha
```

The ghost also changes color based on placement validity.

---

## Placement

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

The controller checks the building's `Collider2D` against these layers.

If the building has no `Collider2D`, the placement is considered valid.

---

## Building Parent

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

All newly placed buildings will be instantiated as children of this Transform.

---

## Hotbar

Assign:

```text
Hotbar Controller
```

to the player's `HotbarController`.

This allows `PlacementController` to consume the selected building item when a building is successfully placed.

Example:

```text
PlacementController
└── Hotbar Controller → HotbarController
```

If `HotbarController` is not assigned, the controller falls back to:

```csharp
currentItem.RemoveFromStack(1);
```

---

## Ghost Colors

The controller has two color settings:

```text
Valid Color
Invalid Color
```

### Valid Color

The ghost uses this color when the building can be placed.

Default:

```text
Green
```

### Invalid Color

The ghost uses this color when the building is blocked.

Default:

```text
Red
```

The colors are displayed using the configured `Ghost Alpha`.

Example:

```text
Valid location   → Green transparent ghost
Blocked location → Red transparent ghost
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

# Item Prefab and Building Prefab

The system uses two different prefabs.

## Item Prefab

This is the item stored in the hotbar or inventory.

Example:

```text
Campfire Item
└── Item.cs
```

Its `Item` component contains:

```text
Is Buildable ✓
Building Prefab → Campfire Building
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

# How Normal Placement Works

When the player selects a buildable item:

```text
Hotbar
   ↓
Item
   ↓
PlacementController.StartPlacement()
   ↓
Check Is Buildable
   ↓
Get buildingPrefab
   ↓
Create Ghost
   ↓
Follow Mouse
   ↓
Grid Snap
   ↓
Check Collision
   ↓
Valid / Invalid Color
   ↓
Left Click
   ↓
Place Building
   ↓
Assign BuildingObject.itemID
   ↓
Consume 1 Building Item
```

---

# Creating the Building Ghost

When `StartPlacement()` is called, the controller:

1. Checks that the item exists.
2. Checks that `Is Buildable` is enabled.
3. Checks that `buildingPrefab` is assigned.
4. Destroys any existing ghost.
5. Stores the current item.
6. Stores the building prefab.
7. Instantiates the building prefab as the ghost.
8. Changes its name to:

```text
ItemName_Ghost
```

9. Applies the configured ghost transparency.

Example:

```text
Campfire Item
      ↓
StartPlacement()
      ↓
Campfire_Ghost
```

---

# Placement Validation

The ghost follows the mouse and optionally snaps to the grid.

The controller then checks the ghost's `Collider2D`.

If a collider exists, it uses an overlap check against the configured `Blocking Layers`.

```text
Ghost Collider
      ↓
OverlapBoxAll
      ↓
Check Blocking Layers
      ↓
Blocked?
   /       \
 YES       NO
  ↓         ↓
Red       Green
Ghost     Ghost
```

The ghost itself is ignored when checking for collisions.

If there is no `Collider2D` on the ghost, the controller considers the placement valid.

---

# Placing a Building

When the player left-clicks while the placement location is valid:

```text
Ghost Position
      ↓
Instantiate Building
      ↓
Building Parent
      ↓
Find BuildingObject
      ↓
Assign Item ID
      ↓
Consume 1 Item
```

The Item ID is assigned using:

```csharp
buildingObject.itemID = currentItem.ID;
```

This creates the connection between the placed building and its original item.

---

# Building Item Consumption

Successfully placing a building consumes exactly one building item.

If `HotbarController` is assigned:

```csharp
hotbarController.ConsumeSelectedItem(1);
```

If it is not assigned, the controller falls back to:

```csharp
currentItem.RemoveFromStack(1);
```

Therefore:

```text
Place Building
      ↓
Consume 1 Item
```

The building is only created after the placement location has passed the placement check.

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

Attach:

```text
WrenchFunction.cs
```

to the `ItemFunctionController` GameObject.

`WrenchFunction` is **not attached to the wrench item prefab**.

It acts as a separate controller that manages the wrench's functionality.

---

# Wrench Inspector Setup

On the `ItemFunctionController` GameObject, find:

```text
WrenchFunction
```

Assign:

## Wrench Item ID

Enter the Item ID of the wrench.

Example:

```text
Wrench Item ID = 24
```

The system checks the currently held item's ID against this value.

---

## Instruction UI

Create a UI GameObject containing the wrench instructions.

For example:

```text
Canvas
└── WrenchInstruction
    └── TMP Text
```

Then assign:

```text
Instruction UI → WrenchInstruction
```

The script automatically hides the UI when:

* No item is equipped.
* The equipped item is not the wrench.
* The mouse is not over a building.

---

# Wrench Building Detection

When the wrench is equipped, the controller checks the position of the mouse in the world.

It uses:

```csharp
Physics2D.OverlapPoint()
```

to detect the collider underneath the mouse.

It then searches the collider's parent hierarchy for:

```csharp
BuildingObject
```

Therefore, the building's collider does not necessarily have to be on the same GameObject as `BuildingObject`.

Example:

```text
Campfire
├── Sprite
├── Collider2D
└── BuildingObject
```

or:

```text
Campfire
└── Visual
    └── Collider2D
```

with `BuildingObject` located somewhere in the parent hierarchy.

---

# Wrench Controls

When the wrench is equipped and the cursor is over a building:

```text
[E] Relocate     [F] Retrieve
```

The instruction UI becomes visible.

---

# Relocating a Building

Press:

```text
E
```

while hovering over a building.

The flow is:

```text
Wrench Equipped
      ↓
Hover Building
      ↓
Press E
      ↓
StartRelocation()
      ↓
Existing Building Becomes Ghost
      ↓
Follow Mouse
      ↓
Grid Snap
      ↓
Check Placement
      ↓
Valid / Invalid Color
      ↓
Left Click
      ↓
New Position
```

Unlike normal placement, relocation does **not** create a new building prefab.

The existing building itself is used as the relocation object.

This means:

```csharp
ghostObject = building.gameObject;
```

The building is moved to the new position and then returned to its normal appearance after successful placement.

---

# Relocation Validation

Relocated buildings use the same placement validation system as normal buildings.

While relocating:

```text
FollowMouse()
      ↓
CheckPlacement()
      ↓
Blocking Layers
      ↓
Valid / Invalid
```

The building cannot be confirmed at a blocked location.

The existing building is ignored when checking its own collider.

---

# Retrieving a Building

Press:

```text
F
```

while hovering over a building.

The system first checks that the building has a valid:

```text
BuildingObject.itemID
```

Example:

```text
Campfire
    ↓
BuildingObject
    ↓
itemID = 7
```

The system then uses that ID to find the corresponding item prefab.

---

# Retrieval Flow

The complete retrieval process is:

```text
BuildingObject.itemID
        ↓
ItemDictionary
        ↓
GetItemPrefab()
        ↓
Instantiate Temporary Item
        ↓
Set Quantity = 1
        ↓
Try Hotbar
        ↓
If Full → Try Inventory
        ↓
If Both Full → Cancel
        ↓
If Added Successfully
        ↓
Destroy Building
```

---

# Temporary Retrieval Item

The retrieval system does not directly add the shared ItemDictionary prefab.

Instead, it creates a temporary runtime copy:

```csharp
GameObject retrieveItem =
    Instantiate(itemPrefab);
```

It then gets the `Item` component from that copy.

The quantity is explicitly set to:

```csharp
retrieveItemComponent.quantity = 1;
```

This ensures that retrieving one building gives the player exactly one corresponding item.

After the item has been passed to the hotbar or inventory, the temporary runtime object is destroyed.

---

# Hotbar First, Inventory Second

Retrieved buildings are always returned using this order:

```text
Retrieved Building
       ↓
     Hotbar
       ↓
   If Failed
       ↓
   Inventory
```

The system first attempts:

```csharp
hotbar.AddItem(retrieveItem);
```

If that fails, it attempts:

```csharp
inventory.AddItem(retrieveItem);
```

This means the hotbar always has priority over the inventory.

---

# Full Storage Protection

If both the hotbar and inventory cannot accept the retrieved item:

```text
Hotbar Full
    ↓
Inventory Full
    ↓
Retrieval Cancelled
    ↓
Building Remains
```

The building is **not destroyed**.

The building is only destroyed after the retrieved item has successfully been added to either the hotbar or inventory.

This prevents accidental loss of placed buildings.

---

# Important Prefab Relationships

The complete relationship between the item and world building is:

```text
Item Prefab
    │
    │ buildingPrefab
    ↓
Building Prefab
    │
    │ BuildingObject.itemID
    ↓
Original Item ID
    │
    ↓
ItemDictionary
    │
    ↓
Item Prefab
    │
    ├── Hotbar
    │
    └── Inventory
```

This allows the game to know exactly which item should be returned when a building is retrieved.

---

# Recommended Hierarchy

A basic setup should look like:

```text
GameController
├── PlacementController
├── ItemFunctionController
│   └── WrenchFunction
├── ItemDictionary
└── Buildings

Canvas
└── WrenchInstruction

Player
└── PlayerHeldItem

Hotbar
└── Slots
```

Placed buildings are stored under:

```text
GameController
└── Buildings
    ├── Campfire
    ├── Chest
    ├── Furnace
    └── ...
```

---

# Inspector Checklist

## PlacementController

Make sure these are configured:

```text
Use Grid
Grid Size
Ghost Alpha
Blocking Layers
Building Parent
Hotbar Controller
Valid Color
Invalid Color
```

---

## Buildable Item

Make sure:

```text
Item ✓
Is Buildable ✓
Building Prefab → World Building Prefab
```

---

## Building Prefab

Make sure:

```text
Sprite Renderer
Collider2D
BuildingObject
```

`BuildingObject` must be somewhere in the building's hierarchy.

---

## WrenchFunction

Make sure:

```text
Wrench Item ID → Correct Wrench ID
Instruction UI → Wrench Instruction UI
```

The script automatically searches for:

```text
PlacementController
PlayerHeldItem
ItemDictionary
HotbarController
InventoryController
```

---

# Troubleshooting

## Building Cannot Be Placed

Check:

* The Item has `Is Buildable` enabled.
* `Building Prefab` is assigned.
* `PlacementController` exists.
* `Building Parent` is assigned.
* `Blocking Layers` are configured correctly.
* The building prefab has the appropriate `Collider2D` if collision checking is required.
* `Hotbar Controller` is assigned if you want placement to consume the selected hotbar item through `ConsumeSelectedItem()`.

---

## Building Ghost Is Always Red

Check:

* The building has a `Collider2D`.
* The collider is overlapping a layer included in `Blocking Layers`.
* The building is not unintentionally overlapping another blocking object.
* The `Blocking Layers` mask is configured correctly.

---

## Building Ghost Is Always Green

If the building has no `Collider2D`, the current implementation automatically considers the placement valid.

If collision checking is required, make sure the building has a `Collider2D`.

---

## Wrench Does Nothing

Check:

* `PlayerHeldItem` exists.
* The wrench is actually equipped.
* `Wrench Item ID` matches the wrench's Item ID.
* `PlacementController` exists.
* The placed building has `BuildingObject`.
* The building has a valid `itemID`.
* `ItemDictionary` contains the corresponding item prefab.
* The building has a collider that can be detected by `Physics2D.OverlapPoint()`.

---

## Instruction UI Does Not Appear

Check:

* `Instruction UI` is assigned.
* The UI GameObject exists in the scene.
* The building has a `Collider2D`.
* The mouse is positioned over the building.
* The wrench is currently equipped.
* The equipped wrench's Item ID matches `Wrench Item ID`.

---

## Retrieved Building Does Not Return to Inventory

Check:

* The building has a valid `itemID`.
* `ItemDictionary` contains the corresponding item.
* The hotbar has space.
* The inventory has space.
* `HotbarController.AddItem()` is functioning correctly.
* `InventoryController.instance` exists.

The system tries:

```text
Hotbar → Inventory
```

in that order.

---

## Retrieved Building Disappears Unexpectedly

The current retrieval system is designed to prevent this.

The building is only destroyed after the retrieved item has successfully been added.

If both storage systems are full:

```text
Building remains in the world.
```

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
                  Grid Snap
                       │
                       ↓
                Check Collision
                       │
                 ┌─────┴─────┐
                 ↓           ↓
              Invalid      Valid
                 ↓           ↓
             Red Ghost   Green Ghost
                             │
                             ↓
                         Left Click
                             │
                             ↓
                      Place Building
                             │
                             ↓
                       BuildingObject
                             │
                             ↓
                    Store Item ID
                             │
                             ↓
                     Consume 1 Item
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
                       E           F
                       ↓           ↓
                  Relocate     Retrieve
                       ↓           ↓
               Existing Building  │
                 Becomes Ghost    │
                       ↓           ↓
                 Move + Validate  Item ID
                       ↓           ↓
                   Left Click  ItemDictionary
                       ↓           ↓
                  New Position Temporary Item
                                   │
                                   ↓
                              Quantity = 1
                                   │
                                   ↓
                              Try Hotbar
                                   │
                            If Failed
                                   ↓
                             Try Inventory
                                   │
                        ┌──────────┴──────────┐
                        ↓                     ↓
                     Added                 Failed
                        ↓                     ↓
               Destroy Building       Keep Building
```

---

# Summary

`BuildingObject` identifies what item a placed building originally came from.

`PlacementController` handles:

* Building placement.
* Ghost previews.
* Grid snapping.
* Placement validation.
* Valid/invalid ghost colors.
* Building Item ID assignment.
* Building item consumption.
* Building relocation.

`WrenchFunction` handles:

* Detecting buildings under the mouse.
* Displaying wrench instructions.
* Relocating buildings.
* Retrieving buildings.
* Finding the original item through `ItemDictionary`.
* Creating a temporary runtime item.
* Returning one item to the hotbar first.
* Falling back to the inventory.
* Preventing building destruction when storage is full.

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
Temporary Item Copy
    ↓
Hotbar
    ↓
Inventory
```

This allows buildings to be placed, relocated, and retrieved while keeping them connected to their original inventory items.
