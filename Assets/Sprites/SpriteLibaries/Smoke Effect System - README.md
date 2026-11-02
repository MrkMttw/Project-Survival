# Smoke Effect System

The Smoke Effect System provides reusable smoke animations for player actions and breakable world objects in Project Survival.

It uses a dedicated `SmokeEffectAnimator` component to control the smoke GameObject, Animator, animation trigger, and playback duration.

The system supports two types of smoke effects:

- **Player Action Smoke** – Plays whenever the player performs a held-item swing.
- **Breakable Object Smoke** – Plays when a breakable object is destroyed. The smoke is detached from the object so it can finish playing before the destroyed object is removed.

---

## Features

- Reusable smoke animation component.
- Plays smoke whenever `PlayerHeldItem.PlaySwing()` is called.
- Plays smoke when a breakable object is destroyed.
- Restarts the smoke animation when triggered again.
- Uses Unity Animator and Sprite Resolver for sprite animation.
- Supports detached smoke effects for breakable objects.
- Keeps detached smoke alive while the destroyed object is being removed.
- Configurable smoke playback duration.
- Automatically disables normal player smoke after its duration.

---

## 1. Required Scripts

| Script | Purpose |
|---|---|
| `SmokeEffectAnimator.cs` | Controls smoke activation, animation playback, and detachment. |
| `PlayerHeldItem.cs` | Triggers smoke whenever the player performs a held-item swing. |
| `BreakSystem.cs` | Triggers detached smoke when a breakable object is destroyed. |
| `TreeFallAnimation.cs` | Handles tree falling before the destruction and smoke sequence. |

---

## 2. Hierarchy Setup

### Player Smoke

Create a `Smoke` GameObject under the Player.

```text
Player
├── Visual
├── HeldItem
└── Smoke
    ├── SpriteRenderer
    ├── SpriteResolver
    ├── Animator
    └── SmokeEffectAnimator
```

Attach `SmokeEffectAnimator` to the Smoke GameObject.

### Breakable Object Smoke

Add a Smoke GameObject to every breakable object that should produce smoke when destroyed.

Example:

```text
Stone
├── Smoke
│   ├── SpriteRenderer
│   ├── SpriteResolver
│   ├── Animator
│   └── SmokeEffectAnimator
└── Shadow
```

Tree example:

```text
Tree
├── TreeElement
├── Smoke
│   ├── SpriteRenderer
│   ├── SpriteResolver
│   ├── Animator
│   └── SmokeEffectAnimator
└── Shadow
```

The smoke should be assigned to the corresponding object's `SmokeEffectAnimator` component.

**Important:** The detached smoke becomes a root-level GameObject when `PlaySmokeDetached()` is called. Make sure its position and visual scale are appropriate for the world.

---

## 3. Animator Setup

Create an Animator Controller for the Smoke GameObject.

### Parameters

| Name | Type |
|---|---|
| `Play` | Trigger |

### Animator States

- **Idle** – Default state. No smoke animation is playing.
- **Smoke** – Contains the smoke animation clip.

### Transitions

**Idle → Smoke**

- Has Exit Time: OFF
- Transition Duration: 0
- Condition: `Play`

**Smoke → Idle**

- Has Exit Time: ON
- Exit Time: 1
- Transition Duration: 0
- No conditions required.

### Animation Clip Setup

Select the Smoke animation clip and configure:

- Loop Time: OFF
- Samples: Set your preferred animation frame rate.
- Sprite Resolver: Ensure the animation updates the correct sprite.

The animation should play once and return to Idle.

---

## 4. SmokeEffectAnimator Inspector Setup

Select the Smoke GameObject and configure the `SmokeEffectAnimator` component.

| Field | Assignment |
|---|---|
| Smoke Object | The Smoke GameObject |
| Smoke Animator | The Animator component on Smoke |
| Smoke Trigger | `Play` |
| Smoke Duration | Duration of the smoke effect in seconds |

The `Smoke Duration` should be long enough for the animation to finish.

For example, the default duration is `0.5` seconds.

---

## 5. Player Action Integration

The `PlayerHeldItem` script references the reusable `SmokeEffectAnimator` component.

### Inspector Setup

