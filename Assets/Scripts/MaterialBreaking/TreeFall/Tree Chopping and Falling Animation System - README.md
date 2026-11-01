# Tree Chopping and Falling Animation System

## Overview

The Tree Chopping and Falling Animation System adds a falling animation when a tree's health reaches zero. Instead of immediately destroying the tree, its visual element rotates toward the ground before loot is dropped and the GameObject is destroyed.

The system is integrated with the existing `BreakSystem` and uses a dedicated animation script attached to individual tree prefabs.

## Scripts

### 1. TreeFallAnimation.cs

Attached to the tree prefab.

**Responsibilities:**
- Handles the tree falling animation.
- Determines the falling direction based on the player's position.
- Rotates only the assigned tree visual element.
- Keeps the shadow stationary during the animation.
- Prevents the animation from playing multiple times.

**Inspector Fields:**

| Field | Description |
|---|---|
| Tree Element | Transform of the tree visuals to rotate. |
| Shadow Element | Shadow GameObject that stays stationary. |
| Fall Duration | Duration of the falling animation. |
| Fall Angle | Maximum rotation angle. |
| Fall Curve | Controls the animation's easing. |

### 2. BreakableMaterial.cs

Handles the health and damage of breakable objects.

**Responsibilities:**
- Tracks current and maximum health.
- Receives damage from BreakSystem.
- Detects when health reaches zero.
- Prevents additional damage after breaking.
- Returns `true` when the object is completely broken.

**Important:** The script no longer destroys the GameObject immediately. Destruction is handled by BreakSystem after the appropriate animation.

### 3. BreakSystem.cs

Handles the breaking process and coordinates the destruction sequence.

**Responsibilities:**
- Detects breakable objects.
- Checks equipped tools and material compatibility.
- Applies damage and tool durability consumption.
- Triggers the tree falling animation when applicable.
- Marks persistent world objects as destroyed.
- Drops loot after the animation.
- Destroys the tree and its shadow.

## Tree Prefab Setup

The system supports keeping the existing prefab hierarchy unchanged.

Assign the following references in the TreeFallAnimation Inspector:

- **Tree Element:** The visual element responsible for the tree sprite.
- **Shadow Element:** The existing shadow GameObject.

The shadow is automatically detached from its parent during the falling animation while preserving its world position and rotation.

Because the shadow is detached, BreakSystem explicitly destroys it alongside the main tree GameObject.

## Breaking Sequence

When a tree reaches zero health:

1. BreakSystem detects that the tree is broken.
2. The current breakable target is cleared.
3. TreeFallAnimation is retrieved from the tree prefab.
4. The shadow is detached to prevent inherited rotation.
5. The tree visual element rotates toward the ground.
6. The animation finishes.
7. Persistent object destruction is registered.
8. Tree loot is generated and dropped.
9. The detached shadow is destroyed.
10. The main tree GameObject is destroyed.

## Compatibility

The system supports both animated and non-animated breakable objects.

| Object Type | Behavior |
|---|---|
| Trees | Play falling animation before dropping loot. |
| Rocks | Drop loot and get destroyed normally. |
| Other breakables | Retain the normal breaking behavior. |

## Important Notes

- Attach TreeFallAnimation to the tree prefab's root GameObject.
- Assign the correct Tree Element and Shadow Element references.
- Position the tree visual pivot at the bottom of the trunk for natural rotation.
- The shadow must remain assigned even though it is temporarily detached.
- Do not reintroduce immediate destruction inside BreakableMaterial.
- Loot must only drop after the falling animation finishes.

## Result

Trees now have a more natural chopping experience. They rotate and fall toward the ground, their shadows remain stationary, and loot only drops after the animation is completed.