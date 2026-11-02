# Lighting System

## Overview

The Lighting System manages lighting for held items and objects in the game world using Unity's Universal Render Pipeline `Light2D` components.

It is divided into three main systems:

1. **Item Lighting** – Controls lighting for held items, including retained light behavior.
2. **Global Lighting** – Controls world light activation based on the player's distance.
3. **World Object Lighting** – Handles custom light presets and nighttime flickering for supported world objects.

The system supports:

- Held item lighting.
- Retained held-item lighting.
- Distance-based world light activation and deactivation.
- Independent light prefab registration using string IDs.
- World object lighting presets.
- Multiple `Light2D` components per object.
- Smooth Perlin Noise flickering.
- Light radius variation.
- Day/night integration.
- E key light toggling for supported held items.
- Chunk-based world object registration and unregistration.

---

# 1. Requirements

This system uses Unity's Universal Render Pipeline and `Light2D`.

Make sure your project has:

- Universal Render Pipeline (URP).
- 2D Renderer.
- `Light2D` components.
- `PlayerHeldItem` component for held-item lighting.
- `Item` component for inventory items.
- Unity Input System.

---

# 2. Item Lighting

Item lighting is used for items that the player can hold.

The main scripts are:

- `ItemLightController`
- `ItemLightPreset`

The actual `Light2D` used for held-item lighting is located under the player's `PlayerHeldItem` object rather than directly on the item prefab.

The system uses the currently equipped item's `Item.ID` to determine which lighting preset should be used.

## Step 1: Create an Item Light Preset

In the Project window:

1. Right-click.
2. Select:

`Create > Lighting > Item Light Preset`

3. Give the preset a descriptive name.

Example:

`TorchLightPreset`

## Step 2: Assign the Item Prefab

Open the newly created `ItemLightPreset`.

Set:

`Item Prefab`

to the item prefab that should use this lighting.

Example:

```text
Torch Prefab
└── Item
    ├── Sprite Renderer
    └── ...
```

The prefab must contain an `Item` component.

The system compares the `ID` of the `Item` component.

The preset does not need to be assigned directly to the item prefab.

## Step 3: Configure the Light

The preset contains:

### Intensity

Controls the base brightness of the light.

Example:

```text
Intensity = 2
```

Higher values produce a brighter light.

### Radius

Controls how far the light reaches.

Example:

```text
Radius = 5
```

Higher values produce a larger light radius.

---

# 3. Flicker Settings

Flickering can be enabled from:

`Enable Flicker`

When enabled, the system uses smooth Perlin Noise to create natural-looking light variation.

Flickering continues while a supported item light is retained, even when the player switches to another item.

## Flicker Amount

Controls how much the light intensity changes.

Example:

```text
Flicker Amount = 0.15
```

Lower values create subtle flickering.

Higher values create stronger variation.

Recommended range:

```text
0.05 - 0.25
```

## Flicker Speed

Controls how quickly the flickering changes.

Example:

```text
Flicker Speed = 2
```

Higher values make the flickering change faster.

## Radius Variation

`Radius Variation` controls how much the light radius changes while flickering.

Example:

```text
Radius Variation = 0.03
```

Keep this value relatively low for stable-looking lights.

Recommended range:

```text
0.01 - 0.10
```

---

# 4. Setting Up ItemLightController

Add:

`ItemLightController`

to the GameObject responsible for controlling the player's held-item lighting.

The Inspector contains:

## Light Presets

Add all `ItemLightPreset` assets that can be used by the player.

Example:

```text
Light Presets
    Element 0 = TorchLightPreset
    Element 1 = LanternLightPreset
    Element 2 = MagicLightPreset
```

## Is Night

Controls whether item lighting is currently active.

```text
Is Night = true
```

means item lights are allowed to operate.

```text
Is Night = false
```

disables the held-item light.

This value can also be controlled through:

```csharp
SetNight(bool night)
```

