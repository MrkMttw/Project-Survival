# Breakable Material System

A simple Unity 2D system for breaking objects using tools or bare hands.

The system supports:

* Different tools for different materials
* Bare-hand breaking
* Different damage values
* Different breaking cooldowns
* Different material HP
* Multiple drops per material
* Randomized drop amounts
* Player break hitbox detection
* Hotbar click protection

---

## 1. Scripts

The system uses five scripts.

### `BreakableMaterial.cs`

Attached to objects that can be broken.

Handles:

* Material type
* Health
* Taking damage
* Breaking the object
* Drops

Each breakable object has its **own drop configuration**.

---

### `BreakHitbox.cs`

Attached to the player's break hitbox.

Handles:

* Detecting breakable objects
* Setting the current break target
* Removing the target when it leaves the hitbox

It uses:

```csharp
GetComponentInParent<BreakableMaterial>()
```

so the collider does not have to be on the same GameObject as `BreakableMaterial`.

---

### `BreakSystem.cs`

Attached to the Player.

Handles:

* Detecting the equipped item
* Supporting bare hands
* Finding the current break target
* Checking the breakable layer
* Checking break distance
* Finding the correct `ToolBreakPreset`
* Applying damage
* Applying breaking cooldown
* Preventing breaking when clicking the hotbar

---

### `ToolBreakPreset.cs`

A ScriptableObject that defines **how a tool breaks materials**.

Handles:

* Whether a tool is required
* Required tool
* Materials the tool can break
* Damage
* Breaking cooldown

**Drops are not stored here.**

---

### `DropItemData.cs`

Defines an individual drop.

Handles:

* Item to drop
* Minimum amount
* Maximum amount

---

# 2. System Structure

The system separates **breaking behavior** from **material drops**.

```text
ToolBreakPreset
    ↓
Defines HOW something is broken

BreakableMaterial
    ↓
Defines WHAT the object is and WHAT it drops
```

For example:

```text
Axe
    ↓
Can break Tree
    ↓
Deals 1 damage
    ↓
0.4 second cooldown
```

The Tree itself decides what it drops:

```text
Tree
    ↓
Wood × 2–5
Apple × 1–2
Seed × 1–3
```

This means different trees can have different drops without creating different tool presets.

---

# 3. Player Setup

Your player should have:

```text
Player
├── PlayerHeldItem
├── BreakSystem
├── BreakHitbox
└── ...
```

`BreakSystem` automatically looks for `PlayerHeldItem` on the same GameObject.

The `BreakHitbox` should have a `Collider2D` with:

```text
Is Trigger = true
```

Example:

```text
Player
└── BreakHitbox
    ├── BreakHitbox.cs
    └── Collider2D
```

Assign the Player's `BreakSystem` to:

```text
BreakHitbox
└── Break System
```

---

# 4. Break Hitbox

The system does not use the mouse position to select a material.

Instead, the player's break hitbox determines the current target.

```text
Player
    ↓
BreakHitbox
    ↓
Detects BreakableMaterial
    ↓
BreakSystem.SetTarget()
    ↓
Current break target
```

When the object leaves the hitbox:

```text
BreakableMaterial leaves hitbox
    ↓
BreakSystem.RemoveTarget()
    ↓
Target cleared
```

This allows the player to break objects based on their position and hitbox rather than directly clicking an object.

---

# 5. Breakable Object Setup

To make an object breakable:

1. Add `BreakableMaterial`.
2. Add a `Collider2D`.
3. Put the object on the `Breakable` layer.
4. Configure its material type.
5. Configure its health.
6. Configure its drops.

Example:

```text
Tree
├── SpriteRenderer
├── Collider2D
└── BreakableMaterial
```

---

# 6. BreakableMaterial Inspector

A breakable object contains:

```text
Material Type
Max Health
Drops
```

## Material Type

Identifies what type of material the object is.

Example:

```text
Tree
```

```text
Stone
```

```text
IronOre
```

