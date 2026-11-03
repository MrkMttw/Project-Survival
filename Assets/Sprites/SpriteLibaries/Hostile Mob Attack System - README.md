# Hostile Mob Attack System

## Overview

The Hostile Mob Attack System allows hostile mobs to detect, chase, and attack the player using an Animator Controller.

The system consists of two main scripts:

- `HostileMobBehavior.cs` – Handles enemy AI, detection, chasing, attack timing, damage, health, knockback, death, and item drops.
- `MobAttackAnimator.cs` – Handles attack animation playback, attack state tracking, and animation completion events.

The attack system uses a configurable damage delay, allowing damage to be applied independently of Animation Events.

## 1. GameObject Setup

Attach the scripts to your hostile mob.

### Recommended Hierarchy

```text
HostileMob
├── Sprite
├── Animator
├── Rigidbody2D
├── Collider2D
├── HostileMobBehavior
├── MobAttackAnimator
├── HitFlashBehavior
└── HealthBar
```

**Important:**

- `HostileMobBehavior` and `MobAttackAnimator` should be attached to the same GameObject as the Animator for this setup.
- The Animator component must have a valid Animator Controller.
- The mob must have a Rigidbody2D.
- The player must have the `Player` tag.
- The player must have a `HealthController` component.

`HostileMobBehavior` searches for `MobAttackAnimator` using `GetComponentInChildren`, so the component can also be placed on a child object.

## 2. Animator Controller Setup

Create an Animator Controller for your hostile mob.

Example:

```text
Idle
  |
  | Any State
  | Trigger: Attack
  v
Attack
  |
  | Has Exit Time
  v
Idle
```

### Step 1: Create the Attack Parameter

1. Open the Animator window.
2. Select the Parameters tab.
3. Click the `+` button.
4. Select Trigger.
5. Name the parameter:

```text
Attack
```

The parameter name must match the `attackTrigger` field in `MobAttackAnimator`.

### Step 2: Configure Any State → Attack

Create a transition from Any State to Attack.

Configure the transition:

| Setting | Value |
|---|---|
| Has Exit Time | Disabled |
| Condition | Attack |
| Transition Duration | 0 or a small value |

This allows the mob to enter its attack animation whenever its AI requests an attack.

### Step 3: Configure Attack → Idle

Create a transition from Attack to Idle.

Configure:

| Setting | Value |
|---|---|
| Has Exit Time | Enabled |
| Exit Time | 1 |
| Condition | None |
| Transition Duration | Small value |

The Attack animation should be allowed to finish naturally.

If your mob uses directional animations, create appropriate transitions back to its idle or movement states.

## 3. Attack Animation Setup

Import or create your attack animation clip.

Assign the animation clip to the Attack state.

Make sure:

- The animation plays correctly.
- The attack animation has the correct duration.
- The animation is not interrupted by unnecessary transitions.
- The Attack state can return to the appropriate idle or movement state.

### Animation Events

The system supports two Animation Events:

| Method | Purpose |
|---|---|
| `OnAttackHit()` | Optional hit-frame event. |
| `OnAttackFinished()` | Signals that the attack animation has finished. |

#### OnAttackHit()

This method is no longer responsible for applying player damage.

Damage is handled through the configurable `attackDamageDelay` field in `HostileMobBehavior`.

The hit event can remain in the animation clip if needed for future effects or synchronization.

#### OnAttackFinished()

This event is important.

Add an Animation Event near the end of the Attack animation.

Configure its Function:

```text
OnAttackFinished
```

This notifies `MobAttackAnimator` that the animation has finished.

It then invokes the `AttackFinished` event, allowing `HostileMobBehavior` to reset its attack state and begin the cooldown.

**Important:** Place the event slightly before the final frame rather than directly on the last frame.

## 4. Attack Failsafe

`MobAttackAnimator` includes a failsafe to prevent the mob from becoming permanently stuck in its attack state.

Inspector field:

```text
Max Attack Duration = 1.5
```

If `OnAttackFinished()` is not received within the configured duration, the script automatically resets its internal attack state and invokes `AttackFinished`.

This prevents missed Animation Events from permanently blocking subsequent attacks.

Set `maxAttackDuration` slightly longer than the actual attack animation duration.

For example:

| Attack Animation Duration | Suggested Failsafe |
|---|---|
| 0.8 seconds | 1.1 seconds |
| 1.0 seconds | 1.3 seconds |
| 1.2 seconds | 1.5 seconds |
| 1.5 seconds | 1.8 seconds |

The failsafe resets the attack state but does not apply damage.

## 5. HostileMobBehavior Inspector Settings

### Enemy Stats

