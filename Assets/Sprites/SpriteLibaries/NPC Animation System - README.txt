# NPC Animation System

## Overview

The NPC Animation System controls the idle and walking animations of NPCs in Project Survival. It supports four movement directions:

- Up
- Down
- Left
- Right

Each direction has an idle animation and a walking animation, giving a total of eight animation clips.

The `NPCAnimationBehavior` script detects movement through the NPC's `Rigidbody2D` and selects the appropriate animation state. It does not control the NPC's AI, wandering, chasing, fleeing, or combat behavior.

This system uses **Sprite Library Assets and Sprite Resolver properties**, allowing different NPCs to share the same animation clips and Animator Controller while displaying their own sprites.

## 1. Required Animation Files

Create eight animation clips for the NPC.

| Animation Clip | Purpose |
| -------------- | ------- |
| `NPCIdleDown` | NPC standing while facing down |
| `NPCWalkDown` | NPC walking downward |
| `NPCIdleUp` | NPC standing while facing up |
| `NPCWalkUp` | NPC walking upward |
| `NPCIdleLeft` | NPC standing while facing left |
| `NPCWalkLeft` | NPC walking left |
| `NPCIdleRight` | NPC standing while facing right |
| `NPCWalkRight` | NPC walking right |

Keep the clip names consistent with this table because `NPCAnimationBehavior` uses these exact names to select animations.

A recommended project folder structure is:

```text
Assets/
└── Animations/
    └── NPC/
        ├── NPCAnimator.controller
        ├── NPCIdleDown.anim
        ├── NPCWalkDown.anim
        ├── NPCIdleUp.anim
        ├── NPCWalkUp.anim
        ├── NPCIdleLeft.anim
        ├── NPCWalkLeft.anim
        ├── NPCIdleRight.anim
        └── NPCWalkRight.anim
```

The names and folder structure are recommendations. The important requirement is that the clips and Animator states use the correct names.

## 2. Preparing NPC Sprites and Sprite Libraries

Before creating animations, prepare the NPC's sprite frames and Sprite Library Asset.

### Preparing the sprites

1. Import the NPC's sprite sheet into Unity.
2. Set **Sprite Mode** to `Multiple` if the sheet contains several frames.
3. Open the Sprite Editor and slice the sheet into individual sprites.
4. Apply the changes.
5. Verify that each frame is sliced correctly.

### Creating or configuring the Sprite Library

Each NPC species should have a Sprite Library Asset containing its own artwork.

1. Create or select the NPC's Sprite Library Asset.
2. Open the Sprite Library editor.
3. Create a category named `Sprite`.
4. Add labels for the animation frames.
5. Assign the corresponding sprites to those labels.

Use consistent category and label names across NPCs that share animations.

For example, a library might contain these labels:

| Animation | Example labels |
| --------- | -------------- |
| Downward idle | `IdleDown` |
| Downward walk | `WalkDown1`, `WalkDown2` |
| Upward idle | `IdleUp` |
| Upward walk | `WalkUp1`, `WalkUp2` |
| Left idle | `IdleLeft` |
| Left walk | `LeftWalk1`, `LeftWalk2` |
| Right idle | `IdleRight1` |
| Right walk | `WalkRight1`, `WalkRight2` |

Use exactly the same category and label names in every Sprite Library that will use the shared animation clips. If a label is misspelled or missing, the animation may not resolve to the intended sprite.

For example, if the walking animation uses `WalkUp2`, the Sprite Library must also contain `WalkUp2`. Do not use a different spelling such as `WalkIp2`.

Each NPC's Sprite Library maps these labels to that NPC's own sprites. The bunny library uses bunny sprites, while the sheep library uses sheep sprites.

## 3. Creating Animation Clips with Sprite Resolver

Create the animation clips using Unity's Animation window and the **Sprite Resolver** component instead of animating the Sprite Renderer directly.

### Preparing the NPC GameObject

Before recording animations, make sure the NPC has:

- A `Sprite Renderer` to display the resolved sprite.
- A `Sprite Library` component referencing the correct Sprite Library Asset.
- A `Sprite Resolver` component that works with the Sprite Library.

The Sprite Resolver selects a category and label from the Sprite Library. The corresponding sprite is then displayed by the Sprite Renderer.

### Creating an idle animation

1. Select the NPC GameObject that contains the Sprite Resolver.
2. Open **Window → Animation → Animation**.
3. Create a new clip named `NPCIdleDown`.
4. In the Animation window, press **Record**.
5. Add a frame at the starting time.
6. Select the Sprite Resolver properties in the Animation window.
7. Choose the appropriate category and label for the downward idle sprite, such as category `Sprite` and label `IdleDown`.
8. Stop recording.
9. Preview the clip and confirm that the correct sprite appears.

