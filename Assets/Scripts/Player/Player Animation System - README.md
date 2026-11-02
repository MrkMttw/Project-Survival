# Player Animation System

The Player Animation System handles the player's directional walking and idle animations using Unity's 2D Animation package.

It uses **Sprite Library** and **Sprite Resolver** to manage character sprites, allowing the Animator to switch between different directional animations.

The system also supports sprint animation speed adjustments and remembers the player's last facing direction when movement stops.

---

## 1. System Overview

The system uses two main scripts:

| Script | Responsibility |
|---|---|
| `PlayerAnimationBehavior.cs` | Handles directional animations, idle states, and animation speed |
| `PlayerMovement.cs` | Handles movement input and communicates with the animation system |

### Supported Animations

The player has eight animation states:

| Direction | Walking | Idle |
|---|---|---|
| Up | `PlayerWalkUp` | `PlayerIdleUp` |
| Down | `PlayerWalkDown` | `PlayerIdleDown` |
| Left | `PlayerWalkLeft` | `PlayerIdleLeft` |
| Right | `PlayerWalkRight` | `PlayerIdleRight` |

Diagonal movement uses horizontal direction priority.

For example:
- W + A = Walk Left
- W + D = Walk Right
- S + A = Walk Left
- S + D = Walk Right

When the player stops moving, the system plays the idle animation corresponding to the last facing direction.

---

## 2. Required Unity Components

The Player GameObject should have the following components:

- Rigidbody2D
- Animator
- Sprite Renderer
- Sprite Library
- PlayerMovement
- PlayerAnimationBehavior

The `Sprite Resolver` should be attached to the GameObject responsible for displaying the animated character sprite.

### Recommended Hierarchy

```text
Player
├── Sprite
│   ├── Sprite Renderer
│   ├── Sprite Resolver
│   └── Sprite Library
│
├── Rigidbody2D
├── Animator
├── PlayerMovement
└── PlayerAnimationBehavior
```

The Sprite Resolver and Sprite Library can be arranged differently depending on your existing character hierarchy. Make sure the Animator controls the correct animated object.

---

## 3. Setting Up the Sprite Library

The Sprite Library stores the different sprites used by the character.

### Step 1: Create a Sprite Library Asset

1. Import your character sprites into Unity.
2. Select the character sprite sheet.
3. Set its Texture Type to `Sprite (2D and UI)`.
4. Set Sprite Mode to `Multiple` if the sheet contains multiple sprites.
5. Open the Sprite Editor and slice your sprites.
6. Apply the changes.

### Step 2: Create the Sprite Library

1. Right-click inside the Project window.
2. Select `Create → 2D → Sprite Library Asset`.
3. Name it `PlayerSpriteLibrary`.
4. Open the Sprite Library Asset.
5. Create the categories and labels needed for your character.

Example organization:

```text
PlayerSpriteLibrary
│
├── Idle
│   ├── Up
│   ├── Down
│   ├── Left
│   └── Right
│
└── Walk
    ├── Up
    ├── Down
    ├── Left
    └── Right
```

Assign the corresponding sliced sprites to their labels.

**Important:** The category and label names must match the references used by your Sprite Resolver animation keyframes.

---

## 4. Setting Up the Sprite Resolver

The Sprite Resolver determines which sprite from the Sprite Library is currently displayed.

1. Select the animated Sprite GameObject.
2. Add the `Sprite Resolver` component.
3. Add the `Sprite Library` component.
4. Assign your `PlayerSpriteLibrary` asset to the Sprite Library component.
5. Check that the Sprite Resolver can access the Sprite Library.

The Sprite Resolver should be animated through the Animation window.

### Animating the Sprite Resolver

For each animation clip:

1. Open `Window → Animation → Animation`.
2. Select the Player GameObject.
3. Choose the animation clip you want to edit.
4. Add the Sprite Resolver's category and label properties to the animation.
5. Create keyframes for the corresponding sprites.
6. Repeat for the remaining animation clips.

Make sure each animation clip references the correct category and label.

---

## 5. Creating Animation Clips

Create eight animation clips for the player.

Recommended naming:

```text
PlayerWalkUp
PlayerWalkDown
PlayerWalkLeft
PlayerWalkRight

PlayerIdleUp
PlayerIdleDown
PlayerIdleLeft
PlayerIdleRight
```

### Walking Animations

Walking animations should contain the appropriate Sprite Resolver keyframes for each walking frame.

Recommended starting settings:

| Setting | Suggested Value |
|---|---|
| Samples | 8–12 FPS |
| Loop Time | Enabled |
| Animation Frames | Based on your sprite sheet |

Adjust the sample rate depending on your animation style.

### Idle Animations

