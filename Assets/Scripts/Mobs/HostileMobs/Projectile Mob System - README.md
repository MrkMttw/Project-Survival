# Projectile Mob System

## Overview

The Projectile Mob System introduces a ranged hostile mob that detects and follows the player, maintains a preferred distance, and attacks by shooting projectiles.

The system consists of two scripts:

- **ProjectileMobBehavior.cs** – Controls the ranged mob's detection, movement, combat, health, knockback, and death.
- **MobProjectile.cs** – Controls projectile movement, collision detection, player damage, and knockback.

The attack animation is visual only. Projectile spawning is controlled through a configurable timer rather than animation events.

---

## 1. ProjectileMobBehavior.cs

Controls the ranged enemy's overall behavior.

### Features

- Player detection using the `Player` tag.
- Separate detection and chase ranges.
- Chases the player until it reaches its preferred distance.
- Stops moving when the player is within its preferred distance.
- Does not retreat when the player gets too close.
- Performs ranged attacks when the player is within attack range and preferred distance.
- Supports configurable first attack delay and attack cooldown.
- Plays an attack animation independently of projectile spawning.
- Spawns projectiles after a configurable delay.
- Supports obstacle detection and directional avoidance.
- Supports enemy health, defense, and health bar updates.
- Supports mob knockback and knockback resistance.
- Supports smoke effects when the mob dies.
- Supports item drops through `MobDropData`.
- Registers boss defeats through `StructureObjectIdentity`.

### Inspector Settings

#### Enemy Stats
| Variable | Description |
|---|---|
| Max HP | Maximum health of the mob. |
| Defense | Reduces incoming damage. |

#### Detection
| Variable | Description |
|---|---|
| Detection Range | Distance required to detect the player. |
| Chase Range | Maximum distance before the mob stops detecting and chasing the player. |

#### Chase Movement
| Variable | Description |
|---|---|
| Movement Speed | Mob movement speed. |
| Obstacle Layer | Layers that the mob recognizes as obstacles. |
| Obstacle Check Distance | Distance used for obstacle detection. |
| Avoidance Duration | How long the mob follows its selected avoidance direction. |

#### Maintain Distance
| Variable | Description |
|---|---|
| Preferred Distance | Distance the mob attempts to maintain from the player. |

#### Ranged Combat
| Variable | Description |
|---|---|
| Attack Range | Maximum distance allowed to begin an attack. |
| Attack Cooldown | Time between completed attacks. |
| First Attack Delay | Initial delay before the first attack after detecting the player. |
| Projectile Spawn Delay | Time between starting the attack animation and spawning the projectile. |
| Attack Animation Duration | Duration of the mob's attack sequence. |

#### Projectile
| Variable | Description |
|---|---|
| Projectile Prefab | Projectile GameObject to instantiate. |
| Attack Point | Transform where the projectile spawns. |
| Projectile Speed | Projectile movement speed. |
| Projectile Max Distance | Maximum distance the projectile can travel. |
| Projectile Max Lifetime | Maximum time the projectile can exist. |
| Projectile Damage | Damage dealt to the player. |
| Projectile Knockback Strength | Knockback force applied to the player. |
| Projectile Knockback Duration | Duration of the player's knockback. |
| Projectile Rotation Offset | Adjusts the projectile sprite's rotation. |

#### Mob Knockback
| Variable | Description |
|---|---|
| Knockback Resistance | Reduces incoming knockback strength. |
| Knockback Duration | Duration of the mob's knockback. |
| Knockback Damping | Controls how quickly knockback velocity decreases. |

#### Animation and Effects
| Variable | Description |
|---|---|
| Mob Attack Animator | Reference to the mob's attack animation controller. |
| Smoke Effect Animator | Reference to the mob's smoke effect controller. |

---

## 2. MobProjectile.cs

Controls the projectile after it has been instantiated by `ProjectileMobBehavior`.

### Features

- Moves toward the player's position when fired.
- Uses configurable projectile speed.
- Automatically destroys itself after reaching its maximum distance.
- Automatically destroys itself after reaching its maximum lifetime.
- Ignores collisions with the mob that fired it.
- Detects collisions with the player.
- Applies damage through `HealthController`.
- Applies knockback through `PlayerMovement`.
- Disables its collider after hitting a target.
- Delays projectile destruction after a collision.

### Inspector Settings

#### Projectile Settings
| Variable | Description |
|---|---|
| Speed | Projectile movement speed. |
| Max Distance | Maximum travel distance. |
| Max Lifetime | Maximum projectile lifetime. |

#### Damage
| Variable | Description |
|---|---|
| Damage | Damage applied to the player. |
| Knockback Strength | Force applied to the player upon impact. |
| Knockback Duration | Duration of the knockback effect. |

