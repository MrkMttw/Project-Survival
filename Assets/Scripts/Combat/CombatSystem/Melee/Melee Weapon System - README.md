# Melee Weapon System

## Overview

The Melee Weapon System is a reusable combat system for Project Survival, a 2D top-down survival game developed in Unity.

It allows players to equip melee weapons and attack nearby enemies and passive mobs using left-click or Spacebar.

Each melee weapon has its own configurable stats through a ScriptableObject preset, allowing different weapons to have different damage values, attack speeds, attack ranges, and knockback strengths.

The system is integrated with the centralized ClickInteractionController to prevent conflicts between weapon attacks and other player interactions.

## Features

- **Weapon Detection** – Automatically identifies equipped melee weapons.
- **Centralized Input** – Uses ClickInteractionController to handle attack input.
- **Multiple Attack Inputs** – Supports left-click and Spacebar.
- **Configurable Weapon Presets** – Each weapon has its own WeaponData asset.
- **Randomized Damage** – Damage is calculated using the weapon's minimum and maximum damage values.
- **Attack Cooldown** – Controls how frequently the player can attack.
- **Attack Range** – Determines how far the player can hit enemies.
- **Knockback** – Applies knockback to enemies when attacked.
- **Passive Mob Interaction** – Damages passive mobs and triggers their fleeing behavior.
- **Enemy Layer Filtering** – Uses LayerMask to identify valid combat targets.
- **Reusable Architecture** – Supports additional melee weapons without requiring separate combat controllers.

## System Architecture

The system consists of the following scripts:

### 1. WeaponData.cs

A ScriptableObject that stores the configuration of each melee weapon.

**Configuration:**
- Item Prefab
- Minimum Damage
- Maximum Damage
- Attack Speed
- Attack Range
- Knockback Strength

Each melee weapon should have its own WeaponData asset.

### 2. MeleeWeaponFunction.cs

A component attached to melee weapon item prefabs.

It identifies an item as a melee weapon and references its corresponding WeaponData asset.

### 3. MeleeWeaponController.cs

Handles the player's melee combat functionality.

**Responsibilities:**
- Detects whether a melee weapon is equipped.
- Retrieves the currently equipped weapon's preset.
- Checks attack cooldown.
- Detects nearby enemies using Physics2D.OverlapCircleAll.
- Calculates randomized damage.
- Applies damage and knockback to enemies.
- Damages passive mobs and triggers their attack response.
- Displays the attack range through an Inspector gizmo.

The controller does not independently listen for player input. It receives attack requests from ClickInteractionController.

### 4. ClickInteractionController.cs

Centralizes player click interactions and determines which action should be performed.

**Interaction Priority:**
1. Projectile weapon
2. Melee weapon
3. Breaking objects

When a melee weapon is equipped, the controller routes the interaction to MeleeWeaponController.

This prevents melee attacks and object-breaking interactions from being executed simultaneously.

## Setup Guide

### Step 1: Create a Melee Weapon Preset

1. Right-click inside the Project window.
2. Select Create → Combat → Weapon Data.
3. Name the asset (e.g., IronSwordData).
4. Configure its damage, attack speed, range, and knockback.

### Step 2: Configure the Weapon Item

1. Select the melee weapon item prefab.
2. Add MeleeWeaponFunction.
3. Assign the corresponding WeaponData asset.

### Step 3: Configure MeleeWeaponController

Assign the following references in the Inspector:

| Field | Assignment |
|---|---|
| Player | Player Transform |
| Player Held Item | Player's PlayerHeldItem |
| Enemy Layer | Layer containing valid combat targets |

Make sure the Enemy Layer includes the layers used by both enemies and passive mobs.

### Step 4: Configure ClickInteractionController

Assign the MeleeWeaponController reference.

The controller should also have its existing ProjectileWeaponController and BreakSystem references assigned.

## Weapon Configuration

| Setting | Description |
|---|---|
| Min Damage | Minimum randomized damage |
| Max Damage | Maximum randomized damage |
| Attack Speed | Number of attacks allowed per second |
| Attack Range | Maximum distance for detecting targets |
| Knockback Strength | Force applied when hitting enemies |

## Combat Integration

The melee system reuses the existing combat controllers.

### EnemyController

Uses:

`TakeDamage(damage, knockbackDirection, knockbackStrength)`

The enemy controller handles:
- Defense calculations.
- Health reduction.
- Knockback resistance.
- Enemy death.
- Item drops.

### PassiveMobController

Uses:

- `TakeDamage(damage)`
- `ReactToAttack(attacker)`

Passive mobs receive damage and begin fleeing after being attacked.

Unlike enemies, passive mobs do not currently receive weapon knockback through the melee controller.

## Attack Mechanics

When the player attacks:

1. The controller checks the currently equipped item.
2. It retrieves the corresponding WeaponData.
3. It checks whether the attack cooldown has elapsed.
4. It searches for valid targets within the weapon's attack range.
5. It calculates randomized damage.
6. It applies damage and knockback to enemies.
7. It applies damage and fleeing behavior to passive mobs.
8. If a valid target was hit, the attack cooldown begins.

The cooldown is calculated using:

`Attack Cooldown = 1 / Attack Speed`

The attack cooldown is only applied when a valid target is hit.

## Adding New Melee Weapons

Additional melee weapons can reuse the existing system.

Examples:

| Weapon | Possible Configuration |
|---|---|
| Wooden Sword | Low damage and knockback |
| Iron Sword | Moderate damage and knockback |
| Axe | Higher damage |
| Dagger | High attack speed and short range |
| Heavy Hammer | High damage and knockback, slower attacks |

To add another weapon:

1. Create a new WeaponData asset.
2. Assign the weapon item prefab.
3. Configure its combat statistics.
4. Add MeleeWeaponFunction to the weapon item prefab.
5. Assign the new WeaponData asset.

No additional melee controller is necessary.

## Important Notes

- Melee weapons support left-click and Spacebar through ClickInteractionController.
- The system uses the New Input System through the centralized input controller.
- Only colliders included in the assigned Enemy Layer are detected.
- Attack range is measured from the player's position.
- Damage is randomized between the configured minimum and maximum values.
- Attack cooldown begins only after successfully hitting a target.
- The system does not currently include directional attacks or weapon swing animations.
- Melee and projectile weapons share the same centralized interaction architecture.

## Future Improvements

Potential extensions include:

- Directional melee attacks.
- Weapon swing animations.
- Attack hitboxes.
- Weapon-specific attack animations.
- Critical hit mechanics.
- Charged attacks.
- Combo attacks.
- Weapon-specific sound effects.
- Different attack patterns.

## Status

**Implemented and tested.**

The core melee weapon system is functional and supports configurable melee weapons through reusable ScriptableObject presets.