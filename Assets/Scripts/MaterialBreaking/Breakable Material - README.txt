# Breakable Material System — README

This system allows the player to **break/mining materials using specific tools**.

For example:

* Axe → can break Trees
* Pickaxe → can break Stone
* Tree → can drop Wood, Apples, Seeds, etc.
* Stone → can drop Stone, Coal, etc.
* Different tools can deal different amounts of damage.
* Different tools can have different breaking speeds.
* Different materials can have different HP.
* Multiple items can be dropped from one material.
* Drop amounts can be randomized.

---

# 1. Required Scripts

The system uses these three scripts:

### `BreakableMaterial.cs`

Attached to objects that can be broken.

Handles:

* Material type
* Material HP
* Taking damage
* Preventing HP from going below 0
* Destroying the object when HP reaches 0

---

### `BreakSystem.cs`

Attached to the **Player**.

Handles:

* Detecting the player's equipped item
* Detecting what the player clicks
* Checking break distance
* Checking whether the equipped tool can break the material
* Applying damage
* Applying the tool's breaking cooldown
* Creating dropped items

---

### `ToolBreakPreset.cs`

Creates a **ScriptableObject preset** that defines how a specific tool behaves.

Handles:

* Required tool
* Materials the tool can break
* Multiple drop items
* Minimum/maximum drop amount for each item
* Damage dealt
* Breaking cooldown

---

# 2. Required Existing Components

Before setting up this system, make sure your project already has:

* `Item.cs`
* `PlayerHeldItem.cs`
* Your Hotbar/Inventory system
* Item prefabs
* Unity Input System

`BreakSystem` specifically looks for `PlayerHeldItem` on the same GameObject as `BreakSystem`.

Your player should therefore look something like:

```text
Player
├── Rigidbody2D
├── PlayerMovement
├── PlayerHeldItem
├── BreakSystem
└── ...
```

---

# 3. Setting Up Breakable Materials

To make an object breakable:

1. Select the object in the Unity Hierarchy.
2. Add the `BreakableMaterial` component.
3. Set its values in the Inspector.

Example:

```text
Tree
├── SpriteRenderer
├── Collider2D
└── BreakableMaterial
```

## BreakableMaterial Inspector

### Material Type

Set this to the name of the material.

Example:

```text
Tree
```

For a stone:

```text
Stone
```

For another material:

```text
IronOre
```

The name must match the material name used in the `ToolBreakPreset`.

For example:

```text
BreakableMaterial
Material Type = Stone
```

must match:

```text
ToolBreakPreset
Breakable Materials
Element 0 = Stone
```

The spelling and capitalization should match.

---

### Max Health

Determines how much HP the material has.

Example:

```text
Max Health = 3
```

If an axe deals 1 damage:

```text
Hit 1 → 2 HP
Hit 2 → 1 HP
Hit 3 → 0 HP → BREAK
```

If a stronger tool deals 3 damage:

```text
Hit 1 → 0 HP → BREAK
```

HP will never go below 0.

---

# 4. Setting Up the Breakable Layer

`BreakSystem` uses a LayerMask to determine which objects can be detected.

Create a layer called:

```text
Breakable
```

Then assign that layer to every object that should be breakable.

For example:

```text
Tree → Breakable
Stone → Breakable
IronOre → Breakable
CoalOre → Breakable
```

Objects without this layer will not be detected by `BreakSystem`.

---

# 5. Important: Collider2D

Every breakable object needs a `Collider2D`.

For example:

```text
Tree
├── SpriteRenderer
├── BoxCollider2D
└── BreakableMaterial
```

or:

```text
Stone
├── SpriteRenderer
├── CircleCollider2D
└── BreakableMaterial
```

The collider allows:

```csharp
Physics2D.OverlapPoint()
```

to detect the object when the player clicks it.

Without a collider, the system will report:

```text
Nothing breakable clicked.
```

---

# 6. Creating a Tool Break Preset

`ToolBreakPreset` is a ScriptableObject.

After importing the script:

1. Right-click inside the Project window.
2. Select:

```text
Create
→ Game
→ Tool Break Preset
```

You can create one preset for each tool.

For example:

```text
ToolBreakPresets
├── Axe_BreakPreset
├── Pickaxe_BreakPreset
└── ...
```

Each preset controls the behavior of one tool.

---

# 7. Example: Axe Preset

Create:

```text
Axe_BreakPreset
```

Set the Inspector values like this.

## Required Tool

Drag your **Axe Item** into:

```text
Required Tool
```

This must be the same `Item` used by the player's inventory/hotbar.

---

## Materials This Tool Can Break

Set:

```text
Size = 1
Element 0 = Tree
```

This means:

```text
Axe → Tree (valid)
```

but:

```text
Axe → Stone (invalid)
```