The name must match the material listed in the appropriate `ToolBreakPreset`.

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

The comparison is case-sensitive.

---

## Max Health

Determines how much damage the object can take.

Example:

```text
Max Health = 3
```

With a tool dealing 1 damage:

```text
Hit 1 → 2 HP
Hit 2 → 1 HP
Hit 3 → 0 HP → Break
```

Health cannot go below:

```text
0
```

---

# 7. Material Drops

Drops are configured directly on the `BreakableMaterial`.

This is the main change from the previous system.

Example:

```text
Tree
├── Material Type = Tree
├── Max Health = 3
└── Drops
    ├── Wood → 2–5
    ├── Apple → 1–2
    └── Seed → 1–3
```

Another tree can have completely different drops:

```text
DeadTree
├── Material Type = Tree
├── Max Health = 5
└── Drops
    ├── Wood → 3–7
    └── Seed → 0–1
```

The tool does not control these drops.

The material does.

---

# 8. DropItemData

Each entry in `BreakableMaterial → Drops` uses `DropItemData`.

Example:

```text
Drops
Size = 2

Element 0
    Item = Wood
    Min Amount = 2
    Max Amount = 5

Element 1
    Item = Apple
    Min Amount = 1
    Max Amount = 2
```

When the material breaks, each drop is processed independently.

For example:

```text
Tree breaks
    ↓
Wood → random amount from 2–5
    ↓
Apple → random amount from 1–2
```

The dropped item is created using:

```csharp
CloneItem(amount)
```

and placed at the broken object's position.

---

# 9. Drop Amounts

Each drop has:

```text
Min Amount
Max Amount
```

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

The system uses:

```csharp
Random.Range(
    minAmount,
    maxAmount + 1
);
```

The current implementation ensures the minimum amount is at least `1`.

---

# 10. Breakable Layer

Create a layer called:

```text
Breakable
```

Assign it to objects that can be broken.

Example:

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

Objects outside this layer will not be broken.

---

# 11. Tool Break Presets

Create a preset through:

```text
Project Window
→ Right Click
→ Create
→ Game
→ Tool Break Preset
```

Example:

```text
BreakPresets
├── Axe_BreakPreset
├── Pickaxe_BreakPreset
└── BareHands_BreakPreset
```

A preset controls:

```text
Requires Tool
Required Tool
Breakable Materials
Damage
Break Cooldown
```

It does **not** contain drops.

---

# 12. Tool Requirement

### Tool Required

Set:

```text
Requires Tool = true
```

Then assign:

```text
Required Tool = Axe
```

The preset can then be used when the player has the Axe equipped.

---

### Bare Hands

Set:

```text
Requires Tool = false
```

The player can use this preset with:

```text
Held Item = null
```

For example:

```text
BareHands_BreakPreset

Requires Tool = false

Breakable Materials
    Bush

Damage = 1
Break Cooldown = 0.5
```

---

# 13. Breakable Materials

A preset can support multiple materials.

Example:

```text
Axe_BreakPreset

Breakable Materials
    Tree
    Bush
    WoodenCrate
```

This means:

```text
Axe → Tree = Valid
Axe → Bush = Valid
Axe → WoodenCrate = Valid
Axe → Stone = Invalid
```

The material itself determines its drops.

---

# 14. Damage

Damage is configured in the `ToolBreakPreset`.

Example:

```text
Wooden Axe
Damage = 1
```

```text
Iron Axe
Damage = 2
```

The damage is applied whenever the player successfully hits the material.

For example:

```text
Tree HP = 5

Axe Damage = 2

Hit 1 → 3 HP
Hit 2 → 1 HP
Hit 3 → 0 HP → Break
```

---

# 15. Breaking Cooldown

The preset also controls how quickly the player can hit again.

Example:

```text
Break Cooldown = 0.4
```

means the player must wait approximately 0.4 seconds before another hit.

Example:

```text
Wooden Axe
Damage = 1
Cooldown = 0.5
```

```text
Iron Axe
Damage = 2
Cooldown = 0.3
```

