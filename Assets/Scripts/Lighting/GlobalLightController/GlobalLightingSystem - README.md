# Global Lighting System

## Overview

The Global Lighting System manages world light activation based on the player's distance. Its purpose is to reduce unnecessary lighting calculations and rendering workload by disabling lights that are far away from the player.

The system uses a centralized `GlobalLightingController` that tracks registered light objects and periodically checks their distance from the player.

It supports:

- Distance-based light activation and deactivation.
- Independent light prefab registration using string IDs.
- Multiple instances of the same prefab sharing an ID.
- Automatic detection of `Light2D` components in child objects.
- Automatic registration and unregistration when objects are enabled or disabled.
- Integration with chunk-based world generation.
- Separate world-object lighting presets and flickering.

---

# 1. System Architecture

The system consists of four scripts.

### GlobalLightingController.cs

The main controller responsible for managing registered world lights.

Responsibilities:

- Maintains a registry of active `WorldLightRegistration` components.
- Checks the distance between registered lights and the player.
- Activates or deactivates lights based on configured distances.
- Validates independent prefab IDs.
- Provides a method for updating the player reference.

This script should be attached to a persistent GameObject, such as `GameController`.

### WorldLightRegistration.cs

The registration component attached to objects containing world lights.

Responsibilities:

- Automatically detects all child `Light2D` components.
- Registers the object with `GlobalLightingController`.
- Unregisters the object when disabled.
- Controls whether its lights are allowed to activate.
- Supports independent prefab identification.

Attach this component to the root GameObject of each object managed by the global lighting system.

### WorldObjectLightController.cs

Manages world-object lighting presets and flickering.

Responsibilities:

- Maintains registered `WorldObjectLight` components.
- Controls nighttime flickering.
- Applies lighting presets when requested.
- Updates registered lights through the `SetNight()` method.

This controller operates separately from the global distance detection system.

### WorldObjectLight.cs

Handles preset-based lighting for world objects.

Responsibilities:

- Stores a `WorldObjectLightPreset` reference.
- Applies preset intensity and radius.
- Generates randomized Perlin Noise flickering.
- Provides a check for active lights.

This component requires `WorldLightRegistration`.

---

# 2. GameObject Setup

Create a GameObject named `GameController` or use your existing global controller object.

Attach:

- `GlobalLightingController`
- `WorldObjectLightController`

Assign the Player Transform to the `Player` field in `GlobalLightingController`.

The two controllers operate independently.

---

# 3. Global Lighting Configuration

Select the GameObject containing `GlobalLightingController`.

### References

| Setting | Description |
|---|---|
| Player | Transform used for distance calculations. |

### Distance Settings

| Setting | Default | Description |
|---|---:|---|
| Activation Distance | 25 | Distance at which lights become active. |
| Deactivation Distance | 30 | Distance at which active lights become inactive. |

The system uses separate activation and deactivation distances to prevent lights from rapidly switching on and off when the player is near the boundary.

This behavior is called hysteresis.

### Performance

| Setting | Default | Description |
|---|---:|---|
| Check Interval | 0.25 | Time interval between distance checks, in seconds. |

A lower value makes distance detection more frequent. A higher value reduces how often the controller checks registered lights.

### Independent Light Prefab IDs

This array contains the IDs of independent light prefabs allowed to register.

Only objects with `Is Independent Prefab` enabled are checked against this list.

Example:

| Element | ID |
|---|---|
| 0 | FIREFLY |
| 1 | TREE |

IDs are compared as strings and must match exactly.

Multiple prefab variants can share the same ID if they should use the same registration permission.

---

# 4. Setting Up Independent Light Prefabs

Independent lights are objects that use the global distance detection system without requiring `WorldObjectLight` or `WorldObjectLightPreset`.

Examples:

- Fireflies
- Trees
- Other environmental light sources
- Player-placed structures with independent lighting

### Step 1: Add WorldLightRegistration

Open the prefab and select its root GameObject.

Add the `WorldLightRegistration` component.

Configure:

| Setting | Description |
|---|---|
| Is Independent Prefab | Enable for independent light prefabs. |
| Independent Prefab ID | Unique or shared identifier for the prefab. |
| Is Available | Allows the registered lights to activate. |

### Step 2: Assign an ID

Example Firefly:

```text
Is Independent Prefab: Enabled
Independent Prefab ID: FIREFLY
Is Available: Enabled
```

Example Tree:

```text
Is Independent Prefab: Enabled
Independent Prefab ID: TREE
Is Available: Enabled
```

### Step 3: Add IDs to GlobalLightingController

Open the GameObject containing `GlobalLightingController`.

Under `Independent Light Prefab IDs`, add:

```text
FIREFLY
TREE
```

The ID on the prefab must match an entry in this array.

**Important:** Independent prefab IDs do not need to be unique per prefab asset. Multiple tree variants can share `TREE`.