Repeat this process for `NPCIdleUp`, `NPCIdleLeft`, and `NPCIdleRight`, selecting the appropriate idle label for each direction.

### Creating a walking animation

1. Select the NPC GameObject.
2. Open the Animation window.
3. Create a clip named `NPCWalkDown`.
4. Press **Record**.
5. Add a frame at the starting time.
6. Select the Sprite Resolver properties and choose the first walking label, such as `WalkDown1`.
7. Move the timeline to the next frame time.
8. Add another frame and select the second walking label, such as `WalkDown2`.
9. Add more frames if the animation requires them.
10. Stop recording and preview the animation.

Repeat this process for the other three walking directions, using the corresponding labels from the Sprite Library.

**Important:** Animate the Sprite Resolver's category and label properties. Do not add `Sprite Renderer → Sprite` keyframes to these clips. Direct Sprite Renderer keyframes reference specific sprites and can cause another NPC to display the wrong animal's artwork.

### Walking animation settings

Select each walking clip in the Project window and enable **Loop Time** in its Inspector.

This allows the walking animation to repeat continuously while the NPC moves.

For idle clips, looping is optional when the clip contains only one static frame.

### How shared animations work

The animation clips specify which Sprite Resolver label to display, rather than directly referencing a particular animal's sprite.

For example:

- The shared animation requests `WalkDown1`.
- The bunny's Sprite Library resolves `WalkDown1` to the bunny's walking sprite.
- The sheep's Sprite Library resolves `WalkDown1` to the sheep's walking sprite.

Both NPCs can therefore use the same animation clips and Animator Controller while displaying different artwork.

## 4. Creating the Animator Controller

Create an Animator Controller named `NPCAnimator`.

1. In the Project window, right-click and select **Create → Animator Controller**.
2. Name it `NPCAnimator`.
3. Open it in the Animator window.
4. Assign it to the NPC's `Animator` component.

### Adding the animation states

Drag all eight animation clips into the Animator window.

Create these states:

- `NPCIdleDown`
- `NPCWalkDown`
- `NPCIdleUp`
- `NPCWalkUp`
- `NPCIdleLeft`
- `NPCWalkLeft`
- `NPCIdleRight`
- `NPCWalkRight`

Set `NPCIdleDown` as the default state. It should be orange and connected to the Entry node.

**Only `NPCIdleDown` connects directly to Entry.** The other states are selected by the animation behavior.

### Adding Animator parameters

Create the following three parameters in the Animator's Parameters tab.

| Parameter | Type | Initial Value |
| --------- | ---- | ------------- |
| `IsMoving` | Bool | False |
| `MoveX` | Float | 0 |
| `MoveY` | Float | -1 |

The script updates these parameters based on movement and facing direction. The current implementation selects states directly by name, so the `MoveX` and `MoveY` parameters are informational for this setup and can be used for future animation improvements.

### Configuring transitions

Create transitions between each idle/walk pair.

| From | To | Condition |
| ---- | -- | --------- |
| `NPCIdleDown` | `NPCWalkDown` | `IsMoving = true` |
| `NPCWalkDown` | `NPCIdleDown` | `IsMoving = false` |
| `NPCIdleUp` | `NPCWalkUp` | `IsMoving = true` |
| `NPCWalkUp` | `NPCIdleUp` | `IsMoving = false` |
| `NPCIdleLeft` | `NPCWalkLeft` | `IsMoving = true` |
| `NPCWalkLeft` | `NPCIdleLeft` | `IsMoving = false` |
| `NPCIdleRight` | `NPCWalkRight` | `IsMoving = true` |
| `NPCWalkRight` | `NPCIdleRight` | `IsMoving = false` |

For each transition:

- Disable **Has Exit Time**.
- Set **Transition Duration** to `0`.
- Use the condition shown in the table.

The transitions are useful if the Animator is controlled through parameters. However, the current `NPCAnimationBehavior` directly calls `Animator.Play()` to select the desired state. Avoid adding conflicting transition logic or other scripts that also control the same Animator.

## 5. NPCAnimationBehavior Script

The `NPCAnimationBehavior` script reads the NPC's position changes and selects the matching idle or walking state.

### How it works

1. It obtains the NPC's `Rigidbody2D` and `Animator`.
2. It samples movement in `FixedUpdate()`, in sync with physics updates.
3. It compares the current position with the previous position.
4. It ignores movement smaller than the configured threshold.
5. It determines the dominant movement direction.
6. It updates `IsMoving`, `MoveX`, and `MoveY`.
7. It plays the matching animation state.

For example:

| NPC Movement | Animation |
| ------------ | --------- |
| Moving down | `NPCWalkDown` |
| Moving up | `NPCWalkUp` |
| Moving left | `NPCWalkLeft` |
| Moving right | `NPCWalkRight` |
| Stops after moving up | `NPCIdleUp` |
| Stops after moving left | `NPCIdleLeft` |

