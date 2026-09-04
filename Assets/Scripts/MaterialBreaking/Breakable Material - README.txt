# Breakable Material System — README

This system allows the player to **break/mining materials using tools or bare hands**, depending on the configured `ToolBreakPreset`.

For example:

* Axe → can break Trees
* Pickaxe → can break Stone
* Bare hands → can break specific materials if configured
* Tree → can drop Wood, Apples, Seeds, etc.
* Stone → can drop Stone, Coal, etc.
* Different tools can deal different amounts of damage.
* Different tools can have different breaking speeds.
* Different materials can have different HP.
* Multiple items can be dropped from one material.
* Drop amounts are randomized.
* Materials are detected through a **player break hitbox**.
* Clicking the hotbar does not trigger breaking.

---

# 1. Required Scripts

The system currently uses these four scripts:

### `BreakableMaterial.cs`

Attached to objects that can be broken.

Handles:

* Material type
* Material HP
* Taking damage
* Preventing HP from going below 0
* Detecting when the material reaches 0 HP
* Destroying the material when it breaks

---

### `BreakHitbox.cs`

Attached to the player's break hitbox.

Handles:

* Detecting breakable materials inside the player's hitbox
* Setting the current break target
* Removing the target when it leaves the hitbox

The hitbox uses:

```csharp
GetComponentInParent<BreakableMaterial>()
```

This means the `BreakableMaterial` component can be on the parent of the collider.

---

### `BreakSystem.cs`

Attached to the **Player**.

Handles:

* Detecting the player's equipped item
* Supporting bare hands
* Checking whether the player has a breakable target
* Checking the breakable layer
* Checking break distance
* Finding the correct `ToolBreakPreset`
* Checking whether the equipped tool can break the material
* Applying damage
* Applying the tool's breaking cooldown
* Creating dropped items
* Preventing breaking when clicking the hotbar

The player clicks the left mouse button to attempt a break.

However, the system does **not** use the mouse position to directly find the material.

Instead, `BreakHitbox` determines what breakable object is currently aligned with the player.

---

### `ToolBreakPreset.cs`

Creates a ScriptableObject preset that defines how a tool or bare hands behave.

Handles:

* Whether a tool is required
* Required tool
* Materials that can be broken
* Multiple drop items
* Minimum/maximum drop amounts
* Damage dealt
* Breaking cooldown

---

### `DropItemData.cs`

Defines an individual item drop inside a `ToolBreakPreset`.

Handles:

* Item to drop
* Minimum amount
* Maximum amount

---

# 2. Required Existing Components

Before setting up this system, make sure your project already has:

* `Item.cs`
* `PlayerHeldItem.cs`
* Your Hotbar/Inventory system
* Item prefabs
* Unity Input System

`BreakSystem` looks for `PlayerHeldItem` on the same GameObject.

Your player should look something like:

```text
Player
├── Rigidbody2D
├── PlayerMovement
├── PlayerHeldItem
├── BreakSystem
├── BreakHitbox
└── ...
```

The `BreakHitbox` should have a `Collider2D` configured as a trigger.

Example:

```text
Player
├── BreakSystem
└── BreakHitbox
    └── Collider2D
```

---

# 3. Setting Up the Break Hitbox

The system uses a **break hitbox** instead of checking the mouse position for breakable objects.

Create a child GameObject under the Player:

```text
Player
└── BreakHitbox
    └── Collider2D
```

Add:

```text
BreakHitbox.cs
```

to the `BreakHitbox` object.

Then assign the Player's `BreakSystem` to:

```text
BreakHitbox
└── Break System
```

The collider should be configured as:

```text
Is Trigger = true
```

The size and position of the hitbox determine which breakable objects can be targeted.

---

# 4. How the Break Hitbox Works

When a breakable object enters the hitbox:

```text
Breakable Object
      ↓
BreakHitbox detects Collider2D
      ↓
GetComponentInParent<BreakableMaterial>()
      ↓
BreakSystem.SetTarget()
      ↓
Object becomes current break target
```

When the object leaves:

```text
Breakable Object leaves hitbox
      ↓
BreakHitbox detects OnTriggerExit2D
      ↓
BreakSystem.RemoveTarget()
      ↓
Target is cleared
```

The system therefore only attempts to break the object currently detected by the player's break hitbox.