## Player Held Item Object

Drag the GameObject containing the `PlayerHeldItem` component into:

`Player Held Item Object`

Example:

```text
Player
└── PlayerHeldItem
```

The controller searches this GameObject for the `PlayerHeldItem` component.

The system then checks which item is currently being held.

---

# 5. Item Light Setup

The actual `Light2D` must exist under the `PlayerHeldItem` GameObject or one of its children.

Example:

```text
Player
├── PlayerHeldItem
│   ├── HeldItemSprite
│   └── Light2D
└── ...
```

The controller searches the children of `PlayerHeldItem` for a `Light2D`.

The first `Light2D` it finds is used.

The controller automatically changes:

- Light intensity.
- Light radius.
- Light enabled state.

The light does not need to be placed on the Torch prefab.

---

# 6. Retained Item Lighting

Supported light-emitting items can have their light retained after the player switches to another item.

For example, a Torch can be activated and then remain lit while the player holds another item.

## Activating a Light

When a supported light-emitting item is equipped:

1. Press `E`.
2. `ItemLightController` checks for a matching `ItemLightPreset`.
3. The light is activated.
4. The item ID and lighting preset are stored as the retained light.
5. The light continues flickering using the stored preset.

Example:

```text
Torch equipped
      ↓
Press E
      ↓
Torch light ON
      ↓
Torch light is retained
```

## Switching Items

After the light has been retained, the player can switch to another item.

Example:

```text
Torch
  ↓
E
  ↓
Light ON
  ↓
Switch to Axe
  ↓
Light remains ON
```

The retained light continues using its original lighting preset.

This means the flickering effect continues even though the original light-emitting item is no longer equipped.

## Selecting the Light Item Again

Selecting the original light-emitting item again does not turn the retained light off.

Example:

```text
Torch
  ↓
E
  ↓
Light ON
  ↓
Switch to Axe
  ↓
Light remains ON
  ↓
Switch back to Torch
  ↓
Light remains ON
```

The retained light remains associated with the item's `Item.ID`.

## Turning the Retained Light Off

While the original light-emitting item is equipped, press `E` again.

Example:

```text
Torch equipped
      ↓
Light retained
      ↓
Press E
      ↓
Light OFF
```

The retained light state is cleared.

After this, switching items will no longer keep the light active.

---

# 7. Item Lighting Flow

The system works in this order:

```text
Player holds an item
        ↓
ItemLightController checks the current Item
        ↓
Finds matching ItemLightPreset
        ↓
Checks whether it is nighttime
        ↓
Finds Light2D under PlayerHeldItem
        ↓
Checks retained light state
        ↓
Applies preset settings
        ↓
Light appears and flickers
```

When a supported light item is activated with `E`:

```text
Light item equipped
        ↓
Press E
        ↓
Light is enabled
        ↓
Item ID and preset are retained
        ↓
Player switches items
        ↓
Retained preset continues controlling the light
```

If there is no matching preset and no retained light, the light is disabled.

If there is no held item but a retained light exists, the retained light can continue operating.

If it is daytime, the held-item light is disabled.

---

# 8. Global Lighting System

The Global Lighting System manages world light activation based on the player's distance.

Its purpose is to reduce unnecessary lighting workload by disabling lights that are far away from the player.

Instead of checking every light individually through separate scripts, a centralized controller manages registered lights.

The main scripts are:

- `GlobalLightingController`
- `WorldLightRegistration`

The system supports:

- Distance-based light activation and deactivation.
- Independent light prefab registration.
- String-based prefab identification.
- Multiple instances sharing the same prefab ID.
- Multiple `Light2D` components per object.
- Automatic registration and unregistration.
- Chunk-based world generation support.

## Global Lighting Architecture

### GlobalLightingController

The central controller responsible for:

- Maintaining a registry of world lights.
- Checking the distance between the player and registered lights.
- Activating and deactivating lights.
- Validating independent prefab IDs.
- Updating the player reference.

