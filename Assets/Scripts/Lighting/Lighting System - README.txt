# Lighting System

This lighting system allows held items and world objects to automatically use custom `Light2D` settings during nighttime.

It supports:

* Held item lighting
* World object lighting
* Custom light presets
* Smooth flickering
* Light radius variation
* Day/night enabling and disabling
* Multiple item and world object presets

---

# 1. Requirements

This system uses Unity's Universal Render Pipeline and `Light2D`.

Make sure your project has:

* Universal Render Pipeline (URP)
* 2D Renderer
* `Light2D` components
* `PlayerHeldItem` component for held-item lighting
* `Item` component for inventory items

---

# 2. Item Lighting

Item lighting is used for items that the player can hold.

The main scripts are:

* `ItemLightController`
* `ItemLightPreset`

## Step 1: Create an Item Light Preset

In the Project window:

1. Right-click.
2. Select:

`Create > Lighting > Item Light Preset`

3. Give the preset a descriptive name.

Example:

`TorchLightPreset`

---

## Step 2: Assign the Item Prefab

Open the newly created `ItemLightPreset`.

Set:

`Item Prefab`

to the item prefab that should use this lighting.

The prefab must contain an `Item` component.

Example:

```text
Torch Prefab
└── Item
    ├── Sprite Renderer
    └── ...
```

The system compares the `ID` of the `Item` component.

The preset does not need to be assigned directly to the item prefab.

---

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

---

## Flicker Speed

Controls how quickly the flickering changes.

Example:

```text
Flicker Speed = 2
```

Higher values make the flickering change faster.

---

# 4. Radius Variation

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

# 5. Setting Up ItemLightController

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

---

## Is Night

Controls whether item lighting is currently active.

```text
Is Night = true
```

means item lights are allowed to turn on.

```text
Is Night = false
```

disables the held-item light.

This value can also be controlled through:

```csharp
SetNight(bool night)
```

---

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

# 6. Item Light Setup

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

* Light intensity
* Light radius
* Light enabled state

based on the active preset.

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
Applies preset settings
        ↓
Light appears
```

If there is no matching preset, the light is disabled.

If there is no held item, the light is disabled.

If it is daytime, the light is disabled.

---

# 8. World Object Lighting

World object lighting is used for objects placed in the game world.

Examples:

* Torches
* Lamps
* Campfires
* Street lights
* Candles
* Other environmental light sources

The main scripts are:

* `WorldObjectLightController`
* `WorldObjectLightPreset`

---

# 9. Create a World Object Light Preset

In the Project window:

1. Right-click.
2. Select:

`Create > Lighting > World Object Light Preset`

3. Give the preset a descriptive name.

Example:

`TorchWorldLightPreset`

---

# 10. Assign the World Object Prefab

Open the `WorldObjectLightPreset`.

Set:

`World Object Prefab`

to the prefab that should use the preset.

Example:

```text
TorchWorldPrefab
```

The controller compares the name of the spawned world object with the name of the assigned prefab.

For example:

```text
Prefab:
Torch

Spawned object:
Torch(Clone)
```

These are treated as the same object.

---

# 11. Add a Light2D to the World Object

The world object prefab must contain a `Light2D`.

Example:

```text
Torch
├── Sprite
└── Light2D
```

The controller searches the object's children for `Light2D` components.

Multiple lights are supported.

Example:

```text
Lamp
├── Sprite
├── Light2D
└── Light2D
```

Both lights will receive the preset settings.

---

# 12. Setting Up WorldObjectLightController

Add:

`WorldObjectLightController`

to a GameObject that exists in the scene.

A common setup is:

```text
GameController
├── WorldObjectLightController
└── ...
```

---

## Light Presets

Add all world object presets to:

`World Object Light Presets`

Example:

```text
World Object Light Presets
    Element 0 = TorchWorldLightPreset
    Element 1 = LampWorldLightPreset
    Element 2 = CampfireWorldLightPreset
```

---

## Is Night

Controls whether world object lights are active.

```text
Is Night = true
```

enables the lights.

```text
Is Night = false
```

disables them.

The value can also be controlled through:

```csharp
SetNight(bool night)
```

---

# 13. World Object Lighting Flow

The system works like this:

```text
WorldObjectLightController
        ↓
Checks all WorldObjectLightPreset assets
        ↓
Finds matching world objects in the scene
        ↓
Finds Light2D components
        ↓
Checks day/night state
        ↓
Applies the preset
        ↓
Light becomes active
```

---

# 14. Day/Night Integration

Both controllers provide:

```csharp
SetNight(bool night)
```

This can be called by a day/night system.

Example:

```csharp
itemLightController.SetNight(true);
worldObjectLightController.SetNight(true);
```

For daytime:

```csharp
itemLightController.SetNight(false);
worldObjectLightController.SetNight(false);
```

Recommended usage:

```text
Day
 ↓
SetNight(false)
 ↓
Lights disabled

Night
 ↓
SetNight(true)
 ↓
Lights enabled
```

---

# 15. Refreshing Lights

Both controllers provide a refresh method.

## ItemLightController

```csharp
RefreshLights();
```

This resets the currently tracked held light and preset.

Use this if the held-item setup changes and the controller needs to search again.

## WorldObjectLightController

```csharp
RefreshLights();
```

This clears the flicker data and searches the scene again.

It can be useful after spawning or changing world objects.

---

# 16. Recommended Folder Structure

A clean project structure could be:

```text
Lighting
├── Scripts
│   ├── ItemLightController.cs
│   ├── ItemLightPreset.cs
│   ├── WorldObjectLightController.cs
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

# 17. Example Setup

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

---

## World

```text
GameController
└── WorldObjectLightController
```

Inspector:

```text
World Object Light Controller

World Object Light Presets
    TorchWorldLightPreset
    LampWorldLightPreset

Is Night
    true
```

Example world prefab:

```text
Torch
├── Sprite
└── Light2D
```

---

# 18. Troubleshooting

## Held item has no light

Check:

1. `Player Held Item Object` is assigned.
2. The assigned GameObject has `PlayerHeldItem`.
3. The held item has an `Item` component.
4. The item's `ID` matches the preset item's `ID`.
5. The item has a matching `ItemLightPreset`.
6. A `Light2D` exists under `PlayerHeldItem`.
7. `Is Night` is enabled.

---

## World object has no light

Check:

1. `WorldObjectLightController` exists in the scene.
2. The world object has a `Light2D`.
3. The correct prefab is assigned to `World Object Prefab`.
4. The spawned object's name matches the prefab name.
5. The preset is added to the controller's `Light Presets`.
6. `Is Night` is enabled.

---

## Light does not flicker

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

If `Enable Flicker` is disabled, the light uses a constant intensity and radius.

---

# 19. Quick Setup Checklist

### Item Lighting

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
```

### World Object Lighting

```text
[ ] Create WorldObjectLightPreset
[ ] Assign World Object Prefab
[ ] Configure intensity
[ ] Configure radius
[ ] Configure flicker
[ ] Add Light2D to world object prefab
[ ] Add preset to WorldObjectLightController
[ ] Make sure Is Night is enabled
```

---

# 20. Important Notes

The lighting system does not create `Light2D` components automatically.

You must add the `Light2D` component to the appropriate prefab or held-item hierarchy.

Presets control the light's runtime properties. The actual `Light2D` remains on the item or world object.

For item lighting, matching is based on the `Item.ID`.

For world object lighting, matching is based on the prefab and spawned object name.

The system uses Perlin Noise for smooth flickering rather than completely random frame-by-frame changes.
