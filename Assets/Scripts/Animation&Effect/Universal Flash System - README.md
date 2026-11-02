# Universal Hit Flash System

## Overview

The Universal Hit Flash System provides visual feedback whenever a character receives damage. The character's main body temporarily changes color (red by default) to indicate that it has been hit.

The system is reusable and supports the Player, Hostile Mobs, and Passive Mobs.

## Features

- Configurable flash color through the Inspector.
- Adjustable flash duration.
- Supports Player, Hostile Mobs, and Passive Mobs.
- Affects only the root GameObject's SpriteRenderer.
- Preserves the original sprite color after flashing.
- Automatically restarts the flash when hit repeatedly.
- No Update() loop required.
- No Animator setup required.

## Scripts

### HitFlashBehavior.cs

Handles the visual hit effect.

Responsibilities:
- Gets the root SpriteRenderer.
- Stores the original sprite color.
- Changes the sprite color when damage is received.
- Restores the original color after the configured duration.
- Restarts the flash coroutine when another hit occurs.

### HealthController.cs

Handles the Player's health and triggers the hit flash through `TakeDamage()`.

### HostileMobBehavior.cs

Handles hostile mob health and triggers the hit flash through `TakeDamage()`.

### PassiveMobBehavior.cs

Handles passive mob health and triggers the hit flash through `TakeDamage()`.

## Inspector Configuration

Attach `HitFlashBehavior` to the root GameObject of each character prefab.

| Setting | Description |
|---|---|
| Flash Color | Color displayed when the character receives damage. |
| Flash Duration | How long the character remains in the flash state. |

Recommended defaults:

- Flash Color: Red
- Flash Duration: 0.15 seconds

## Implementation

Each character's damage-handling script references the `HitFlashBehavior` component.

```csharp
private HitFlashBehavior hitFlash;
```

Initialize the component in `Awake()`:

```csharp
hitFlash = GetComponent<HitFlashBehavior>();
```

Trigger the effect when damage is received:

```csharp
hitFlash?.PlayHitFlash();
```

## Unity Setup

1. Attach `HitFlashBehavior` to the Player root prefab.
2. Attach it to every Hostile Mob root prefab.
3. Attach it to every Passive Mob root prefab.
4. Configure the flash color and duration in the Inspector.
5. Ensure each damage-handling script references the component.

## Important Notes

- Only the root SpriteRenderer is affected.
- Child SpriteRenderers, weapons, shadows, and other visual elements remain unchanged.
- The original sprite color is restored after every flash.
- Repeated hits restart the flash duration.
- The system does not modify health, damage calculations, knockback, or death logic.

## Expected Result

Whenever the Player, a Hostile Mob, or a Passive Mob receives damage, its main body briefly flashes using its configured color.

The system provides consistent and reusable damage feedback throughout Project Survival.