Attach this script to a persistent GameObject, such as `GameController`.

### WorldLightRegistration

The component attached to objects managed by the global lighting system.

Responsibilities:

- Detects all child `Light2D` components.
- Registers the object with `GlobalLightingController`.
- Unregisters the object when disabled.
- Controls light availability.
- Stores independent prefab identification information.

Attach this component to the root GameObject of each object managed by the system.

---

# 9. Setting Up GlobalLightingController

Add:

`GlobalLightingController`

to your GameController or another persistent GameObject.

Assign the Player Transform in the Inspector.

## References

| Setting | Description |
|---|---|
| Player | Transform used for distance calculations. |

## Distance Settings

| Setting | Default | Description |
|---|---:|---|
| Activation Distance | 25 | Distance at which lights become active. |
| Deactivation Distance | 30 | Distance at which active lights become inactive. |

The system uses separate activation and deactivation distances to prevent lights from rapidly switching on and off when the player is near the boundary.

This behavior is called hysteresis.

## Performance

| Setting | Default | Description |
|---|---:|---|
| Check Interval | 0.25 | Time interval between distance checks, in seconds. |

A lower value makes distance detection more frequent.

A higher value reduces how often the controller checks registered lights.

## Independent Light Prefab IDs

This array contains the IDs of independent light prefabs allowed to register.

Example:

```text
Independent Light Prefab IDs

Element 0 = FIREFLY
Element 1 = TREE
```

The IDs are compared as strings and must match exactly.

Multiple prefab variants can share the same ID if they should use the same registration permission.

---

# 10. Setting Up WorldLightRegistration

Open the prefab that contains the world light.

Select its root GameObject and add:

`WorldLightRegistration`

The Inspector contains:

| Setting | Description |
|---|---|
| Is Independent Prefab | Enables independent prefab ID validation. |
| Independent Prefab ID | Identifier used to validate the prefab. |
| Is Available | Allows registered lights to activate. |

There are two main ways to configure this component.

## Independent Light Prefabs

Use this setup for objects that do not use `WorldObjectLight` or `WorldObjectLightPreset`.

Examples:

- Fireflies.
- Trees.
- Other independent environmental lights.

Configure:

```text
Is Independent Prefab = Enabled
Independent Prefab ID = TREE
Is Available = Enabled
```

The ID must also exist in the `Independent Light Prefab IDs` array of `GlobalLightingController`.

## Preset-Based World Object Lights

For objects using `WorldObjectLight` and `WorldObjectLightPreset`, configure:

```text
Is Independent Prefab = Disabled
Is Available = Enabled
```

These objects still use global distance detection, but they do not need an independent prefab ID.

---

# 11. Independent Light Prefab Setup

Independent lights use string IDs instead of direct prefab references.

This avoids prefab self-reference problems when objects are instantiated by the world generator.

## Example: Firefly

Select the root GameObject of the Firefly prefab.

Add `WorldLightRegistration`.

Configure:

```text
Is Independent Prefab = Enabled
Independent Prefab ID = FIREFLY
Is Available = Enabled
```

Then add `FIREFLY` to the `GlobalLightingController` Inspector list.

## Example: Tree

Select the root GameObject of the Tree prefab.

Add `WorldLightRegistration`.

Configure:

```text
Is Independent Prefab = Enabled
Independent Prefab ID = TREE
Is Available = Enabled
```

Then add `TREE` to the `GlobalLightingController` Inspector list.

## Sharing IDs Between Prefabs

Multiple prefab variants can share the same ID.

Example:

| Prefab | Independent Prefab ID |
|---|---|
| Tree Oak | TREE |
| Tree Pine | TREE |
| Tree Birch | TREE |
| Tree Dead | TREE |

Only one `TREE` entry is needed in the controller's allowed ID list.