---

# 5. Setting Up Breakable Materials

To make an object breakable:

1. Select the object in the Unity Hierarchy.
2. Add `BreakableMaterial`.
3. Add a `Collider2D`.
4. Set the object to the `Breakable` layer.
5. Configure the material type and health.

Example:

```text
Tree
├── SpriteRenderer
├── Collider2D
└── BreakableMaterial
```

---

# 6. BreakableMaterial Inspector

## Material Type

Set this to the material's identifier.

Example:

```text
Tree
```

Stone:

```text
Stone
```

Iron Ore:

```text
IronOre
```

The value must match the material name configured in the `ToolBreakPreset`.

Example:

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

The comparison is case-sensitive.

---

## Max Health

Determines how much damage the material can take.

Example:

```text
Max Health = 3
```

If the tool deals 1 damage:

```text
Hit 1 → 2 HP
Hit 2 → 1 HP
Hit 3 → 0 HP → BREAK
```

If the tool deals 3 damage:

```text
Hit 1 → 0 HP → BREAK
```

The system prevents health from going below:

```text
0
```

---

# 7. Setting Up the Breakable Layer

`BreakSystem` uses a `LayerMask` to verify that the target is breakable.

Create a layer called:

```text
Breakable
```

Assign it to every object that should be breakable.

For example:

```text
Tree → Breakable
Stone → Breakable
IronOre → Breakable
CoalOre → Breakable
```

Then assign the same layer to:

```text
BreakSystem
└── Breakable Layer
```

Objects that are not on this layer will not be broken.

---

# 8. Collider2D Requirements

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

The collider allows `BreakHitbox` to detect the material.

The collider does not need to be on the same GameObject as `BreakableMaterial`.

For example, this is also supported:

```text
Tree
├── BreakableMaterial
└── TreeCollider
    └── BoxCollider2D
```

because `BreakHitbox` uses:

```csharp
GetComponentInParent<BreakableMaterial>()
```

---

# 9. Creating a Tool Break Preset

`ToolBreakPreset` is a ScriptableObject.

After importing the script:

1. Right-click inside the Project window.
2. Select:

```text
Create
→ Game
→ Tool Break Preset
```

Create one preset for each type of breaking behavior.

Example:

```text
BreakPresets
├── Axe_BreakPreset
├── Pickaxe_BreakPreset
└── BareHands_BreakPreset
```

---

# 10. Tool Requirement

The preset now has:

```text
Requires Tool
```

This determines whether the preset requires an equipped item.

### Tool Required

```text
Requires Tool = true
```

The preset will only work if the player has the correct tool equipped.

For example:

```text
Required Tool = Axe
```

---

### Bare Hands

A preset can also work without a tool:

```text
Requires Tool = false
```

In this case:

```text
Required Tool
```

does not need to be assigned.

The player can have:

```text
Held Item = null
```

and the preset can still be used.

This allows you to create behavior such as:

```text
Bare Hands → Bush
Bare Hands → SmallRock
```

while still preventing bare hands from breaking materials that require tools.

---

# 11. Example Axe Preset

Create:

```text
Axe_BreakPreset
```

Set:

```text
Requires Tool = true
Required Tool = Axe
```

Materials:

```text
Breakable Materials
Size = 1

Element 0 = Tree
```

This means:

```text
Axe → Tree = Valid
Axe → Stone = Invalid
```

---

# 12. Setting Up Multiple Materials

A single preset can support multiple materials.

For example:

```text
Axe_BreakPreset

Breakable Materials
Size = 3

Element 0 = Tree
Element 1 = Bush
Element 2 = WoodenCrate
```

The preset can then break:

```text
Tree
Bush
WoodenCrate
```

but not:

```text
Stone
IronOre
CoalOre
```

---

# 13. Setting Up Multiple Drops

A `ToolBreakPreset` can contain multiple `DropItemData` entries.

Example:

```text
Drops
Size = 3
```

Element 0:

```text
Item = Wood
Min Amount = 2
Max Amount = 5
```

Element 1:

```text
Item = Apple
Min Amount = 1
Max Amount = 2
```

Element 2:

```text
Item = Seed
Min Amount = 1
Max Amount = 3
```

When the material breaks, every configured drop is processed independently.

Example:

```text
Tree breaks
    ↓
Wood → random amount
    ↓
Apple → random amount
    ↓
Seed → random amount
```

---

# 14. Drop Amounts

Each `DropItemData` contains:

```text
Min Amount
Max Amount
```

The system generates a random amount between the two values.

For example:

```text
Min Amount = 2
Max Amount = 5
```

can produce:

```text
2
3
4
5
```

The code uses:

```csharp
Random.Range(
    minAmount,
    maxAmount + 1
);
```

The current implementation also ensures the minimum amount is at least `1`.

Therefore, a configured value of:

```text
Min Amount = 0
```

will be treated as:

```text
Min Amount = 1
```

This means the current system **does not support zero-quantity drops**.

---

# 15. Drop Item Setup

Each drop must reference an `Item` prefab.

The `Item` field should reference the object containing the `Item` component.

For example:

```text
Drops

Element 0
├── Item → Wood
├── Min Amount → 2
└── Max Amount → 5
```

The item is then cloned using:

```csharp
CloneItem(amount)
```

The cloned item is placed at the position of the broken material.

---

# 16. Damage

Set the damage dealt by the preset:

```text
Damage = 1
```

Example:

```text
Wooden Axe
Damage = 1
```

or:

```text
Iron Axe
Damage = 2
```

Damage is applied every time the player successfully hits the material.

The preset requires a minimum damage of:

```text
1
```

---

# 17. Breaking Cooldown

The breaking cooldown is controlled by the `ToolBreakPreset`.

Example:

```text
Axe_BreakPreset

Damage = 1
Break Cooldown = 0.4
```

The cooldown is measured in seconds.

```text
Break Cooldown = 0.4
```

means the player must wait approximately 0.4 seconds before another successful hit can occur.

Example:

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

Lower cooldown means faster breaking.

Higher cooldown means slower breaking.

---

# 18. Setting Up BreakSystem

Add:

```text
BreakSystem.cs
```

to the Player.

Example:

```text
Player
├── PlayerHeldItem
├── BreakSystem
├── BreakHitbox
└── ...
```

Configure the Inspector.

---

## Player

Drag the player's Transform into:

```text
Player
```

This is used to calculate the distance between the player and the current break target.

---

## Break Range

Example:

```text
Break Range = 2.5
```

The player can only break the target if it is within this distance.

If the target is too far away:

```text
Material is too far away.
```

The break attempt is cancelled.

---

## Tool Presets

Add all of your `ToolBreakPreset` assets.

Example:

```text
Tool Presets
Size = 2

Element 0 → Axe_BreakPreset
Element 1 → Pickaxe_BreakPreset
```

You can add more:

```text
Element 2 → Shovel_BreakPreset
Element 3 → Hammer_BreakPreset
Element 4 → BareHands_BreakPreset
```

---

## Breakable Layer

Set:

```text
Breakable Layer = Breakable
```

This must match the layer assigned to your breakable objects.

---

## Hotbar Panel

Assign the player's hotbar:

```text
Hotbar Panel → HotbarPanel
```

This is used to prevent breaking when the player clicks UI elements inside the hotbar.

If the player clicks a hotbar slot:

```text
Left Click
    ↓
Is pointer over hotbar?
    ↓
YES
    ↓
Do not attempt to break
```

---

# 19. How the Player Breaks Objects

The current system works like this:

```text
Player moves near material
        ↓
BreakHitbox detects material
        ↓
BreakSystem stores target
        ↓
Player left-clicks
        ↓
Check if click is over hotbar
        ↓
NO
        ↓
Get equipped Item
        ↓
Check cooldown
        ↓
Check current break target
        ↓
Check Breakable layer
        ↓
Get BreakableMaterial
        ↓
Check break distance
        ↓
Find matching ToolBreakPreset
        ↓
Apply damage
        ↓
Start cooldown
        ↓
Material reaches 0 HP?
        ↓
YES
        ↓
Destroy material
        ↓
Clone configured drops
```

---

# 20. Equipped Item Detection

`BreakSystem` gets the currently held item through:

```csharp
playerHeldItem.GetHeldItem();
```

There are two possible states.

### Tool Equipped

Example:

```text
Held Item = Axe
```

The system searches for a preset that matches:

```text
Axe ID
+
Material Type
```

---

