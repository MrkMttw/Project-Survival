# Projectile Weapon System

## Overview

The Projectile Weapon System is a reusable weapon system for Project Survival, a 2D top-down survival game developed in Unity.

It allows players to equip projectile-based weapons, aim toward the mouse cursor, and fire projectiles using a single click. Each weapon can have its own configurable stats, projectile prefab, and ammunition type.

The system is designed to support multiple projectile weapons, including bows, crossbows, slingshots, guns, and magic staffs, without requiring separate shooting logic for each weapon.

## Features

- **Mouse Aiming** – Projectiles travel toward the mouse cursor.
- **Single-Click Shooting** – Each left-click attempts one shot.
- **Configurable Weapon Presets** – Each weapon has its own ScriptableObject configuration.
- **Custom Ammunition** – Each weapon can require a specific ammunition item.
- **Ammunition Consumption** – Checks the hotbar first, then the inventory.
- **Projectile Damage** – Configurable minimum and maximum damage.
- **Knockback** – Projectiles can apply knockback to enemies and passive mobs.
- **Attack Cooldown** – Configurable attacks per second.
- **Maximum Projectile Distance** – Projectiles are destroyed after reaching their maximum travel distance.
- **Maximum Projectile Lifetime** – Projectiles are destroyed after their configured lifetime.
- **Collision Handling** – Projectiles interact with enemies, passive mobs, and other colliders.
- **Shooter Collision Prevention** – Projectiles ignore the firing player's colliders.
- **Centralized Input** – Uses ClickInteractionController to manage weapon input and interaction priority.
- **Reusable Architecture** – Supports additional projectile weapons through configurable presets.

## System Architecture

The system consists of the following scripts:

### 1. ProjectileWeaponData.cs

A ScriptableObject that stores the configuration of each projectile weapon.

**Configuration:**
- Item Prefab
- Projectile Prefab
- Ammunition Item
- Minimum Damage
- Maximum Damage
- Attack Speed
- Projectile Speed
- Maximum Distance
- Maximum Lifetime
- Knockback Strength

Each projectile weapon should have its own ProjectileWeaponData asset.

### 2. ProjectileWeaponFunction.cs

A component attached to projectile weapon item prefabs.

It identifies an item as a projectile weapon and references its corresponding ProjectileWeaponData asset.

### 3. ProjectileWeaponController.cs

Handles the player's projectile weapon functionality.

**Responsibilities:**
- Detects the currently equipped projectile weapon.
- Retrieves its weapon preset.
- Calculates the direction toward the mouse cursor.
- Spawns projectiles slightly above the player's position.
- Calculates randomized projectile damage.
- Checks attack cooldown.
- Consumes the required ammunition.
- Initializes the spawned projectile.

Ammunition is consumed from the hotbar first. If unavailable, the controller checks the inventory.

### 4. ProjectileBehavior.cs

Handles the behavior of individual projectiles.

**Responsibilities:**
- Moves the projectile in its firing direction.
- Tracks projectile travel distance.
- Tracks projectile lifetime.
- Detects collisions.
- Applies damage and knockback.
- Destroys the projectile after hitting a target or obstacle.
- Ignores the shooter's colliders.

### 5. ClickInteractionController.cs

Centralizes player click interactions.

**Interaction Priority:**
1. Projectile weapon
2. Melee weapon
3. Breaking objects

Projectile weapons respond to left-click, while melee weapons can respond to left-click or Spacebar.

An equipped weapon claims the interaction even when its cooldown is active or ammunition is unavailable, preventing accidental object breaking.

## Setup Guide

### Step 1: Create a Projectile Weapon Preset

1. Right-click inside the Project window.
2. Select Create → Combat → Projectile Weapon Data.
3. Name the asset (e.g., BowData).
4. Configure its weapon and projectile settings.
5. Assign the projectile prefab and required ammunition item.

### Step 2: Configure the Weapon Item

1. Select the weapon item prefab.
2. Add ProjectileWeaponFunction.
3. Assign the corresponding ProjectileWeaponData asset.

### Step 3: Configure the Projectile Prefab

1. Create the projectile GameObject.
2. Add a Sprite Renderer.
3. Add a Collider2D.
4. Enable Is Trigger.
5. Add ProjectileBehavior.
6. Assign the projectile prefab to the weapon preset.

Ensure the projectile has an appropriate collider size.

### Step 4: Configure ProjectileWeaponController

Assign the following references in the Inspector:

- Player
- PlayerHeldItem
- HotbarController

### Step 5: Configure ClickInteractionController

Assign the ProjectileWeaponController reference alongside the existing MeleeWeaponController and BreakSystem references.

## Projectile Configuration

| Setting | Description |
|---|---|
| Min Damage | Minimum randomized projectile damage |
| Max Damage | Maximum randomized projectile damage |
| Attack Speed | Number of attacks allowed per second |
| Projectile Speed | Projectile movement speed |
| Max Distance | Maximum projectile travel distance |
| Max Lifetime | Maximum projectile lifetime |
| Knockback Strength | Force applied when hitting a target |

The projectile spawn position currently uses a small vertical offset above the player's position.

## Damage Integration

The projectile system reuses existing combat controllers.

**EnemyController**
- Uses TakeDamage(damage, knockbackDirection, knockbackStrength).
- Applies defense and knockback resistance.
- Handles health reduction and enemy death.

**PassiveMobController**
- Uses TakeDamage(damage).
- Uses ApplyKnockback(direction, strength).
- Uses ReactToAttack(attacker) to trigger fleeing behavior.

No separate health or damage system is required for projectiles.

## Ammunition System

The weapon preset determines which ammunition item is required.

The ammunition consumption order is:

1. Search the hotbar for the required ammunition.
2. Consume one item if available.
3. If unavailable, search the inventory.
4. Consume one item if available.
5. If no ammunition is available, cancel the shot.

The equipped weapon remains equipped when ammunition runs out.

## Adding New Projectile Weapons

New projectile weapons can reuse the existing system.

For example:

| Weapon | Projectile | Ammunition |
|---|---|---|
| Bow | Arrow | Arrow |
| Crossbow | Bolt | Bolt |
| Slingshot | Stone | Stone |
| Magic Staff | Magic Orb | Magic Orb |

To add another weapon:

1. Create a new ProjectileWeaponData asset.
2. Assign the weapon item prefab.
3. Assign its projectile prefab.
4. Assign its required ammunition item.
5. Configure its damage, attack speed, projectile speed, range, lifetime, and knockback.
6. Add ProjectileWeaponFunction to the weapon item prefab.
7. Assign the new preset.

No additional weapon controller is necessary.

## Important Notes

- Projectile weapons use left-click for shooting.
- Projectiles spawn from the player's position with a small upward offset.
- The projectile ignores the firing player's colliders.
- Projectile damage is randomized between the configured minimum and maximum values.
- Projectiles are destroyed upon hitting a supported target or another collider.
- Projectiles are destroyed when either their maximum distance or maximum lifetime is reached.
- Ammunition is consumed only when a shot is successfully initiated.
- The system uses Unity's 2D physics and the New Input System.

## Future Improvements

Potential extensions include:

- Dedicated projectile spawn points.
- Weapon-specific firing animations.
- Reloading mechanics.
- Projectile penetration.
- Explosive projectiles.
- Piercing and bouncing projectiles.
- Projectile effects and impact animations.
- Weapon-specific firing sounds.
- Charged attacks.

## Status

**Implemented and tested.**

The core projectile weapon system is functional and ready for additional projectile-based weapons.