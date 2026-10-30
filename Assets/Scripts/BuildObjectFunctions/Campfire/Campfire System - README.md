# Campfire System

The Campfire System provides a central survival station that allows the player to feed Wood, maintain fuel, heal the player, increase the campfire level, and display its current progression.

The system is designed around a single global campfire controller while the visual, trigger, and UI components remain attached to the campfire or UI objects.

---

## 1. System Overview

The Campfire System consists of four main scripts:

* `CampfireController.cs`
* `CampfireLightController.cs`
* `CampfireTrigger.cs`
* `CampfireUI.cs`

The system interacts with existing inventory and player systems to receive Wood and apply healing.

### Main Responsibilities

| Script                    | Responsibility                                             |
| ------------------------- | ---------------------------------------------------------- |
| `CampfireController`      | Handles Wood, fuel, leveling, healing, and progression     |
| `CampfireLightController` | Controls the campfire's lit/unlit visuals and Light2D      |
| `CampfireTrigger`         | Detects when the player enters or leaves the healing range |
| `CampfireUI`              | Displays campfire level and Wood progression               |

---

## 2. Hierarchy

### GameController

The `CampfireController` is placed globally under the `GameController`.

```text
GameController
└── ItemFunctionController
    └── CampfireController
```

The controller is not placed directly on the Campfire prefab.

### Campfire Prefab

The Campfire prefab contains the visual, lighting, and trigger components.

```text
Campfire
├── Light2D
├── CampfireLightController
├── CampfireTrigger
└── UnlitVisual
```

The Campfire's main `SpriteRenderer` represents the lit visual.

`UnlitVisual` is shown when the campfire has no Wood.

---

## 3. CampfireController

`CampfireController.cs` is the main controller for the Campfire System.

It manages:

* Current Wood
* Maximum Wood
* Fuel decay
* Campfire level
* Level progression
* Healing
* Healing rate
* Player detection state
* Campfire UI updates
* Wood item validation

### Main Fields

```text
baseHealPercentPerSecond
healPercentPerSecond

progressionWood
campfireLevel

currentWood
maxWood
woodItemID

fuelDecayDelay
fuelDecayInterval
fuelDecayAmount
```

### Campfire Level

The campfire has a maximum level of 5.

```text
Level 0
Level 1
Level 2
Level 3
Level 4
Level 5
```

The level is stored permanently in `campfireLevel` while fuel is represented separately by `currentWood`.

The public `CampfireLevel` property allows other systems, such as the future Crafting System, to read the current campfire level.

---

## 4. Campfire Level Progression

Wood added to the campfire acts as both:

1. Fuel
2. Level progression

The amount required for each level is cumulative.

| Campfire Level    | Wood Required |
| ----------------- | ------------: |
| Level 0 → Level 1 |            10 |
| Level 1 → Level 2 |            25 |
| Level 2 → Level 3 |            40 |
| Level 3 → Level 4 |            55 |
| Level 4 → Level 5 |            70 |

Level 5 is the maximum level.

### Level-Up Behavior

When the current Wood reaches the required amount:

1. The required Wood is consumed.
2. 2 Wood is added back as starting fuel.
3. The campfire level increases by 1.
4. The healing rate is updated.
5. The player's maximum HP is updated.

### Example

If the campfire is Level 0 and receives 10 Wood:

```text
10 Wood added
↓
10 Wood consumed for progression
↓
2 Wood returned as fuel
↓
Campfire becomes Level 1
↓
Current Wood = 2
```

If 15 Wood is added:

```text
15 Wood added
↓
10 Wood used for Level 1
↓
2 Wood returned
↓
3 Wood remains
↓
Current Wood = 5
```

The excess Wood is preserved.

---

## 5. Fuel System

`currentWood` also functions as the Campfire's fuel.

Fuel decay begins after the configured delay.

### Default Settings

```text
Fuel Decay Delay: 10 seconds
Fuel Decay Interval: 5 seconds
Fuel Decay Amount: 1 Wood
```

The sequence is:

```text
Wood added
↓
Fuel timer resets
↓
10 second delay
↓
Every 5 seconds
↓
1 Wood is consumed
```

When `currentWood` reaches 0:

```text
currentWood = 0
```

The campfire remains at its current level.

Fuel depletion **does not decrease the Campfire Level**.

### Important

Campfire Level and Campfire Fuel are separate values.

```text
Campfire Level = permanent progression
Current Wood = temporary fuel
```

---

## 6. Healing System

The Campfire heals the player while:

* The player is inside the Campfire trigger.
* The player has been detected by `CampfireTrigger`.
* `currentWood` is greater than 0.

If the Campfire has no Wood, healing stops.

### Base Healing

The default base healing rate is:

```text
2.0% of maximum HP per second
```

Each Campfire Level increases the healing rate by:

```text
0.2% per second
```

### Healing Rates

| Level   | Healing Rate |
| ------- | -----------: |
| Level 0 |   2.0% / sec |
| Level 1 |   2.2% / sec |
| Level 2 |   2.4% / sec |
| Level 3 |   2.6% / sec |
| Level 4 |   2.8% / sec |
| Level 5 |   3.0% / sec |