Lower cooldown means faster breaking.

---

# 16. BreakSystem Setup

Add:

```text
BreakSystem.cs
```

to the Player.

Configure:

```text
Player
    → Player Transform

Break Range
    → 2.5

Tool Presets
    → Your ToolBreakPreset assets

Breakable Layer
    → Breakable

Hotbar Panel
    → Your Hotbar Panel
```

---

# 17. Break Range

Example:

```text
Break Range = 2.5
```

The material must be within this distance from the player.

If it is too far away:

```text
Material is too far away.
```

The break attempt is cancelled.

---

# 18. Hotbar Protection

`BreakSystem` checks whether the mouse is over the hotbar before attempting to break.

```text
Left Click
    ↓
Mouse over hotbar?
    ↓
YES → Do nothing
    ↓
NO → Try to break
```

This prevents clicking a hotbar slot from also triggering the breaking system.

---

# 19. Breaking Process

The complete process is:

```text
Player moves near material
        ↓
BreakHitbox detects material
        ↓
BreakSystem stores target
        ↓
Player left-clicks
        ↓
Check hotbar
        ↓
Get equipped Item
        ↓
Check cooldown
        ↓
Check target
        ↓
Check Breakable layer
        ↓
Get BreakableMaterial
        ↓
Check break distance
        ↓
Find ToolBreakPreset
        ↓
Apply preset damage
        ↓
Material HP reaches 0?
        ↓
YES
        ↓
Create material's configured drops
        ↓
Destroy material
```

---

# 20. Preset Matching

`BreakSystem` searches through the assigned `ToolBreakPreset` assets.

First, the material type must match.

Example:

```text
Material Type = Stone
```

must exist in:

```text
Breakable Materials
    Stone
```

Then the tool requirement is checked.

### Bare Hands

```text
Requires Tool = false
```

The preset can be used without an equipped item.

### Tool Required

```text
Requires Tool = true
```

The equipped item's ID must match:

```csharp
preset.requiredTool.ID == tool.ID
```

---

# 21. Example Axe Setup

### Axe Preset

```text
Axe_BreakPreset

Requires Tool = true
Required Tool = Axe

Breakable Materials
    Tree

Damage = 1
Break Cooldown = 0.4
```

### Tree

```text
Tree

Material Type = Tree
Max Health = 3

Drops
    Wood → 2–5
    Apple → 1–2
    Seed → 1–3
```

Gameplay:

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
Tree breaks
    ↓
Tree's drops are created
```

---

# 22. Example Pickaxe Setup

### Pickaxe Preset

```text
Pickaxe_BreakPreset

Requires Tool = true
Required Tool = Pickaxe

Breakable Materials
    Stone
    IronOre
    CoalOre

Damage = 1
Break Cooldown = 0.5
```

### Stone

```text
Stone

Material Type = Stone
Max Health = 5

Drops
    Stone → 1–3
    Coal → 1–2
```

### Iron Ore

```text
IronOre

Material Type = IronOre
Max Health = 8

Drops
    IronOre → 1–2
```

The same Pickaxe preset can break both materials, while each material has its own drops.

---

# 23. Example Bare Hands Setup

### Bare Hands Preset

```text
BareHands_BreakPreset

Requires Tool = false

Breakable Materials
    Bush

Damage = 1
Break Cooldown = 0.5
```

### Bush

```text
Bush

Material Type = Bush
Max Health = 2

Drops
    Fiber → 1–3
    Seed → 1–2
```

The player does not need to equip an item.

---

# 24. Common Problems

### "No breakable object aligned with player."

Check:

* `BreakHitbox` exists.
* `BreakHitbox` has a `Collider2D`.
* `Is Trigger` is enabled.
* Breakable object has a `Collider2D`.
* Breakable object has `BreakableMaterial`.
* `BreakHitbox.breakSystem` references the Player's `BreakSystem`.
* The hitbox can detect the breakable object's collider.

---

### "Target does not have BreakableMaterial."

The detected collider could not find:

```csharp
GetComponentInParent<BreakableMaterial>()
```

Make sure the collider is on the same GameObject as `BreakableMaterial` or somewhere below it in the hierarchy.

---

### "Material is too far away."

The target is outside:

```text
Break Range
```

Increase the range or move closer.

---

### "Axe cannot break Tree."

Check:

```text
Axe_BreakPreset

