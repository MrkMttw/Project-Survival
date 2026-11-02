# Player Animation System

## Overview

The Player Animation System handles the player's directional idle and walking animations, held-item positioning, and animation speed adjustments when sprinting.

It follows the same animation logic as the NPC Animation System to maintain consistency across characters in Project Survival.

## Scripts

### 1. PlayerAnimationBehavior.cs

Responsible for controlling the player's animations.

**Responsibilities:**
- Detects player movement using Rigidbody2D position changes.
- Determines the player's facing direction.
- Controls idle and walking animation states.
- Preserves the last facing direction when the player stops moving.
- Adjusts animation speed when sprinting.

**Animator Parameters:**

| Parameter | Type | Description |
|---|---|---|
| IsMoving | Bool | Determines whether the player is moving. |
| MoveX | Float | Stores the horizontal facing direction. |
| MoveY | Float | Stores the vertical facing direction. |

**Animation States:**

| Idle Animations | Walking Animations |
|---|---|
| PlayerIdleDown | PlayerWalkDown |
| PlayerIdleUp | PlayerWalkUp |
| PlayerIdleLeft | PlayerWalkLeft |
| PlayerIdleRight | PlayerWalkRight |

**Directional Movement:**

Horizontal movement takes priority over vertical movement when determining the facing direction.

| Input | Facing Direction |
|---|---|
| W | Up |
| A | Left |
| S | Down |
| D | Right |
| W + A | Left |
| W + D | Right |
| S + A | Left |
| S + D | Right |

**Animation Speed:**

| State | Animation Speed |
|---|---|
| Walking | 1x |
| Sprinting | 1.6x |

Animation speeds can be adjusted through the Unity Inspector.

### 2. PlayerMovement.cs

Responsible for player movement and sprint mechanics.

**Responsibilities:**
- Handles movement through the Unity Input System.
- Controls player movement using Rigidbody2D.
- Implements sprinting using Left Shift.
- Manages sprint stamina and regeneration.
- Updates the sprint UI bar.
- Communicates sprinting status to PlayerAnimationBehavior.

**Default Settings:**

| Setting | Value |
|---|---|
| Move Speed | 5 |
| Sprint Multiplier | 2 |
| Sprint Duration | 3 seconds |
| Sprint Regeneration | 0.3 per second |

### 3. PlayerHeldItem.cs

Responsible for displaying and animating the currently held item.

**Responsibilities:**
- Displays the currently equipped item.
- Adjusts held-item positioning based on facing direction.
- Hides the held item when facing upward.
- Flips the held-item sprite for directional adjustments.
- Plays swing animations.
- Restores the original item rotation after swinging.

**Directional Adjustments:**

| Facing Direction | Behavior |
|---|---|
| Down | Applies down offset. |
| Up | Hides the held item. |
| Left | Applies left offset and adjusts sprite flipping. |
| Right | Applies right offset and adjusts sprite flipping. |

**Swing Animation Settings:**

| Setting | Default Value |
|---|---|
| Swing Angle | 70 degrees |
| Swing Duration | 0.12 seconds |
| Return Duration | 0.10 seconds |

## Unity Setup

Attach the following components to the Player GameObject:

- Rigidbody2D
- Animator
- PlayerMovement
- PlayerAnimationBehavior

Ensure that the Animator Controller contains all eight directional animation states and the required Animator parameters.

The PlayerHeldItem component should be configured with its corresponding held-item visual and SpriteRenderer.

## System Integration

The animation system is separated from the movement system.

- PlayerMovement handles movement and sprint mechanics.
- PlayerAnimationBehavior handles directional animations and animation speed.
- PlayerHeldItem handles equipped item visuals and swing animations.

This separation keeps the scripts organized and makes future animation updates easier to maintain.

## Notes

- Player and NPC animation systems follow the same movement detection approach.
- Animation direction is determined by Rigidbody2D position changes.
- The last facing direction is preserved when the player becomes idle.
- Sprinting increases animation playback speed without changing the animation states.
- Held-item direction reads MoveX and MoveY from the Animator.