Each spawned instance still registers separately and receives its own distance checks.

---

# 12. Multiple Light2D Components

`WorldLightRegistration` automatically detects all `Light2D` components attached to the root GameObject and its children.

It uses:

```csharp
GetComponentsInChildren<Light2D>(true);
```

This means multiple lights can be controlled by a single registration component.

Example:

```text
Tree (Root)
├── Light2D
└── Light
    └── Light2D
```

Both Light2D components activate and deactivate together.

There is no need to add a separate `WorldLightRegistration` to each light.

Another example:

```text
Firefly (Root)
└── Glow
    └── Light2D
```

The registration component on the Firefly root automatically detects the child Light2D.

---

# 13. Distance Detection Logic

The controller uses squared distance calculations instead of calculating the actual distance.

This avoids unnecessary square root calculations.

The activation and deactivation behavior is:

| Player Distance | Light State |
|---|---|
| 0–25 units | Activated |
| 25–30 units | Retains previous state |
| 30 units or more | Deactivated |

The actual thresholds can be changed in the Inspector.

Distance checks occur at the configured `Check Interval`, rather than every frame.

---

# 14. Registration Lifecycle

When an object containing `WorldLightRegistration` becomes enabled:

1. Its Light2D components are detected.
2. It attempts to register with `GlobalLightingController`.
3. Independent prefab IDs are validated when applicable.
4. The controller checks the object's distance from the player.
5. Its lights are enabled or disabled according to availability and distance.

When the object becomes disabled:

1. It unregisters from the global controller.
2. Its distance state is reset.
3. Its Light2D components are disabled.

This supports chunk-based world generation because objects disabled or destroyed during chunk unloading automatically unregister through their lifecycle methods.

---

# 15. Runtime Light Availability

`WorldLightRegistration` provides:

```csharp
SetAvailable(bool available);
```

This allows other scripts to control whether registered lights are allowed to activate.

To disable the lights:

```csharp
GetComponent<WorldLightRegistration>()
    .SetAvailable(false);
```

To make them available again:

```csharp
GetComponent<WorldLightRegistration>()
    .SetAvailable(true);
```

Availability does not override the distance check.

Both conditions must be satisfied:

- The light must be available.
- The object must be within the activation range.

---

# 16. Updating the Player Reference

The player can be assigned directly through the Inspector.

Alternatively, another script can update the reference at runtime:

```csharp
GlobalLightingController.Instance.SetPlayer(playerTransform);
```

This also refreshes the registered lights using the new player position.

---

# 17. World Object Lighting

World object lighting is used for objects placed in the game world that require custom light settings and optional flickering.

Examples:

- Torches.
- Lamps.
- Campfires.
- Street lights.
- Candles.
- Other environmental light sources.

The main scripts are:

- `WorldObjectLightController`
- `WorldObjectLight`
- `WorldObjectLightPreset`
- `WorldLightRegistration`

The global distance controller and world-object lighting controller have separate responsibilities.

| Script | Responsibility |
|---|---|
| GlobalLightingController | Distance-based light activation. |
| WorldLightRegistration | Registers and controls Light2D components. |
| WorldObjectLightController | Manages registered world-object lights and nighttime flickering. |
| WorldObjectLight | Applies presets and updates flickering. |

---

# 18. Create a World Object Light Preset

In the Project window:

1. Right-click.
2. Select:

`Create > Lighting > World Object Light Preset`

3. Give the preset a descriptive name.

Example:

`TorchWorldLightPreset`

The preset contains settings such as:

- Intensity.
- Radius.
- Enable Flicker.
- Flicker Amount.
- Flicker Speed.
- Radius Variation.

---

# 19. Setting Up WorldObjectLight

Open the world object prefab.

Attach the following components to its root GameObject:

- `WorldLightRegistration`
- `WorldObjectLight`

Example hierarchy:

```text
Torch (Root)
├── Sprite
└── Light2D
```