The script preserves the previous facing direction when the NPC stops moving.

### Movement threshold

The current movement threshold is:

```csharp
[SerializeField] private float movementThreshold = 0.02f;
```

This helps ignore very small position changes that could otherwise cause rapid switching between idle and walking animations.

Keep the working value unless testing shows that a different movement scale requires adjustment.

## 6. NPC Prefab Requirements

The NPC prefab must have the following components.

### Required components

**Rigidbody2D**

- Must be present on the same GameObject as `NPCAnimationBehavior`.
- Must be the Rigidbody2D used by the NPC's movement script.
- The movement system must actually move this Rigidbody2D.

**Animator**

- Must have the shared `NPCAnimator` Controller assigned.
- Must contain the eight expected animation states.
- Can be on the same GameObject or a child GameObject, provided the animation bindings target the correct Sprite Resolver.

**Sprite Renderer**

- Displays the sprite selected by the Sprite Resolver.
- Must be on the intended GameObject or correctly targeted by the Sprite Resolver.

**Sprite Library**

- Must reference the correct Sprite Library Asset for that NPC.
- Must contain the category and labels expected by the shared animation clips.

**Sprite Resolver**

- Must be present and compatible with the Sprite Library.
- Must resolve the category and labels animated by the clips.

**NPCAnimationBehavior**

- Add it to the same GameObject as the Rigidbody2D.
- Assign the Animator and Rigidbody2D in the Inspector if they are not found automatically.

### Existing movement scripts

Keep the NPC's existing movement script.

For example:

- Enemies can keep `EnemyController`.
- Passive animals can keep `PassiveMobController`.

`NPCAnimationBehavior` handles animation selection only. It should not replace or duplicate the AI movement logic.

Avoid attaching multiple animation scripts that attempt to control the same Animator.

## 7. Adding the System to Another NPC

To add the shared animation system to another NPC:

1. Prepare the NPC's sprite frames.
2. Create or configure a Sprite Library Asset for that NPC.
3. Add the same category and labels used by the existing animation clips.
4. Assign the NPC's own sprites to the matching labels.
5. Add or verify the Sprite Library and Sprite Resolver components on the prefab.
6. Assign the shared `NPCAnimator` Controller to the Animator.
7. Ensure the prefab has a Rigidbody2D and `NPCAnimationBehavior`.
8. Keep the NPC's existing movement script.
9. Test movement in all four directions and verify that idle animations preserve the last facing direction.

You do not need to create eight new animation clips for each animal when the existing clips animate Sprite Resolver properties and the new Sprite Library uses matching category and label names.

For example, the bunny and sheep can share the same eight clips and Animator Controller, but each prefab must reference its own Sprite Library Asset.

## 8. Troubleshooting

### NPC switches rapidly between idle and walking

Check that `NPCAnimationBehavior` samples movement in `FixedUpdate()` and that the movement threshold is configured appropriately.

Also verify that no other component is changing `IsMoving` or calling `Animator.Play()` on the same Animator.

### NPC keeps facing the wrong direction

Check that the movement script moves the same Rigidbody2D referenced by `NPCAnimationBehavior`. Confirm that all eight state names match exactly.

### NPC does not change sprites

Check that:

- The Animator Controller is assigned.
- All eight states exist.
- The animation clips contain Sprite Resolver category and label keyframes.
- The Sprite Resolver is present and correctly configured.
- The Sprite Library Asset is assigned.
- Every animated category and label exists in that library.

### NPC displays the wrong animal's sprites

Check whether the clips contain direct `Sprite Renderer → Sprite` keyframes. These keyframes reference specific sprites and can override the expected shared-animation behavior.

Use Sprite Resolver properties in the animation clips and assign the correct Sprite Library Asset to each NPC prefab. Confirm that each library maps the shared labels to that animal's own artwork.

### Sprite Resolver cannot find an animation label

Check the category and label spelling in both the animation clip and Sprite Library Asset. They must match exactly.

For example, `WalkUp2` and `WalkIp2` are different labels.

### NPC returns to the wrong idle direction

Verify that `NPCAnimationBehavior` is attached to the correct Rigidbody2D and that no other script resets the animation state.

## Summary

The NPC Animation System consists of eight directional animation clips, a shared Animator Controller, a Sprite Library Asset for each NPC species, a Sprite Resolver, and the `NPCAnimationBehavior` script.

The animation clips use Sprite Resolver properties recorded through Unity's Animation window. Each NPC's Sprite Library maps the same category and label names to its own artwork, allowing animals such as bunnies and sheep to share animation clips without displaying each other's sprites.

Existing AI scripts remain responsible for movement, while `NPCAnimationBehavior` determines which directional idle or walking animation is displayed.