A single preset can also contain multiple materials:

```text
Size = 3

Element 0 = Tree
Element 1 = Bush
Element 2 = WoodenCrate
```

---

# 8. Setting Up Multiple Drops

The `ToolBreakPreset` supports **multiple drop items**.

Instead of having one Drop Item, the preset now has:

```text
Drops
```

For example:

```text
Drops
Size = 3
```

### Element 0

```text
Item = Wood
Min Amount = 2
Max Amount = 5
```

### Element 1

```text
Item = Apple
Min Amount = 0
Max Amount = 2
```

### Element 2

```text
Item = Seed
Min Amount = 0
Max Amount = 3
```

When the tree breaks, the system processes each drop separately.

For example:

```text
Tree breaks
    ↓
2–5 Wood
    ↓
0–2 Apples
    ↓
0–3 Seeds
```

The amount for each item is randomly generated.

---

# 9. Drop Item Setup

Each drop must reference your **Item prefab**.

Do not assign only the SpriteRenderer or `ItemObject`.

Your existing Item prefab structure is:

```text
Wood
├── Item
├── ItemObject
│   └── SpriteRenderer
└── QtyText
```

The `Item` field in the Drop entry should reference the object containing the `Item` component.

For example:

```text
Drops

Element 0
├── Item → Wood
├── Min Amount → 2
└── Max Amount → 5
```

---

# 10. Example Tree Drops

A tree could have:

```text
Drops
Size = 3
```

### Wood

```text
Item = Wood
Min Amount = 2
Max Amount = 5
```

### Apple

```text
Item = Apple
Min Amount = 0
Max Amount = 2
```

### Seed

```text
Item = Seed
Min Amount = 0
Max Amount = 1
```

A possible result:

```text
Dropped 4x Wood
Dropped 1x Apple
Dropped 0x Seed
```

If an item's generated amount is `0`, no useful quantity of that item is dropped.

---

# 11. Damage

Set the damage dealt by the tool:

```text
Damage = 1
```

For example:

```text
Axe
Damage = 1
```

or:

```text
Iron Axe
Damage = 2
```

The damage is applied every time the player successfully hits the material.

---

# 12. Breaking Cooldown

The breaking cooldown is controlled by the **ToolBreakPreset**.

This means different tools can have different breaking speeds.

Example:

```text
Axe_BreakPreset

Damage = 1
Break Cooldown = 0.4
```

and:

```text
IronAxe_BreakPreset

Damage = 2
Break Cooldown = 0.25
```

The cooldown is measured in seconds.

For example:

```text
Break Cooldown = 0.4
```

means the tool can break/hit again after 0.4 seconds.

---

## Example Tool Speeds

```text
Wooden Axe
Damage = 1
Cooldown = 0.5
```

```text
Stone Axe
Damage = 1
Cooldown = 0.4
```

```text
Iron Axe
Damage = 2
Cooldown = 0.3
```

Lower cooldown = faster breaking.

Higher cooldown = slower breaking.

---

# 13. Example: Pickaxe Preset

Create:

```text
Pickaxe_BreakPreset
```

Set:

```text
Required Tool = Pickaxe
```

Materials:

```text
Size = 1
Element 0 = Stone
```

Drops:

```text
Size = 2
```

Element 0:

```text
Item = Stone
Min Amount = 1
Max Amount = 3
```

Element 1:

```text
Item = Coal
Min Amount = 0
Max Amount = 2
```

Damage:

```text
Damage = 1
```

Breaking speed:

```text
Break Cooldown = 0.5
```

Now the system understands:

```text
Pickaxe
   ↓
Stone
   ↓
Deal 1 damage
   ↓
Wait 0.5 seconds
   ↓
Can hit again
   ↓
Stone reaches 0 HP
   ↓
Drop 1–3 Stone
   ↓
Drop 0–2 Coal
```

---

# 14. Setting Up BreakSystem

Add `BreakSystem` to the **Player**.

Example:

```text
Player
├── PlayerMovement
├── PlayerHeldItem
├── BreakSystem
└── ...
```

Then configure the Inspector.

---

## Player

Drag the player's Transform into:

```text
Player
```

This is used to calculate the distance between the player and the material.

---

## Break Range

Example:

```text
Break Range = 2.5
```

This means the player can only break objects within approximately 2.5 Unity units.

If the object is too far away:

```text
Material is too far away.
```

---

## Tool Presets

Increase the array size and add every `ToolBreakPreset` you created.

Example:

```text
Tool Presets
Size = 2

Element 0 → Axe_BreakPreset
Element 1 → Pickaxe_BreakPreset
```

You can add more later:

```text
Element 2 → Shovel_BreakPreset
Element 3 → Hammer_BreakPreset
Element 4 → SpecialTool_BreakPreset
```

---