---

# 5. Multiple Light2D Components

`WorldLightRegistration` automatically detects every `Light2D` component attached to the root GameObject and its children.

It uses:

```csharp
GetComponentsInChildren<Light2D>(true);
```

This means multiple lights can be controlled by a single registration component.

Example hierarchy:

```text
Tree (Root)
├── Light2D
└── Light
    └── Light2D
```

Both Light2D components will activate and deactivate together.

There is no need to add a separate `WorldLightRegistration` component to each light.

---

# 6. Setting Up World Object Lighting

World-object lighting uses a separate system for preset-based intensity, radius, and flickering.

### Step 1: Add WorldLightRegistration

Attach `WorldLightRegistration` to the root GameObject.

For ordinary world-object lights using presets:

```text
Is Independent Prefab: Disabled
Is Available: Enabled
```

### Step 2: Add WorldObjectLight

Attach `WorldObjectLight` to the same root GameObject.

Assign the appropriate `WorldObjectLightPreset` to its `Preset` field.

### Step 3: Configure the Preset

The preset controls:

- Light intensity.
- Light radius.
- Whether flickering is enabled.
- Flicker amount.
- Flicker speed.
- Radius variation.

The `WorldObjectLight` component applies the preset to its detected Light2D components.

### Step 4: Nighttime Flickering

`WorldObjectLightController` manages flickering.

When `isNight` is false, flickering updates stop.

When `SetNight(true)` is called, the controller refreshes registered world-object lights.

The global distance controller remains responsible for distance-based activation.

---

# 7. Registration Lifecycle

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

This supports chunk-based world generation because objects destroyed or disabled during chunk unloading automatically unregister through their lifecycle methods.

---

# 8. Distance Detection Logic

The controller uses squared distance calculations instead of calculating the actual distance.

This avoids unnecessary square root calculations.

The activation and deactivation behavior is:

| Player Distance | Light State |
|---|---|
| 0–25 units | Activated |
| 25–30 units | Retains previous state |
| 30 units or more | Deactivated |

The actual thresholds can be changed in the Inspector.

---

# 9. Runtime Availability

The `WorldLightRegistration` component provides:

```csharp
SetAvailable(bool available);
```

This allows other scripts to control whether the registered lights are allowed to activate.

Example:

```csharp
GetComponent<WorldLightRegistration>()
    .SetAvailable(false);
```

To make the lights available again:

```csharp
GetComponent<WorldLightRegistration>()
    .SetAvailable(true);
```

Availability does not override the distance check. Both availability and distance must permit activation.

---

# 10. Player Reference

The player can be assigned directly through the Inspector.

Alternatively, another script can update the reference at runtime:

```csharp
GlobalLightingController.Instance.SetPlayer(playerTransform);
```

This also refreshes the registered lights using the new player position.

---

# 11. Important Setup Notes

- Only objects containing `WorldLightRegistration` are managed by the global distance controller.
- Attach `WorldLightRegistration` to the root GameObject.
- Independent prefab IDs must match the entries in `GlobalLightingController`.
- Multiple prefab variants can share the same ID.
- All child Light2D components are detected automatically.
- Independent lights do not require `WorldObjectLight`.
- Preset-based world-object lights require both `WorldLightRegistration` and `WorldObjectLight`.
- Player-held lights should remain separate from this system.
- Make sure `GlobalLightingController` exists before dynamically spawned light objects register.
- The player reference must be assigned for distance detection to work.

---

# 12. Troubleshooting

### Light does not activate

Check:

- Is `GlobalLightingController` present in the scene?
- Is the Player Transform assigned?
- Is `WorldLightRegistration` attached to the prefab root?
- Is `Is Available` enabled?
- Is the object within the activation distance?

### Independent light is rejected

Check:

- Is `Is Independent Prefab` enabled?
- Is the prefab's ID assigned?
- Does the ID exactly match an entry in `Independent Light Prefab IDs`?

### Only one Light2D activates

Check whether all Light2D components are children of the GameObject containing `WorldLightRegistration`.

### World-object flickering does not work

Check:

- Is `WorldObjectLightController` present?
- Is `isNight` enabled?
- Is `WorldObjectLight` attached?
- Is a valid `WorldObjectLightPreset` assigned?
- Is flickering enabled in the preset?
- Is at least one detected Light2D enabled?

### Light remains disabled

Check both conditions:

- `Is Available` must be enabled.
- The object must be within the activation range.

---

# Summary

The Global Lighting System centralizes distance-based light activation for the survival world.

`GlobalLightingController` manages distance detection, while `WorldLightRegistration` handles individual light registration and activation.

Independent prefabs use string IDs for registration validation, allowing multiple prefab variants to share the same identifier.

The separate `WorldObjectLightController` and `WorldObjectLight` components continue to manage preset-based lighting and nighttime flickering without taking over global distance detection.