Idle animations should contain the appropriate sprite references for their respective directions.

For a static idle animation, use a single sprite.

If you want an animated idle, add multiple sprite keyframes and enable Loop Time.

**Important:** Inspect the `PlayerIdleUp` clip carefully. Make sure its Sprite Resolver keyframes do not accidentally reference different sprites unless you intentionally want an animated idle.

---

## 6. Setting Up the Animator Controller

Create an Animator Controller named `PlayerAnimatorController`.

Assign it to the Player's Animator component.

Add the eight animation clips to the Animator Controller.

The script uses `Animator.Play()` to directly play animation states, so the state names must match the clip names exactly.

Required state names:

```text
PlayerWalkUp
PlayerWalkDown
PlayerWalkLeft
PlayerWalkRight

PlayerIdleUp
PlayerIdleDown
PlayerIdleLeft
PlayerIdleRight
```

### Animator Parameters

Add the following parameters:

| Parameter | Type | Purpose |
|---|---|---|
| IsMoving | Bool | Indicates whether the player is moving |
| MoveX | Float | Stores horizontal facing direction |
| MoveY | Float | Stores vertical facing direction |

These parameters are updated by `PlayerAnimationBehavior`.

The script directly selects the animation states, so you do not need to create transition conditions for every directional combination.

---

## 7. PlayerAnimationBehavior.cs

Attach this script to the Player GameObject.

### Inspector Setup

| Field | Description | Suggested Value |
|---|---|---|
| Animator | Player's Animator component | Player Animator |
| Movement Threshold | Minimum input needed to count as movement | 0.02 |
| Walk Animation Speed | Normal animation playback speed | 1 |
| Sprint Animation Speed | Sprint animation playback speed | 1.6 |

The script:
- Detects movement using the player's input.
- Updates the player's facing direction.
- Plays the correct walking animation.
- Plays the corresponding idle animation when movement stops.
- Changes animation playback speed while sprinting.

The movement input is used instead of Rigidbody2D position changes to prevent unwanted animation direction changes caused by small physics movements.

---

## 8. PlayerMovement.cs Integration

The PlayerMovement script communicates with PlayerAnimationBehavior.

Make sure the following reference exists:

```csharp
private PlayerAnimationBehavior animationBehavior;
```

Inside `Start()`, retrieve the component:

```csharp
animationBehavior = GetComponent<PlayerAnimationBehavior>();
```

Inside `Update()`, update the animation system:

```csharp
animationBehavior.SetSprinting(isSprinting);
animationBehavior.SetMovementInput(moveInput);
```

`SetMovementInput()` receives the movement input from the Input System.

`SetSprinting()` updates the Animator playback speed depending on whether the player is sprinting.

---

## 9. Testing the Animation System

After completing the setup, test the following:

| Input | Expected Animation |
|---|---|
| W | PlayerWalkUp |
| S | PlayerWalkDown |
| A | PlayerWalkLeft |
| D | PlayerWalkRight |
| W + A | PlayerWalkLeft |
| W + D | PlayerWalkRight |
| S + A | PlayerWalkLeft |
| S + D | PlayerWalkRight |
| No Input | Idle in last facing direction |
| Left Shift + Movement | Faster walking animation |

Also test that the player maintains the correct idle sprite after releasing movement keys.

---

## 10. Troubleshooting

### Player Changes Direction While Standing Still

**Possible cause:** Movement detection is based on Rigidbody2D position changes.

**Solution:** Use movement input to determine the animation direction instead of relying on small physics movements.

### Player Uses the Wrong Sprite

**Possible cause:** Incorrect Sprite Resolver category or label assignment.

**Solution:** Inspect the Sprite Library Asset and the Sprite Resolver keyframes inside the animation clip.

### Player Keeps Changing Sprites While Idle

**Possible cause:** Multiple Sprite Resolver keyframes are present in the idle animation.

**Solution:** Open the affected idle clip in the Animation window and inspect its keyframes.

Pay special attention to `PlayerIdleUp` if the issue only happens when facing upward.

### Animation Does Not Play

Check the following:
- Animator Controller is assigned.
- Animation state names match the names used by the script.
- Sprite Library Asset is assigned.
- Sprite Resolver is configured correctly.
- Animator parameters are named correctly.

---

## 11. Final Notes

The Player Animation System separates movement handling from animation handling.

`PlayerMovement` is responsible for player movement and sprinting, while `PlayerAnimationBehavior` handles directional animation playback.

Using Sprite Library and Sprite Resolver makes it easier to manage character sprites without manually changing sprites through code.

When adding new directional animations or changing character sprites, update the Sprite Library and corresponding animation clips while keeping the required Animator state names consistent.