## Breakable Layer

Set:

```text
Breakable Layer = Breakable
```

This must match the layer assigned to your breakable objects.

---

# 15. Complete Example Setup

Your Project might look like:

```text
Assets
├── Scripts
│   ├── BreakableMaterial.cs
│   ├── BreakSystem.cs
│   ├── ToolBreakPreset.cs
│   ├── Item.cs
│   ├── PlayerHeldItem.cs
│   └── ...
│
├── Items
│   ├── Axe
│   ├── Pickaxe
│   ├── Wood
│   ├── Stone
│   ├── Apple
│   └── Seed
│
├── BreakPresets
│   ├── Axe_BreakPreset
│   └── Pickaxe_BreakPreset
│
└── Prefabs
    ├── Tree
    └── Stone
```

---

# 16. Tree Setup

The Tree GameObject should have:

```text
Tree
├── SpriteRenderer
├── Collider2D
└── BreakableMaterial
```

Inspector:

```text
BreakableMaterial

Material Type = Tree
Max Health = 3
```

Layer:

```text
Breakable
```

---

# 17. Stone Setup

The Stone GameObject should have:

```text
Stone
├── SpriteRenderer
├── Collider2D
└── BreakableMaterial
```

Inspector:

```text
BreakableMaterial

Material Type = Stone
Max Health = 5
```

Layer:

```text
Breakable
```

---

# 18. How the System Works

When the player presses the left mouse button:

```text
Left Click
    ↓
BreakSystem
    ↓
Get currently held Item
    ↓
Detect object under mouse
    ↓
Is it on the Breakable layer?
    ↓
Does it have BreakableMaterial?
    ↓
Is it within break range?
    ↓
Find matching ToolBreakPreset
    ↓
Can this tool break this material?
    ↓
Check cooldown
    ↓
Apply damage
    ↓
Start tool cooldown
    ↓
Material HP reaches 0?
    ↓
YES
    ↓
Destroy material
    ↓
Create all configured dropped Items
```

The important part is that the **cooldown comes from the preset of the equipped tool**.

---

# 19. Example Gameplay

Suppose:

```text
Player has Axe equipped
```

The player clicks a tree.

The tree has:

```text
Material Type = Tree
Health = 3
```

The Axe preset says:

```text
Required Tool = Axe
Breakable Materials = Tree

Damage = 1
Break Cooldown = 0.4
```

Drops:

```text
Wood → 2–5
Apple → 0–2
Seed → 0–1
```

The first hit:

```text
Tree HP: 2/3
```

The player must wait:

```text
0.4 seconds
```

before the next hit.

Second hit:

```text
Tree HP: 1/3
```

Wait:

```text
0.4 seconds
```

Third hit:

```text
Tree HP: 0/3
Tree BROKE!
```

The system then generates the drops independently.

Example:

```text
Dropped 4x Wood
Dropped 1x Apple
Dropped 0x Seed
```

---

# 20. Multiple Materials Per Tool

A tool can break multiple materials.

For example, an advanced pickaxe preset could have:

```text
Breakable Materials

Size = 3

Element 0 = Stone
Element 1 = IronOre
Element 2 = CoalOre
```

Then:

```text
Pickaxe → Stone (valid)
Pickaxe → IronOre (valid)
Pickaxe → CoalOre (valid)
Pickaxe → Tree (invalid)
```

You do **not** need a separate `BreakSystem` for every material.

---

# 21. Multiple Tools

You can create separate presets for each tool.

Example:

```text
Axe_BreakPreset
Pickaxe_BreakPreset
Shovel_BreakPreset
```

The `BreakSystem` searches through the presets and finds the one matching:

```text
Equipped Tool ID
+
Material Type
```

This means you can expand the system without changing `BreakSystem.cs`.

---

# 22. Common Problems

## "Nothing breakable clicked."

Check:

* The object has a `Collider2D`.
* The object's layer is set to `Breakable`.
* `Breakable Layer` in `BreakSystem` includes the `Breakable` layer.
* The mouse is actually clicking the object's collider.

---

## "Clicked object is not breakable."

The clicked object was detected, but it doesn't have:

```text
BreakableMaterial
```

Add the component to the object.

---

## "Material is too far away."

Increase:

```text
Break Range
```

or move the player closer.

---

## "Axe cannot break Tree."

Check the Axe preset:

```text
Required Tool = Axe
```

and:

```text
Breakable Materials
    Tree
```

Also make sure the Axe `Item.ID` matches the `Item.ID` used by the equipped Axe.

---

## "Drop Item is missing."

Open the relevant `ToolBreakPreset`.

Check the `Drops` array.

For each element, make sure:

```text
Item
```

is assigned.

Make sure you're assigning the actual **Item prefab**, not just the SpriteRenderer.

Your Item prefab should contain:

```text
Item
├── ItemObject
│   └── SpriteRenderer
└── QtyText
```

---

## "CloneItem returned null!"

Check your `Item.CloneItem(int newQuantity)` implementation.

Also make sure every item assigned to a `DropItemData` entry is a valid Item prefab.

---

## The material gives negative HP

`BreakableMaterial` already prevents this:

```csharp
currentHealth = Mathf.Max(currentHealth, 0);
```

So HP should stop at:

```text
0
```

If you're still seeing negative HP, check whether another script is also modifying the material's health.

---

## The tool breaks too quickly/slowly

Open the tool's `ToolBreakPreset`.

Adjust:

```text
Break Cooldown
```

Remember:

```text
Lower number = faster
Higher number = slower
```

Example:

```text
0.2 → Fast
0.4 → Moderate
0.6 → Slow
1.0 → Very slow
```

---

# 23. Recommended Naming

To avoid confusion, use consistent names.

### Materials

```text
Tree
Stone
IronOre
CoalOre
```

### Tools

```text
Axe
Pickaxe
Shovel
```

### Presets

```text
Axe_BreakPreset
Pickaxe_BreakPreset
Shovel_BreakPreset
```

### Drops

```text
Wood
Stone
IronOre
Coal
Apple
Seed
```

Keep the `materialType` string and `breakableMaterials` values exactly the same.

For example:

```text
BreakableMaterial:
materialType = "Stone"
```

and:

```text
ToolBreakPreset:
breakableMaterials[0] = "Stone"
```

---

# 24. Quick Setup Checklist

Before testing, make sure:

### Player

* [ ] `PlayerHeldItem` is attached
* [ ] `BreakSystem` is attached
* [ ] Player Transform is assigned
* [ ] Break Range is reasonable
* [ ] Tool presets are assigned
* [ ] Breakable Layer is assigned

### Breakable Object

* [ ] `Collider2D` is attached
* [ ] `BreakableMaterial` is attached
* [ ] Material Type is set
* [ ] Max Health is set
* [ ] Object is on the Breakable layer

### Tool Preset

* [ ] Required Tool is assigned
* [ ] Breakable Materials are listed
* [ ] Drops array is configured
* [ ] Every Drop Item is assigned
* [ ] Minimum drop amounts are set
* [ ] Maximum drop amounts are set
* [ ] Damage is set
* [ ] Break Cooldown is set

### Drop Items

* [ ] Drop prefab has an `Item` component
* [ ] Item prefab has its sprite
* [ ] Item prefab has `QtyText`
* [ ] `CloneItem()` works correctly

---

# 25. Basic Setup Example

The simplest working configuration is:

```text
PLAYER
│
├── PlayerHeldItem
└── BreakSystem
       │
       ├── Player → Player Transform
       ├── Break Range → 2.5
       ├── Tool Presets
       │      └── Axe_BreakPreset
       └── Breakable Layer → Breakable


AXE PRESET
│
├── Required Tool → Axe
├── Breakable Materials → Tree
│
├── Drops
│   └── Element 0
│       ├── Item → Wood
│       ├── Min Amount → 2
│       └── Max Amount → 5
│
├── Damage → 1
└── Break Cooldown → 0.4


TREE
│
├── SpriteRenderer
├── Collider2D
└── BreakableMaterial
       ├── Material Type → Tree
       └── Max Health → 3
```

With this setup:

```text
Equip Axe
    ↓
Click Tree
    ↓
Tree takes 1 damage
    ↓
Wait 0.4 seconds
    ↓
Hit again
    ↓
Repeat until HP = 0
    ↓
Tree is destroyed
    ↓
2–5 Wood is spawned
```

---

# 26. Adding New Materials Later

To add a new material, you generally **don't need to modify the three scripts**.

For example, to add Iron Ore:

1. Create an Iron Ore prefab.
2. Add `Collider2D`.
3. Add `BreakableMaterial`.
4. Set:

```text
Material Type = IronOre
Max Health = 10
```

5. Put it on the `Breakable` layer.
6. Add `IronOre` to the appropriate `ToolBreakPreset`.
7. Create/configure the Iron Ore Item.
8. Add it to the preset's `Drops` array.

That's it.

---

# 27. Adding New Tools Later

To add a new tool:

1. Create the tool Item.
2. Create a new `ToolBreakPreset`.
3. Assign the tool to `Required Tool`.
4. Add the materials it can break.
5. Configure the `Drops` array.
6. Set damage.
7. Set the breaking cooldown.
8. Add the preset to the player's `BreakSystem`.

The main advantage of this system is that **tool behavior is controlled through presets instead of hardcoding every tool/material combination inside `BreakSystem.cs`**.

This makes it easy to add new tools, materials, drops, damage values, and breaking speeds as the game grows.