| Field | Description |
|---|---|
| Max HP | Maximum mob health. |
| Base Damage | Normal attack damage. |
| Attack Damage Percent | Bonus critical damage based on player maximum health. |
| Crit Chance | Probability of a critical hit. |
| Movement Speed | Mob movement speed. |
| Defense | Damage reduction against incoming attacks. |

### Detection and Chase

| Field | Description |
|---|---|
| Detection Range | Distance at which the mob detects the player. |
| Chase Range | Maximum distance before the mob stops chasing. |
| Stopping Distance | Distance at which the mob stops moving toward the player. |

### Combat

| Field | Description |
|---|---|
| Attack Range | Maximum distance required to attack the player. |
| Attack Damage Delay | Time between starting the attack animation and applying damage. |
| Attack Cooldown | Delay before another attack can begin. |
| First Attack Delay | Initial delay before the first attack. |

### Attack Damage Delay

This field allows manual synchronization between the attack animation and damage application.

Example:

```text
Attack Animation Starts
        |
        | 0.3 seconds
        v
Player Receives Damage
```

Adjust this value depending on when the attack visually connects.

Setting it to `0` applies damage immediately.

### Knockback

| Field | Description |
|---|---|
| Knockback Resistance | Reduces incoming knockback strength. |
| Knockback Duration | Duration of the knockback effect. |
| Knockback Damping | Controls how quickly knockback slows down. |

### AI Movement

| Field | Description |
|---|---|
| Obstacle Layer | Layers containing obstacles. |
| Obstacle Check Distance | Distance used to detect obstacles. |
| Avoidance Duration | How long the mob maintains its avoidance direction. |

## 6. MobAttackAnimator Inspector Settings

| Field | Description |
|---|---|
| Mob Animator | Animator component responsible for playing animations. |
| Attack Trigger | Animator Trigger parameter name. |
| Max Attack Duration | Maximum time before the attack failsafe resets the attack state. |

If the Animator is attached to the same GameObject, the script automatically finds it using `GetComponent<Animator>()`.

## 7. Attack Execution Flow

The attack system follows this sequence:

```text
Player Enters Detection Range
          |
          v
Mob Starts Chasing
          |
          v
Player Enters Attack Range
          |
          v
First Attack Delay
          |
          v
PlayAttack()
          |
          v
Attack Animation Starts
          |
          v
Wait For Attack Damage Delay
          |
          v
ApplyAttackDamage()
          |
          v
Attack Animation Finishes
          |
          v
OnAttackFinished()
          |
          v
Reset Attack State
          |
          v
Start Attack Cooldown
          |
          v
Mob Can Attack Again
```

If the animation completion event is missed, the failsafe automatically resets the attack state.

## 8. Troubleshooting

### Mob attacks only once

Possible causes:

- `OnAttackFinished()` is not being called.
- The attack animation is interrupted.
- The failsafe duration is configured incorrectly.
- The Animator cannot return to its idle or movement state.

Check the Console for:

```text
[MobAttackAnimator] Attack finish event missed.
```

This indicates that the failsafe had to reset the attack state.

### Mob plays attack animation but deals no damage

Check:

- `attackDamageDelay` is configured correctly.
- The player has the `Player` tag.
- The player has a `HealthController`.
- The player remains within `attackRange` when damage is applied.
- The player's `TakeDamage()` method is working.

### Mob does not play its attack animation

Check:

- The Animator Controller is assigned.
- The Trigger parameter is named exactly `Attack`.
- The transition from Any State to Attack has the correct condition.
- The `MobAttackAnimator` component is assigned or attached correctly.

### Mob does not attack repeatedly

Check:

- `OnAttackFinished()` is configured as an Animation Event.
- `maxAttackDuration` is longer than the attack animation.
- The attack cooldown is not excessively long.

## 9. Important Notes

- Damage application does not depend on the `OnAttackHit()` Animation Event.
- `OnAttackFinished()` is used to reset the attack state and start the cooldown.
- The failsafe provides an additional recovery mechanism if the finish event is missed.
- Damage is only applied if the player is still within attack range when the delayed damage executes.
- Keep the existing AI, movement, obstacle avoidance, knockback, health, death, and item drop systems intact.

## 10. Required Components Checklist

Before testing a hostile mob, verify the following:

- [ ] Rigidbody2D attached.
- [ ] Collider2D configured.
- [ ] Animator attached.
- [ ] Animator Controller assigned.
- [ ] Attack Trigger parameter created.
- [ ] Any State → Attack transition configured.
- [ ] Attack → Idle transition configured.
- [ ] MobAttackAnimator attached.
- [ ] HostileMobBehavior attached.
- [ ] Attack damage delay configured.
- [ ] Attack cooldown configured.
- [ ] Max attack duration configured.
- [ ] Player tagged as Player.
- [ ] Player has HealthController.
- [ ] OnAttackFinished Animation Event configured.