### Bare Hands

If:

```text
Held Item = null
```

the system treats the player as using bare hands.

The system can then find a preset where:

```text
Requires Tool = false
```

and the material matches.

If no such preset exists:

```text
Bare hands cannot break Tree
```

---

# 21. How Preset Matching Works

`FindPreset()` checks the configured presets.

First, the material must match:

```text
preset.breakableMaterials
```

Then one of two conditions must be satisfied.

### Preset Does Not Require a Tool

```text
Requires Tool = false
```

The preset can be used with bare hands.

---

### Preset Requires a Tool

```text
Requires Tool = true
```

The equipped item must have the same ID as:

```text
Required Tool
```

The system compares:

```csharp
preset.requiredTool.ID == tool.ID
```

This means the actual Item ID is used to identify the tool.

---

# 22. Example Pickaxe Preset

Create:

```text
Pickaxe_BreakPreset
```

Set:

```text
Requires Tool = true
Required Tool = Pickaxe
```

Materials:

```text
Breakable Materials

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
Min Amount = 1
Max Amount = 2
```

Damage:

```text
Damage = 1
```

Cooldown:

```text
Break Cooldown = 0.5
```

The system then works like:

```text
Equip Pickaxe
      ↓
Move close to Stone
      ↓
BreakHitbox detects Stone
      ↓
Left Click
      ↓
Find Pickaxe preset
      ↓
Deal 1 damage
      ↓
Wait 0.5 seconds
      ↓
Hit again
      ↓
Stone reaches 0 HP
      ↓
Stone breaks
      ↓
Drop Stone
      ↓
Drop Coal
```

---

# 23. Example Bare Hands Preset

Create:

```text
BareHands_BreakPreset
```

Set:

```text
Requires Tool = false
```

Required Tool:

```text
None
```

Materials:

```text
Breakable Materials

Size = 1
Element 0 = Bush
```

Drops:

```text
Size = 1

Element 0
Item = Fiber
Min Amount = 1
Max Amount = 3
```

Damage:

```text
Damage = 1
```

Cooldown:

```text
Break Cooldown = 0.5
```

Now:

```text
No item equipped
      ↓
Move near Bush
      ↓
BreakHitbox detects Bush
      ↓
Left Click
      ↓
BareHands preset found
      ↓
Bush takes damage
```

---

# 24. Complete Example Setup

A simple project setup might look like:

```text
Assets
├── Scripts
│   ├── BreakableMaterial.cs
│   ├── BreakHitbox.cs
│   ├── BreakSystem.cs
│   ├── ToolBreakPreset.cs
│   ├── DropItemData.cs
│   ├── Item.cs
│   ├── PlayerHeldItem.cs
│   └── ...
│
├── Items
│   ├── Axe
│   ├── Pickaxe
│   ├── Wood
│   ├── Stone
│   ├── Coal
│   └── ...
│
├── BreakPresets
│   ├── Axe_BreakPreset
│   ├── Pickaxe_BreakPreset
│   └── BareHands_BreakPreset
│
└── Prefabs
    ├── Tree
    └── Stone
```

---

# 25. Tree Setup

The Tree GameObject:

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

The tree's collider must be detectable by the player's `BreakHitbox`.

---

# 26. Stone Setup

The Stone GameObject:

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

# 27. Example Gameplay

Suppose the player has an Axe equipped.

The Axe preset contains:

```text
Requires Tool = true
Required Tool = Axe
Breakable Materials = Tree

Damage = 1
Break Cooldown = 0.4
```

The Tree contains:

```text
Material Type = Tree
Max Health = 3
```

The player moves close enough for the `BreakHitbox` to detect it.

First click:

```text
Tree HP: 2/3
```

Second click after cooldown:

```text
Tree HP: 1/3
```

Third click:

```text
Tree HP: 0/3
Tree BROKE!
```

The tree is destroyed and its configured drops are cloned at the tree's position.

---

# 28. Multiple Materials Per Tool

A single tool preset can support multiple materials.

Example:

```text
Pickaxe_BreakPreset

Breakable Materials

Element 0 = Stone
Element 1 = IronOre
Element 2 = CoalOre
```

Then:

```text
Pickaxe → Stone = Valid
Pickaxe → IronOre = Valid
Pickaxe → CoalOre = Valid
Pickaxe → Tree = Invalid
```