#### Hit Settings
| Variable | Description |
|---|---|
| Hit Destroy Delay | Time before the projectile is destroyed after a collision. |

---

## 3. Unity Setup

### A. Projectile Mob

1. Create a GameObject for the ranged mob.
2. Add `ProjectileMobBehavior.cs`.
3. Add a `Rigidbody2D`.
4. Configure the Rigidbody2D and enemy colliders.
5. Assign the mob's health bar.
6. Assign the `MobAttackAnimator`.
7. Assign the `SmokeEffectAnimator`.
8. Configure the detection, movement, and combat settings.

### B. Attack Point

1. Create an empty GameObject as a child of the mob.
2. Name it `AttackPoint`.
3. Position it where the projectile should spawn.
4. Assign it to the `Attack Point` field.

### C. Projectile Prefab

1. Create a projectile GameObject.
2. Add a Sprite Renderer.
3. Add a Collider2D.
4. Enable `Is Trigger` on the projectile collider.
5. Add `MobProjectile.cs`.
6. Adjust the projectile's visual appearance and collider size.
7. Save the GameObject as a prefab.
8. Assign the prefab to the mob's `Projectile Prefab` field.

Ensure the projectile collision setup supports Unity's 2D trigger callbacks. At least one of the interacting objects must have a Rigidbody2D.

### D. Player Setup

The projectile uses the `Player` tag to locate the player.

Recommended hierarchy:

```text
Player (Tag: Player)
├── HeldItem
├── PlayerHitbox
├── Hunger
├── Health (HealthController)
├── BreakSystem
├── Shadow
└── Smoke
```

Important:

- Keep the `Player` tag on the root Player GameObject.
- Child objects should normally use `Untagged`.
- `HealthController` must be attached to the Player or one of its children.
- `PlayerMovement` must also be attached to the Player or one of its children.

The projectile searches the Player hierarchy for both components.

---

## 4. Attack Sequence

The ranged mob follows this attack sequence:

1. Detects the player within its detection range.
2. Chases the player if they are farther away than its preferred distance.
3. Stops moving once it reaches its preferred distance.
4. Checks whether the player is within attack range.
5. Waits for the first attack delay or remaining cooldown.
6. Plays the attack animation.
7. Waits for the configured projectile spawn delay.
8. Instantiates and initializes the projectile.
9. Finishes the attack sequence after the configured animation duration.
10. Starts the attack cooldown before the next attack.

The animation does not control projectile spawning. Both are managed independently through timers.

---

## 5. Projectile Collision Sequence

When the projectile collides with another object:

1. Checks whether the collision belongs to the mob that fired it.
2. Searches for the Player GameObject using the `Player` tag.
3. Checks whether the collided object belongs to the Player hierarchy.
4. Locates `HealthController` and `PlayerMovement`.
5. Disables the projectile collider.
6. Applies damage to the player.
7. Applies knockback to the player.
8. Destroys the projectile after `Hit Destroy Delay`.

If the projectile hits a non-player collider, it stops moving and is destroyed after the configured delay.

---

## 6. Mob Death

When the ranged mob's health reaches zero:

1. Marks the mob as dead.
2. Stops its movement.
3. Plays its detached smoke effect.
4. Checks whether it is registered as a boss.
5. Registers the boss defeat if applicable.
6. Drops items according to its `MobDropData`.
7. Destroys the mob GameObject.
8. Destroys the detached smoke effect after its configured duration.

---

## 7. Important Notes

- The projectile prefab must have `MobProjectile.cs` attached.
- The projectile prefab must have a Collider2D.
- The projectile collider should normally have `Is Trigger` enabled.
- The Player root must have the `Player` tag.
- The Player must contain `HealthController` and `PlayerMovement` somewhere in its hierarchy.
- The mob requires a `Rigidbody2D`.
- Assign the attack point and projectile prefab in the Inspector.
- The attack animation does not require animation events for projectile spawning.
- The projectile ignores the colliders belonging to its shooter.
- Projectiles are destroyed automatically after reaching their maximum distance or lifetime.

---

## Scripts Used

- `ProjectileMobBehavior.cs`
- `MobProjectile.cs`
- `HealthController.cs`
- `PlayerMovement.cs`
- `MobAttackAnimator.cs`
- `SmokeEffectAnimator.cs`
- `HitFlashBehavior.cs`
- `MobDropData.cs`
- `LootDropAnimation.cs`
- `StructureObjectIdentity.cs`
- `StructureSpawner.cs`

**System Status:** Implemented and functional.

**System Type:** Ranged Hostile Mob / Projectile Combat System