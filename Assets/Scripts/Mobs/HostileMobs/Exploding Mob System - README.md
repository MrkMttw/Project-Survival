# Exploding Mob System

## Overview

The Exploding Mob System allows a hostile mob to detect and approach the player before triggering a timed explosion. Once its fuse reaches zero, it damages nearby targets, applies knockback, plays a detached smoke effect, and disappears.

## Features

- **Player Detection** – Detects the player within a configurable detection range.
- **Chasing Behavior** – Moves toward the player while maintaining a configurable distance.
- **Obstacle Avoidance** – Attempts to avoid obstacles while chasing.
- **Explosion Trigger** – Starts the fuse when the player enters the explosion trigger range.
- **Fuse Countdown** – Displays the remaining fuse time using TextMeshPro.
- **Explosion Damage** – Damages targets within the configured explosion radius.
- **Explosion Knockback** – Pushes affected targets away from the explosion.
- **Smoke Effect** – Plays a detached smoke effect when the mob detonates.
- **Mob Drops** – Uses the configured mob drop preset when destroyed.
- **Boss Defeat Registration** – Supports structure boss defeat registration when applicable.

## Unity Setup

### 1. Required Components

Attach the following components to the exploding mob prefab:

- `ExplodingMobBehavior`
- `Rigidbody2D`
- `Collider2D`
- `HitFlashBehavior` (if using the hit flash system)
- `SmokeEffectAnimator` (for the explosion smoke effect)

### 2. Fuse Countdown UI

1. Create a TextMeshPro UI text object.
2. Position it above the mob.
3. Assign the text object to the `Fuse Timer Text` field in `ExplodingMobBehavior`.
4. The countdown will automatically appear when the fuse starts and disappear after detonation.

### 3. Player References

The script automatically searches for the player and retrieves the required player components.

Make sure the player has the appropriate components and tag configured.

### 4. Smoke Effect

1. Add a smoke effect object to the mob prefab.
2. Configure its Animator and smoke trigger in `SmokeEffectAnimator`.
3. Assign the `SmokeEffectAnimator` reference to `ExplodingMobBehavior`.

The smoke effect is detached before the mob is destroyed so that it can finish playing independently.

## Inspector Configuration

### Enemy Stats
- `Max HP` – Maximum health of the mob.
- `Defense` – Damage reduction.
- `Mob Drop Preset` – Items dropped when the mob is destroyed.
- `HP Bar` – Health bar UI reference.

### Detection and Movement
- `Detection Range` – Distance at which the mob detects the player.
- `Chase Range` – Maximum distance for chasing behavior.
- `Movement Speed` – Mob movement speed.
- `Maintain Distance` – Preferred distance to maintain from the player.
- `Obstacle Layer` – Layers considered obstacles.
- `Obstacle Check Distance` – Distance used for obstacle detection.
- `Avoidance Duration` – Duration of obstacle avoidance movement.

### Explosion Settings
- `Explosion Trigger Range` – Distance at which the fuse starts.
- `Fuse Duration` – Time before detonation.
- `Explosion Radius` – Area affected by the explosion.
- `Explosion Damage` – Damage dealt to nearby targets.
- `Explosion Knockback Strength` – Force applied to affected targets.
- `Explosion Knockback Duration` – Duration of the explosion knockback.

### Knockback Settings
- `Knockback Resistance` – Resistance against incoming knockback.
- `Knockback Duration` – Duration of regular knockback.
- `Knockback Damping` – Controls how quickly knockback movement slows down.

## Detonation Sequence

1. The mob detects the player.
2. The mob approaches the player.
3. When the player enters the explosion trigger range, the fuse starts.
4. The countdown UI displays the remaining fuse time.
5. When the fuse reaches zero, the explosion is triggered.
6. Nearby targets receive explosion damage and knockback.
7. The smoke effect plays independently.
8. The mob drops its configured items and is destroyed.

## Notes

- The explosion does not wait for an attack or explosion animation.
- The mob is destroyed immediately after applying explosion effects.
- The detached smoke effect continues playing even after the mob is destroyed.
- `MobAttackAnimator` is not required by this system.
- Ensure the explosion radius and damage values are balanced with the rest of the game's combat system.