Healing is calculated from the player's **maximum HP**, rather than a fixed amount.

```text
Heal Amount =
Player Max HP × Healing Rate × Time
```

This allows the healing amount to automatically scale with the player's increased maximum HP.

---

## 7. Player Maximum HP Progression

The Campfire Level also increases the player's maximum HP.

The player's base maximum HP is:

```text
100 HP
```

Each Campfire Level adds:

```text
20 HP
```

### Maximum HP

| Campfire Level | Maximum HP |
| -------------- | ---------: |
| Level 0        |        100 |
| Level 1        |        120 |
| Level 2        |        140 |
| Level 3        |        160 |
| Level 4        |        180 |
| Level 5        |        200 |

The Campfire calls:

```text
HealthController.UpdateMaxHealth(campfireLevel)
```

when the Campfire levels up.

---

## 8. Wood Feeding

The Campfire uses the player's Wood item.

`woodItemID` determines which item can be fed into the Campfire.

The `CanFeedWood()` method checks:

1. The player is inside the Campfire range.
2. The supplied item is not null.
3. The item's ID matches `woodItemID`.

The actual feeding input is handled by the player's existing inventory/hotbar system.

### Controls

```text
E
Feed 1 Wood

Shift + E
Feed the entire selected Wood stack
```

Wood is added through:

```text
CampfireController.AddWood(amount)
```

After Wood is added, the fuel timers are reset.

---

## 9. Campfire Trigger

`CampfireTrigger.cs` detects when the player enters or leaves the Campfire's healing range.

The trigger searches for the global `CampfireController`.

### On Enter

When the player enters the trigger:

```text
Player enters Campfire Trigger
↓
HealthController is detected
↓
CampfireController.PlayerEntered()
↓
Player becomes eligible for healing
```

### On Exit

When the player leaves:

```text
Player exits Campfire Trigger
↓
CampfireController.PlayerExited()
↓
Player is no longer healed
```

The trigger uses the player's root object to locate the `HealthController`.

---

## 10. Campfire Lighting

`CampfireLightController.cs` controls the Campfire's visual state.

The Campfire has two visual states:

```text
Lit
Unlit
```

The state is determined entirely by `currentWood`.

### With Wood

```text
currentWood > 0
```

The system:

* Enables the Light2D.
* Shows the lit Campfire SpriteRenderer.
* Hides `UnlitVisual`.

### Without Wood

```text
currentWood = 0
```

The system:

* Disables the Light2D.
* Hides the lit Campfire SpriteRenderer.
* Shows `UnlitVisual`.

The lighting state is synchronized using:

```text
CampfireController.currentWood
```

The initial state is also set during `Awake()` to prevent the Campfire's prefab state from briefly appearing before the correct state is applied.

---

## 11. Campfire UI

`CampfireUI.cs` displays the current Campfire progression.

The UI contains:

```text
Level Text
Progress Text
Progress Bar
```

### Level Text

Displays:

```text
Campfire (Level 0)
Campfire (Level 1)
Campfire (Level 2)
...
Campfire (Level 5)
```

### Progress Text

Displays the current Wood progression.

Example:

```text
7 / 10
```

The displayed Wood is capped at the current level's requirement so that the progress bar does not exceed its maximum.

### Progress Bar

The progress bar uses:

```text
Current Wood / Required Wood
```

to determine its fill amount.

---

## 12. Campfire UI Registration

When `CampfireUI` starts, it searches for the global `CampfireController`.

It then registers itself using:

```text
CampfireController.RegisterCampfireUI()
```

The Campfire Controller can then update the UI whenever:

* Wood is added.
* Fuel decreases.
* The Campfire levels up.
* The UI is registered.

---

## 13. Complete System Flow

The overall Campfire System works as follows:

```text
Player selects Wood
        ↓
Player presses E
        ↓
Campfire checks Wood item
        ↓
Wood is added
        ↓
Current Wood increases
        ↓
Campfire checks level requirement
        ↓
 ┌──────┴──────┐
 │             │
No Level Up   Level Up
 │             │
 │             ├── Required Wood consumed
 │             ├── 2 Wood returned
 │             ├── Level increases
 │             ├── Healing rate increases
 │             └── Player Max HP increases
 │
 └──────┬──────┘
        ↓
Fuel timers reset
        ↓
Campfire UI updates
        ↓
Campfire becomes lit
        ↓
Fuel begins decaying
        ↓
Current Wood reaches 0
        ↓
Healing stops
        ↓
Light turns off
        ↓
Unlit visual appears
```

---

## 14. Dependencies

The Campfire System interacts with several existing Project Survival systems.

### `Item.cs`

Used to identify the Wood item through its item ID.

### `HealthController.cs`

Used to:

* Heal the player.
* Update the player's maximum HP when the Campfire levels up.

### `HotbarController.cs`

Handles the player's selected item and the feeding input.

### `InventoryController.cs`

Handles the player's Wood quantity when Wood is consumed by the Campfire.