Requires Tool = true
Required Tool = Axe

Breakable Materials
    Tree
```

Also check that the required Axe and equipped Axe have matching Item IDs.

---

### "Bare hands cannot break Tree."

Check that a preset exists with:

```text
Requires Tool = false
```

and:

```text
Breakable Materials
    Tree
```

---

### "No Drop Items assigned."

Select the breakable object and check:

```text
BreakableMaterial
└── Drops
```

Make sure the array contains at least one `DropItemData`.

---

### "A Drop Item is missing."

Check every element inside:

```text
BreakableMaterial
└── Drops
```

and make sure:

```text
Item
```

is assigned.

---

### "CloneItem returned null."

Check the `Item` reference and your:

```csharp
CloneItem(int newQuantity)
```

implementation.

---

### "The material has negative HP."

`BreakableMaterial` prevents this using:

```csharp
currentHealth = Mathf.Max(currentHealth, 0);
```

Health should never go below `0`.

---

# 25. Adding a New Material

To add a new breakable material:

1. Create the material prefab.
2. Add a `Collider2D`.
3. Add `BreakableMaterial`.
4. Set its material type.
5. Set its health.
6. Configure its drops.
7. Put it on the `Breakable` layer.
8. Add its material type to the appropriate `ToolBreakPreset`.

Example:

```text
IronOre

Material Type = IronOre
Max Health = 10

Drops
    IronOre → 1–2
    Stone → 0–1
```

No changes to `BreakSystem.cs` are normally required.

---

# 26. Adding a New Tool

To add a new tool:

1. Create the tool Item.
2. Create a `ToolBreakPreset`.
3. Set `Requires Tool = true`.
4. Assign the tool to `Required Tool`.
5. Add the materials it can break.
6. Set damage.
7. Set breaking cooldown.
8. Add the preset to `BreakSystem`.

Example:

```text
Hammer_BreakPreset

Requires Tool = true
Required Tool = Hammer

Breakable Materials
    Stone
    WoodenCrate

Damage = 3
Break Cooldown = 0.6
```

Drops do not need to be configured in the tool preset.

---

# 27. Adding Bare-Hand Breaking

To allow bare hands to break a material:

1. Create a `ToolBreakPreset`.
2. Set:

```text
Requires Tool = false
```

3. Add the materials.
4. Set damage.
5. Set cooldown.
6. Add the preset to `BreakSystem`.
7. Configure the material's drops in `BreakableMaterial`.

---

# 28. Project Structure

A simple project structure:

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
    ├── Stone
    └── IronOre
```

---

# 29. System Architecture

```text
                         PLAYER
                            │
             ┌──────────────┴──────────────┐
             │                             │
      PlayerHeldItem                 BreakSystem
             │                             │
             │                    Finds break preset
             │                    Checks range
             │                    Applies damage
             │                             │
             │                       BreakHitbox
             │                             │
             │                       Finds target
             │                             │
             └──────────────┬──────────────┘
                            │
                    BreakableMaterial
                            │
                    ┌───────┴───────┐
                    │               │
                  Health          Drops
                    │               │
               Takes damage    DropItemData
                    │
                 Breaks
                    │
                 Destroy
```

### Responsibility Summary

```text
BreakHitbox
→ Finds the current target

BreakSystem
→ Decides whether/how the target can be broken

ToolBreakPreset
→ Defines tool/bare-hand breaking behavior

BreakableMaterial
→ Defines the material, health, and drops

DropItemData
→ Defines an individual drop
```

The key design rule is:

> **Tools define how something is broken. Materials define what they give when broken.**