You do not need a separate `BreakSystem` for every material.

---

# 29. Multiple Tools

Create separate presets for different tools.

Example:

```text
Axe_BreakPreset
Pickaxe_BreakPreset
Shovel_BreakPreset
```

Each preset can define:

```text
Required Tool
Breakable Materials
Drops
Damage
Break Cooldown
```

`BreakSystem` searches the presets and finds the one matching:

```text
Equipped Tool ID
+
Material Type
```

This allows new tools and materials to be added without hardcoding every combination inside `BreakSystem.cs`.

---

# 30. Common Problems

## "No breakable object aligned with player."

This means `BreakSystem` currently has no target from `BreakHitbox`.

Check:

* `BreakHitbox` is attached to the player.
* `BreakHitbox` has a `Collider2D`.
* The collider is set to `Is Trigger`.
* The breakable object has a `Collider2D`.
* The breakable object has `BreakableMaterial`.
* The breakable object's collider can enter the hitbox.
* The `BreakHitbox` has the correct `BreakSystem` assigned.

---

## "Target does not have BreakableMaterial."

The hitbox detected a collider, but:

```text
GetComponentInParent<BreakableMaterial>()
```

returned `null`.

Make sure the collider belongs to an object whose parent hierarchy contains:

```text
BreakableMaterial
```

---

## "Material is too far away."

The target was detected by the hitbox, but the distance check failed.

Increase:

```text
Break Range
```

or move the player closer.

---

## "Axe cannot break Tree."

Check:

```text
Axe_BreakPreset
├── Requires Tool = true
├── Required Tool = Axe
└── Breakable Materials
    └── Tree
```

Also make sure the Axe Item ID matches the required Axe Item ID.

---

## "Bare hands cannot break Tree."

Check whether a preset exists with:

```text
Requires Tool = false
```

and:

```text
Breakable Materials
    Tree
```

If the preset requires a tool, bare hands will not work.

---

## "A Drop Item is missing."

Open the relevant `ToolBreakPreset`.

Check:

```text
Drops
```

For every element, make sure:

```text
Item
```

is assigned.

---

## "CloneItem returned null!"

Check your:

```csharp
Item.CloneItem(int newQuantity)
```

implementation.

Also make sure every drop references a valid Item prefab.

---

## "The material gives negative HP."

`BreakableMaterial` prevents this using:

```csharp
currentHealth = Mathf.Max(currentHealth, 0);
```

Health should therefore never go below:

```text
0
```

If negative HP still appears, check whether another script is modifying the material's health.

---

## "The tool breaks too quickly/slowly."

Open the relevant `ToolBreakPreset`.

Adjust:

```text
Break Cooldown
```

Remember:

```text
0.2 → Fast
0.4 → Moderate
0.6 → Slow
1.0 → Very Slow
```

---

## "Clicking the hotbar also tries to break."

Make sure:

```text
BreakSystem
└── Hotbar Panel
```

has the correct hotbar GameObject assigned.

`BreakSystem` checks the UI using Unity's `EventSystem`.

If the pointer is over a child of the assigned hotbar panel, the break attempt is cancelled.

---