Configure `WorldLightRegistration`:

```text
Is Independent Prefab = Disabled
Is Available = Enabled
```

Configure `WorldObjectLight`:

```text
Preset = TorchWorldLightPreset
```

The `WorldObjectLight` component stores its own preset reference.

The preset must be assigned directly to the component.

It does not automatically search for a matching prefab name.

## Multiple Light2D Components

Multiple Light2D components are supported.

Example:

```text
Lamp (Root)
├── Sprite
├── Light2D
└── Light2D
```

The `WorldObjectLight` component detects its child Light2D components and applies the preset settings to them.

The `WorldLightRegistration` component also detects these lights and manages their distance-based activation.

---

# 20. Setting Up WorldObjectLightController

Add:

`WorldObjectLightController`

to a GameObject that exists in the scene.

A common setup is:

```text
GameController
├── GlobalLightingController
├── WorldObjectLightController
└── ...
```

## World Object Light Presets

The controller exposes a `lightPresets` array in the Inspector.

However, the current `WorldObjectLight` implementation uses the preset assigned directly to each component's `Preset` field.

Assign the required preset to each world object prefab through its `WorldObjectLight` component.

## Is Night

The controller contains:

```text
Is Night = true
```

This controls whether registered world-object lights update their flickering.

When nighttime is active, registered lights with flickering enabled can update their intensity and radius.

When daytime is active, the flickering update loop stops.

**Important:** `WorldObjectLightController.isNight` does not independently enable or disable all world-object Light2D components. Distance-based activation is handled by `GlobalLightingController` and `WorldLightRegistration`.

The value can be controlled through:

```csharp
SetNight(bool night);
```

When `SetNight(true)` is called, the controller refreshes registered world-object presets.

---

# 21. World Object Flickering

`WorldObjectLight` uses Perlin Noise to create smooth light variation.

The controller updates registered world-object lights while nighttime is active.

The following preset settings control the effect:

| Setting | Description |
|---|---|
| Enable Flicker | Enables or disables flickering. |
| Flicker Amount | Controls intensity variation. |
| Flicker Speed | Controls the speed of variation. |
| Radius Variation | Controls light radius variation. |

The script uses a randomized noise offset for each world object.

This prevents all registered lights from following exactly the same flickering pattern.

Only enabled Light2D components receive flicker updates.

---

# 22. Day/Night Integration

The lighting systems have separate day/night responsibilities.

## ItemLightController

Controls whether held-item lights are allowed to operate.

```csharp
itemLightController.SetNight(true);
```

For daytime:

```csharp
itemLightController.SetNight(false);
```

## WorldObjectLightController

Controls nighttime flickering updates for registered world-object lights.

```csharp
worldObjectLightController.SetNight(true);
```

For daytime:

```csharp
worldObjectLightController.SetNight(false);
```

## GlobalLightingController

The global controller does not manage day/night.

It only manages distance-based activation and deactivation.

Independent lights such as Fireflies and Trees are not automatically disabled during daytime by this controller.

If a particular world light needs day/night or fuel-based availability, its owning system must manage that state separately.

---

# 23. Refreshing Lights

## ItemLightController

```csharp
RefreshLights();
```

This refreshes the currently tracked held light and preset.

Use this if the held-item setup changes and the controller needs to search again.

## WorldObjectLightController

```csharp
RefreshLights();
```

This reapplies presets to registered `WorldObjectLight` components.

It does not search the scene for new objects.

## GlobalLightingController

```csharp
GlobalLightingController.Instance.RefreshAllLights();
```

This updates the distance thresholds and refreshes the distance state of registered lights.

Use this after changing distance settings at runtime or when a refresh is needed.

---

# 24. Recommended Folder Structure

A clean project structure could be:

```text
Lighting
├── Scripts
│   ├── ItemLightController.cs
│   ├── ItemLightPreset.cs
│   ├── GlobalLightingController.cs
│   ├── WorldLightRegistration.cs
│   ├── WorldObjectLightController.cs
│   ├── WorldObjectLight.cs
│   └── WorldObjectLightPreset.cs
│
└── Presets
    ├── Item
    │   ├── TorchLightPreset
    │   ├── LanternLightPreset
    │   └── MagicLightPreset
    │
    └── World
        ├── TorchWorldLightPreset
        ├── LampWorldLightPreset
        └── CampfireWorldLightPreset
```

---

# 25. Example Setup

## Player

```text
Player
├── PlayerHeldItem
│   ├── HeldItemSprite
│   └── Light2D
│
└── ItemLightController
```

Inspector:

```text
Item Light Controller

Light Presets
    TorchLightPreset
    LanternLightPreset

Is Night
    true

Player Held Item Object
    PlayerHeldItem
```

## GameController

```text
GameController
├── GlobalLightingController
└── WorldObjectLightController
```

GlobalLightingController Inspector:

```text
References
    Player = Player Transform

Distance Settings
    Activation Distance = 25
    Deactivation Distance = 30

Performance
    Check Interval = 0.25

Independent Light Prefab IDs
    FIREFLY
    TREE
```

## Independent Firefly Prefab

```text
Firefly (Root)
├── WorldLightRegistration
└── Glow
    └── Light2D
```

Inspector:

```text
WorldLightRegistration

Is Independent Prefab
    Enabled

Independent Prefab ID
    FIREFLY

Is Available
    Enabled
```

## Independent Tree Prefab

```text
Tree (Root)
├── WorldLightRegistration
├── Light2D
└── Light
    └── Light2D
```

Inspector:

```text
WorldLightRegistration

Is Independent Prefab
    Enabled

Independent Prefab ID
    TREE

Is Available
    Enabled
```

Both tree lights are controlled together by one registration component.

## Preset-Based Torch Prefab

```text
Torch (Root)
├── WorldLightRegistration
├── WorldObjectLight
├── Sprite
└── Light2D
```

Inspector:

```text
WorldLightRegistration

Is Independent Prefab
    Disabled

Is Available
    Enabled
```

```text
WorldObjectLight

Preset
    TorchWorldLightPreset
```

---

# 26. Troubleshooting

## Held Item Has No Light

Check:

1. `Player Held Item Object` is assigned.
2. The assigned GameObject has `PlayerHeldItem`.
3. The held item has an `Item` component.
4. The item's `ID` matches the preset item's `ID`.
5. The item has a matching `ItemLightPreset`.
6. A `Light2D` exists under `PlayerHeldItem`.
7. `Is Night` is enabled.

## Retained Light Does Not Stay On

Check:

1. The item has a matching `ItemLightPreset`.
2. The item's `Item.ID` matches the preset item ID.
3. The light was activated by pressing `E`.
4. `ItemLightController` is receiving the E key input.
5. `Is Night` is enabled.
6. The `Light2D` exists under `PlayerHeldItem`.

## Independent World Light Is Rejected

Check:

1. `Is Independent Prefab` is enabled.
2. The prefab's `Independent Prefab ID` is assigned.
3. The ID exactly matches an entry in `GlobalLightingController`.
4. The object has a `WorldLightRegistration` component on its root.

IDs are case-sensitive.

For example:

```text
TREE
```

does not match:

```text
Tree
```

## World Light Does Not Activate

Check:

1. `GlobalLightingController` exists in the scene.
2. The Player Transform is assigned.
3. The object has `WorldLightRegistration`.
4. `Is Available` is enabled.
5. The object is within the activation distance.
6. The Light2D components are children of the registered root GameObject.

## Only One Light2D Activates

Check whether all Light2D components are children of the GameObject containing `WorldLightRegistration`.

The component automatically detects all child Light2D components.

There is no need for separate registration components on individual lights.

## World Object Preset Is Not Applied

Check:

1. `WorldObjectLightController` exists in the scene.
2. `WorldObjectLight` is attached to the world object root.
3. A valid `WorldObjectLightPreset` is assigned to its `Preset` field.
4. The object has one or more child Light2D components.

## World Object Does Not Flicker

Check:

```text
Enable Flicker = true
```

Then adjust:

```text
Flicker Amount
Flicker Speed
Radius Variation
```

Also check:

1. `WorldObjectLightController` exists.
2. `isNight` is true.
3. The preset is assigned.
4. At least one detected Light2D is enabled.

## Light Does Not Turn Off When Leaving the Area

Check the `Deactivation Distance` setting.

The light remains active until it reaches or exceeds the deactivation distance.

This is intentional to prevent rapid activation and deactivation near the activation boundary.

## Light Does Not Register After Chunk Loading

Check:

1. `GlobalLightingController` exists before the spawned object registers.
2. The spawned object is active.
3. `WorldLightRegistration` is enabled.
4. The Player Transform is assigned.

The registration component uses `OnEnable()` to register and `OnDisable()` to unregister.

---

# 27. Quick Setup Checklist

## Item Lighting

```text
[ ] Create ItemLightPreset
[ ] Assign Item Prefab
[ ] Configure intensity
[ ] Configure radius
[ ] Configure flicker
[ ] Add preset to ItemLightController
[ ] Assign Player Held Item Object
[ ] Add Light2D under PlayerHeldItem
[ ] Make sure the item has an Item component
[ ] Make sure Item IDs match
[ ] Test E to activate the light
[ ] Test switching items while the light is active
[ ] Test E again while the light item is equipped
```

## Global Lighting

```text
[ ] Add GlobalLightingController to GameController
[ ] Assign the Player Transform
[ ] Configure activation distance
[ ] Configure deactivation distance
[ ] Configure check interval
[ ] Add independent prefab IDs
[ ] Add WorldLightRegistration to supported prefab roots
[ ] Configure independent prefab IDs where required
[ ] Make sure Is Available is enabled
[ ] Make sure all Light2D components are children of the registered root
[ ] Test light activation near the player
[ ] Test light deactivation away from the player
[ ] Test chunk loading and unloading
```

## World Object Lighting

```text
[ ] Create WorldObjectLightPreset
[ ] Configure intensity
[ ] Configure radius
[ ] Configure flicker
[ ] Add WorldLightRegistration to the prefab root
[ ] Keep Is Independent Prefab disabled
[ ] Add WorldObjectLight to the prefab root
[ ] Assign the preset directly to WorldObjectLight
[ ] Make sure Light2D exists on the object or its children
[ ] Add WorldObjectLightController to GameController
[ ] Test nighttime flickering
[ ] Test daytime flickering behavior
```

---

# 28. Important Notes

- The lighting system does not create `Light2D` components automatically.
- You must add `Light2D` to the appropriate prefab or held-item hierarchy.
- Item lighting uses the currently equipped item's `Item.ID` to identify its preset.
- Held-item lighting uses the `Light2D` under `PlayerHeldItem`.
- Retained item lights preserve their original item ID and lighting preset.
- Global lighting uses `WorldLightRegistration` to manage world light activation.
- Independent prefabs use string IDs instead of direct prefab references.
- Multiple prefab variants can share the same independent prefab ID.
- Multiple Light2D components can be controlled by one registration component.
- `WorldObjectLight` uses a preset assigned directly through its Inspector field.
- `WorldObjectLightController` handles nighttime flickering, while `GlobalLightingController` handles distance-based activation.
- Independent lights are not automatically disabled during daytime.
- Chunk unloading automatically unregisters objects when their registration components are disabled or destroyed.
- Player-held lights remain separate from the global world-light registration system.

The system separates held-item lighting, world-object preset behavior, and global distance-based activation so each controller can manage its own responsibilities without interfering with the others.