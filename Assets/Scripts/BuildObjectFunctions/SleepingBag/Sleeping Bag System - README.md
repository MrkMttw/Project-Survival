# Sleeping Bag System

## Overview

The Sleeping Bag System allows the player to sleep using a placed sleeping bag. Sleeping transitions the game from nighttime to morning, advances the in-game day, and applies a hunger penalty.

The system uses an interaction trigger, a sleep transition sequence, and a screen fade effect to create a smooth transition between sleeping and waking up.

## Scripts

### 1. SleepingBagHitbox.cs

Detects sleeping bags within the player's interaction hitbox.

**Responsibilities:**
- Detects nearby sleeping bags using `OnTriggerStay2D`.
- Clears the current sleeping bag reference when the player leaves its trigger.
- Handles interaction requests through `TryInteract()`.
- Passes the `SleepTransitionController` reference to the sleeping bag.

**Important references:**
- `Player Interaction Controller`: The player's interaction controller.

### 2. SleepTransitionController.cs

Controls the entire sleeping sequence.

**Responsibilities:**
- Checks whether the current in-game time allows sleeping.
- Prevents the player from moving during sleep.
- Stops the player's Rigidbody2D velocity.
- Fades the screen to black.
- Displays the sleeping message one character at a time.
- Moves the player to the sleeping position.
- Advances the game clock to morning.
- Reduces the player's current hunger by half.
- Fades the screen back in.
- Restores player movement after the transition.

**Sleep time settings:**

| Setting | Default | Description |
|---|---|---|
| Sleep Start Hour | 18 | Earliest hour at which sleeping is allowed. |
| Sleep End Hour | 6 | Hour at which the overnight sleep window ends. |
| Character Interval | 0.5 | Delay between characters in the sleeping message. |

The sleep time uses the 24-hour clock. With the default settings, sleeping is allowed from 18:00 until 06:00.

The system supports both overnight and same-day time ranges. Setting the start and end hours to the same value allows sleeping at any time.

**Important references:**
- `Player Movement`: The player's movement controller.
- `Game Clock`: Controls the in-game time and morning transition.
- `Hunger Controller`: Applies the hunger penalty.
- `Screen Fade Controller`: Handles the screen transition.
- `Sleep Text`: TextMeshPro UI element displaying the sleeping message.

### 3. ScreenFadeController.cs

Controls the screen fade effect during sleeping.

**Responsibilities:**
- Fades the screen to black using `FadeOut()`.
- Reveals the morning using `FadeIn()`.
- Sets the screen transparency immediately using `SetImmediate()`.
- Uses unscaled time so the fade animation does not depend on the game's time scale.

**Settings:**

| Setting | Default | Description |
|---|---|---|
| Fade Panel | None | UI Image used as the screen overlay. |
| Fade Duration | 1 second | Duration of each fade transition. |

The fade panel's alpha changes between 0 (transparent) and 1 (fully opaque).

### 4. PlayerInteractionController.cs

Handles the player's interaction input.

**Responsibilities:**
- Detects the E key using Unity's Input System.
- Prioritizes sleeping bag interaction.
- Attempts to eat the held item if no sleeping bag interaction succeeds.
- Attempts to toggle the held item's light if the previous interactions do not succeed.

Sleeping bag interaction takes priority over eating and toggling item lights.

### 5. SleepingBagInteraction.cs

Handles the sleeping bag's interaction behavior and connects it to the sleep transition system.

This script is required by `SleepingBagHitbox`, which searches for a `SleepingBagInteraction` component on the detected collider or one of its parents.

Ensure its interaction method accepts the `SleepTransitionController` reference and starts the sleeping sequence using the appropriate sleeping position.

## Unity Setup

### Player

1. Add `PlayerInteractionController` to the player.
2. Assign the existing `FoodController` and `ItemLightController` references.
3. Assign the `SleepTransitionController` reference.
4. Assign the `SleepingBagHitbox` reference.
5. Set up the sleeping bag hitbox with a Collider2D configured as a trigger.
6. Assign the player's `PlayerInteractionController` to the hitbox.

### Sleeping Bag Prefab

1. Add `SleepingBagInteraction` to the sleeping bag prefab or its appropriate parent object.
2. Add a Collider2D configured to detect the player's sleeping bag hitbox.
3. Configure the sleeping position used when the player sleeps.
4. Ensure the sleeping bag's collider is detected by the player's hitbox.

The sleeping bag interaction component must be on the collider object or one of its parents.

### Sleep Transition Controller

1. Create or select the GameObject that will manage sleeping.
2. Attach `SleepTransitionController`.
3. Assign the player's `PlayerMovement`.
4. Assign the `GameClock`.
5. Assign the player's `HungerController`.
6. Assign the `ScreenFadeController`.
7. Assign the TextMeshPro UI element used for the sleeping message.
8. Configure the permitted sleeping hours if needed.

### Screen Fade Controller

1. Create a full-screen UI Image on the appropriate Canvas.
2. Make sure the image covers the entire screen.
3. Set its color to black and ensure its alpha can be changed.
4. Attach `ScreenFadeController` to the appropriate GameObject.
5. Assign the UI Image to `Fade Panel`.
6. Configure `Fade Duration`.

Make sure the fade panel renders above the gameplay and sleeping text when the screen should be black. The sleeping text must remain visible during the black-screen portion of the sequence.

## Sleeping Sequence

1. The player approaches a sleeping bag.
2. The sleeping bag hitbox detects it.
3. The player presses **E**.
4. The interaction controller attempts to interact with the sleeping bag first.
5. The sleep controller checks whether sleeping is allowed.
6. Player movement is disabled, and the screen fades to black.
7. The sleeping message appears one character at a time.
8. The player is moved to the configured sleeping position.
9. The game clock advances to morning by the requested number of days.
10. Current hunger is halved.
11. The screen fades back in.
12. Player movement is restored.

If sleeping is not allowed at the current time, the sleeping sequence does not begin.

## Dependencies

This system depends on the following existing game systems:

- `PlayerMovement`
- `GameClock`
- `HungerController`
- `PlayerInteractionController`
- `FoodController`
- `ItemLightController`

It also requires the `SleepingBagInteraction` component and a configured screen fade UI.

## Notes

- Sleeping is triggered with the **E** key.
- The default sleeping window is 18:00–06:00.
- The sleep transition rejects new sleep requests while a sleep sequence is already running.
- The `TryStartSleep()` method accepts a `daysToAdvance` argument, which defaults to 1.
- The hunger penalty is applied once per sleep sequence, regardless of how many days are advanced.
- Player movement and screen state are restored when the sleep sequence finishes or the transition controller is disabled.
- The system uses `WaitForSecondsRealtime` for the sleeping message animation.
- The actual morning time and day advancement are handled by `GameClock.AdvanceToMorning()`.