These scripts are dependencies of the Campfire System but are not Campfire-specific components.

---

## 15. Inspector Setup

### CampfireController

Place the controller at:

```text
GameController
└── ItemFunctionController
    └── CampfireController
```

Configure:

```text
Base Heal Percent Per Second
Current Wood
Max Wood
Wood Item ID
Fuel Decay Delay
Fuel Decay Interval
Fuel Decay Amount
```

Default values:

```text
Base Heal Percent Per Second = 2
Current Wood = 0
Max Wood = 100
Fuel Decay Delay = 10
Fuel Decay Interval = 5
Fuel Decay Amount = 1
```

Set `woodItemID` to the ID of the Wood item in the `ItemDictionary`.

### CampfireLightController

Attach it to the Campfire object containing:

```text
Light2D
SpriteRenderer
```

Assign:

```text
Unlit Visual
```

to the `unlitVisual` field.

### CampfireTrigger

Attach it to the Campfire's trigger object.

The trigger must have a 2D Collider with:

```text
Is Trigger = true
```

### CampfireUI

Assign:

```text
Level Text
Progress Text
Progress Bar
```

to their respective fields.

---

## 16. Future Crafting Integration

The Campfire System exposes:

```text
CampfireLevel
```

as a read-only property.

This is intended to allow the future Crafting System to check the current Campfire Level when determining which recipes are unlocked.

The Campfire Controller should remain responsible for:

* Campfire progression
* Fuel
* Healing
* Campfire state

The future Crafting System should handle:

* Recipes
* Crafting requirements
* Recipe unlocking

This keeps the two systems separated while allowing Campfire Level to control progression-based crafting unlocks.

---

## 17. Troubleshooting

### Campfire does not heal

Check:

* Player is inside the Campfire trigger.
* `HealthController` is present on the player.
* `currentWood` is greater than 0.
* `CampfireTrigger` can find the `CampfireController`.

### Campfire does not level up

Check:

* Wood is being added through `AddWood()`.
* `woodItemID` matches the Wood item's ID.
* Current Wood reaches the required threshold.
* Campfire Level is below 5.

### Campfire light stays on with no Wood

Check:

* `CampfireLightController` is attached to the correct Campfire object.
* The `Light2D` is on the same GameObject expected by `GetComponent<Light2D>()`.
* The lit SpriteRenderer is on the same GameObject expected by `GetComponent<SpriteRenderer>()`.
* `UnlitVisual` is assigned in the Inspector.

### UI does not update

Check:

* `CampfireUI` exists in the scene.
* `Level Text` is assigned.
* `Progress Text` is assigned.
* `Progress Bar` is assigned.
* `CampfireController` exists in the scene.

### Fuel does not decay

Check:

* `currentWood` is greater than 0.
* `fuelDecayDelay` has elapsed.
* `fuelDecayInterval` is configured correctly.
* `fuelDecayAmount` is greater than 0.

---

## 18. Current Campfire Features

The current Campfire System supports:

* Wood feeding with E.
* Feeding an entire Wood stack with Shift + E.
* Wood as both fuel and level progression.
* Five Campfire Levels.
* Permanent Campfire Level progression.
* Fuel decay.
* Level-based healing.
* Level-based player maximum HP.
* Campfire healing range detection.
* Lit and unlit Campfire visuals.
* Light2D control based on fuel.
* Campfire progression UI.
* Future Campfire Level access for crafting unlocks.

---

## 19. Current Limitations

The current system is designed around a **single Campfire** in the world.

`CampfireController` is global and stores the Campfire's state, while individual Campfire prefab components communicate with that controller.

The Campfire's current runtime state is not yet saved to disk. Closing the game will therefore reset runtime Campfire data unless a future save system stores:

* Campfire Level
* Current Wood
* Other required Campfire state

The existing world/building persistence system should be extended later if Campfire persistence across game sessions is required.

---

## 20. Setup Checklist

Before testing the Campfire System:

```text
[ ] CampfireController exists under GameController/ItemFunctionController
[ ] CampfireController has the correct Wood Item ID
[ ] Campfire prefab has CampfireLightController
[ ] Campfire prefab has CampfireTrigger
[ ] Campfire prefab has Light2D
[ ] Campfire prefab has a lit SpriteRenderer
[ ] UnlitVisual is assigned
[ ] Campfire trigger Collider2D has Is Trigger enabled
[ ] CampfireUI references are assigned
[ ] Player has HealthController
[ ] Wood exists in the ItemDictionary
[ ] Hotbar feeding input is working
```

---

## 21. Script Summary

```text
CampfireController
│
├── Wood / Fuel
├── Level Progression
├── Healing
├── Player HP Progression
└── UI Registration
        │
        ├────────────── CampfireUI
        │
        └────────────── HealthController


CampfireLightController
│
├── Light2D
├── Lit SpriteRenderer
└── UnlitVisual


CampfireTrigger
│
└── Player Detection
        │
        └── CampfireController


CampfireUI
│
├── Level Text
├── Progress Text
└── Progress Bar