1. Select the Player's `HeldItem` GameObject.
2. Locate the `PlayerHeldItem` component.
3. Assign the Player's Smoke GameObject's `SmokeEffectAnimator` component to `Smoke Effect Animator`.

### How It Works

Whenever `PlaySwing()` is called, it invokes:

```csharp
// PLAY SMOKE EFFECT
private void PlaySmoke()
{
    if (smokeEffectAnimator == null)
        return;

    smokeEffectAnimator.PlaySmoke();
}
```

This allows the player to produce smoke whenever a held-item swing occurs.

The normal smoke effect automatically disables itself after the configured smoke duration.

---

## 6. Breakable Object Integration

The `BreakSystem` automatically searches for a `SmokeEffectAnimator` component when a breakable object is destroyed.

It uses:

```csharp
SmokeEffectAnimator smokeEffect =
    material.GetComponentInParent<SmokeEffectAnimator>();
```

When the object breaks, the system calls:

```csharp
detachedSmoke = smokeEffect.PlaySmokeDetached();
```

### Detached Smoke Behavior

The `PlaySmokeDetached()` method:

1. Stops any previous normal smoke coroutine.
2. Detaches the Smoke GameObject from its parent.
3. Sets its rotation to an upright orientation.
4. Activates the Smoke GameObject.
5. Restarts the Animator.
6. Triggers the smoke animation.
7. Returns the detached Smoke GameObject.

This allows the smoke to continue playing independently of the breakable object.

---

## 7. BreakSystem Destruction Sequence

When a breakable object reaches zero health, `FinishBreaking()` handles its destruction.

### General Breakable Objects

1. Detect the destroyed object.
2. Play its smoke animation.
3. Update world or structure destruction data when applicable.
4. Spawn the object's loot drops.
5. Wait for the smoke duration.
6. Destroy the detached smoke.
7. Destroy the broken object.

### Trees

Trees have an additional falling animation.

1. Play the tree falling animation.
2. Detach the tree's shadow.
3. Play the detached smoke animation.
4. Update world or structure destruction data when applicable.
5. Spawn the tree's loot drops.
6. Wait for the smoke duration.
7. Destroy the detached smoke.
8. Destroy the shadow.
9. Destroy the tree.

The tree's falling animation is handled by `TreeFallAnimation.cs`.

---

## 8. TreeFallAnimation Setup

For trees, attach `TreeFallAnimation` to the tree's root GameObject.

Configure the following fields:

| Field | Assignment |
|---|---|
| Tree Element | The transform containing the tree sprite that should rotate. |
| Shadow Element | The tree's Shadow GameObject. |
| Fall Duration | How long the tree takes to fall. |
| Fall Angle | The angle of the falling animation. |
| Fall Curve | Controls the progression of the falling animation. |

The `TreeElement` rotates during the falling animation, while the shadow is detached and preserved until the destruction sequence finishes.

---

## 9. Expected Behavior

### Player Actions

1. Player performs a held-item swing.
2. Smoke activates.
3. Smoke animation plays from its beginning.
4. Animation finishes.
5. Smoke automatically becomes inactive.
6. The next swing can trigger the animation again.

### Breakable Objects

1. Player destroys a breakable object.
2. If the object is a tree, its falling animation plays first.
3. Smoke detaches from the object.
4. Smoke animation plays independently.
5. Loot drops spawn.
6. The system waits for the configured smoke duration.
7. Smoke is destroyed.
8. The broken object and its shadow are removed.

---

## Important Notes

- Every object that needs a destruction smoke effect must have a `SmokeEffectAnimator` component in its hierarchy.
- Assign the correct Smoke GameObject and Animator references in the Inspector.
- The Animator trigger must match the configured `Smoke Trigger`.
- `Smoke Duration` should match or slightly exceed the animation's playback duration.
- Player smoke is triggered through `PlayerHeldItem.PlaySwing()`.
- Breakable object smoke is triggered through `BreakSystem.FinishBreaking()`.
- Detached smoke is no longer a child of its original object after `PlaySmokeDetached()` is called.
- Objects without a `SmokeEffectAnimator` can still be destroyed normally, but they will not produce smoke.