# 31. Recommended Naming

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
BareHands_BreakPreset
```

### Drops

```text
Wood
Stone
IronOre
Coal
Apple
Seed
Fiber
```

Keep the material names consistent.

For example:

```text
BreakableMaterial:
materialType = "Stone"
```

must match:

```text
ToolBreakPreset:
breakableMaterials[0] = "Stone"
```

---

# 32. Quick Setup Checklist

## Player

* [ ] `PlayerHeldItem` is attached.
* [ ] `BreakSystem` is attached.
* [ ] `Player` Transform is assigned.
* [ ] `Break Range` is reasonable.
* [ ] Tool presets are assigned.
* [ ] `Breakable Layer` is assigned.
* [ ] `Hotbar Panel` is assigned.
* [ ] `BreakHitbox` exists.
* [ ] `BreakHitbox` has a `Collider2D`.
* [ ] Break hitbox collider is set to `Is Trigger`.
* [ ] `BreakHitbox.breakSystem` references the player's `BreakSystem`.

## Breakable Object

* [ ] `Collider2D` is attached.
* [ ] `BreakableMaterial` is attached.
* [ ] `Material Type` is set.
* [ ] `Max Health` is set.
* [ ] Object is on the `Breakable` layer.

## Tool Break Preset

* [ ] `Requires Tool` is configured.
* [ ] `Required Tool` is assigned if a tool is required.
* [ ] `Breakable Materials` are listed.
* [ ] `Drops` are configured.
* [ ] Every Drop Item is assigned.
* [ ] Minimum drop amounts are set.
* [ ] Maximum drop amounts are set.
* [ ] Damage is set.
* [ ] Break Cooldown is set.

## Drop Items

* [ ] Drop prefab has an `Item` component.
* [ ] Item prefab has its sprite.
* [ ] Item prefab has `QtyText`.
* [ ] `CloneItem()` works correctly.

---

# 33. Basic Setup Example

The simplest tool-based configuration:

```text
PLAYER
│
├── PlayerHeldItem
│
├── BreakSystem
│      │
│      ├── Player → Player Transform
│      ├── Break Range → 2.5
│      ├── Tool Presets
│      │      └── Axe_BreakPreset
│      ├── Breakable Layer → Breakable
│      └── Hotbar Panel → HotbarPanel
│
└── BreakHitbox
       ├── Break System → Player's BreakSystem
       └── Collider2D
              └── Is Trigger → True


AXE PRESET
│
├── Requires Tool → True
├── Required Tool → Axe
├── Breakable Materials
│      └── Tree
│
├── Drops
│      └── Element 0
│             ├── Item → Wood
│             ├── Min Amount → 2
│             └── Max Amount → 5
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
Move near Tree
    ↓
BreakHitbox detects Tree
    ↓
Left Click
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

# 34. Adding New Materials Later

To add a new material, you generally do not need to modify the breaking scripts.

For example, to add Iron Ore:

1. Create an Iron Ore prefab.
2. Add a `Collider2D`.
3. Add `BreakableMaterial`.
4. Set:

```text
Material Type = IronOre
Max Health = 10
```

5. Put it on the `Breakable` layer.
6. Make sure the collider can be detected by the player's `BreakHitbox`.
7. Add `IronOre` to the appropriate `ToolBreakPreset`.
8. Create/configure the Iron Ore Item.
9. Add it to the preset's `Drops` array.

---

# 35. Adding New Tools Later

To add a new tool:

1. Create the tool Item.
2. Create a new `ToolBreakPreset`.
3. Set:

```text
Requires Tool = true
```

4. Assign the tool to:

```text
Required Tool
```

5. Add the materials it can break.
6. Configure the `Drops` array.
7. Set damage.
8. Set the breaking cooldown.
9. Add the preset to the player's `BreakSystem`.

No changes to `BreakSystem.cs` are normally required.

---

# 36. Adding Bare-Hand Breaking Later

To allow the player to break something without a tool:

1. Create a `ToolBreakPreset`.
2. Set:

```text
Requires Tool = false
```

3. Leave:

```text
Required Tool = None
```

4. Add the desired materials.
5. Configure drops.
6. Set damage.
7. Set cooldown.
8. Add the preset to `BreakSystem`.

Example:

```text
BareHands_BreakPreset

Requires Tool = false

Breakable Materials
    Bush

Drops
    Fiber → 1–3

Damage = 1
Break Cooldown = 0.5
```

---

# 37. Current System Architecture

The current breaking system is separated into specialized components:

```text
                    PLAYER
                       │
          ┌────────────┴────────────┐
          │                         │
   PlayerHeldItem              BreakSystem
          │                         │
          │                 Finds preset
          │                 Applies damage
          │                 Handles drops
          │                         │
          │                    BreakHitbox
          │                         │
          │                  Detects target
          │                         │
          └────────────┬────────────┘
                       │
                BreakableMaterial
                       │
                 Stores HP
                 Takes damage
                 Breaks object
```

The main advantage of this structure is that each script has a specific responsibility:

```text
BreakHitbox
    → Finds what the player is targeting

BreakSystem
    → Decides whether/how the target can be broken

ToolBreakPreset
    → Defines tool/bare-hand behavior

BreakableMaterial
    → Stores material health and handles breaking

DropItemData
    → Defines individual item drops
```

This makes the system easier to expand as more tools, materials, and drop types are added.
