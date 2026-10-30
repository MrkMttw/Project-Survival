# Health and Game Over System

## Overview

The Health and Game Over System manages the player's health, health UI, death detection, Game Over screen, and respawning in Project Survival.

The system consists of two scripts:

- `HealthController.cs` — manages health values, damage, healing, maximum health, health UI, and death events.
- `GameOverController.cs` — handles the Game Over panel, pauses gameplay, remembers the player's starting position, and restores the player when respawning.

## 1. HealthController

### Responsibilities

`HealthController` is responsible for:

- Tracking the player's current and maximum health.
- Updating the health bar and health text.
- Applying damage and healing.
- Increasing maximum health based on campfire level.
- Detecting death and notifying other systems.
- Restoring health after respawning.

### Health Variables

| Variable | Default | Description |
|---|---:|---|
| `baseMaxHealth` | 100 | The player's base maximum health. |
| `maxHealth` | 100 | The player's current maximum health. |
| `currentHealth` | 100 | The player's current health. |
| `deathTriggered` | `false` | Prevents death from triggering repeatedly before respawning. |

### Health UI

The script uses two UI objects:

- `healthBar` — a UI Image configured with the Filled image type.
- `healthText` — a TextMeshPro UI text object displaying current and maximum health.

The `UpdateHealthUI()` method updates the fill amount and displays the health values.

The health bar fill amount is calculated using:

`currentHealth / maxHealth`

The displayed values are rounded up to whole numbers using `Mathf.CeilToInt()`.

### Taking Damage

The `TakeDamage(float amount)` method reduces the player's health by the specified amount.

The method:
1. Checks whether death has already been triggered.
2. Subtracts the damage from current health.
3. Clamps health between zero and maximum health.
4. Updates the health UI.
5. Triggers the `OnDeath` event when health reaches zero.

Once death has been triggered, further damage is ignored until the player respawns.

### Healing

The `Heal(float amount)` method restores health by the specified amount.

Healing is ignored when:
- Death has already been triggered.
- Current health is already at maximum health.

Health cannot exceed `maxHealth` because the value is clamped.

### Maximum Health and Campfire Level

The `UpdateMaxHealth(int campfireLevel)` method calculates maximum health using:

`maxHealth = baseMaxHealth + (campfireLevel * 20f)`

Each campfire level adds 20 HP to the player's base maximum health.

For example, with a base maximum health of 100:

| Campfire Level | Maximum Health |
|---:|---:|
| 0 | 100 |
| 1 | 120 |
| 2 | 140 |
| 3 | 160 |
| 4 | 180 |
| 5 | 200 |

This method updates maximum health and clamps current health to the new maximum. It does not automatically increase current health to fill the newly available health capacity.

Another system must call `UpdateMaxHealth()` when the campfire level changes.

### Death Event

The script declares:

`public event Action OnDeath;`

When the player's health reaches zero, `HealthController` invokes this event. Other systems can subscribe to the event and react to the player's death.

In the current implementation, `GameOverController` subscribes to this event to display the Game Over screen.

### Respawn Health

The `RespawnHealth()` method:
1. Restores current health to maximum health.
2. Resets the death flag.
3. Updates the health UI.

Resetting the death flag allows the player to die again after respawning.

### IsDead()

The `IsDead()` method returns `true` when current health is zero or lower. It can be used by other gameplay systems to check whether the player is dead.

---

## 2. GameOverController

### Responsibilities

`GameOverController` manages the transition between gameplay and the Game Over state.

It is responsible for:

- Hiding the Game Over panel at startup.
- Listening for the player's death event.
- Displaying the Game Over panel.
- Pausing gameplay.
- Saving the player's starting position.
- Returning the player to that position during respawn.
- Resetting player movement and health.
- Hiding the panel and resuming gameplay.

### Required References

| Variable | Purpose |
|---|---|
| `healthController` | The player's `HealthController`, used to receive death events and restore health. |
| `gameOverPanel` | The UI panel displayed when the player dies. |
| `player` | The player's root Transform, used to save and restore position. |

### Initialization

In `Awake()`, the Game Over panel is hidden.

In `Start()`, the controller saves the player's initial position in `respawnPosition` and gets the player's `Rigidbody2D`, if one is attached to the assigned player object.

The saved position is the position where the player was when the scene started. It is not updated automatically when the player moves or reaches another location.

### Detecting Death

The controller subscribes to `HealthController.OnDeath` in `OnEnable()` and unsubscribes in `OnDisable()`.

When the event is received, `ShowGameOver()` is called.

The method:
1. Checks whether the game is already in the Game Over state.
2. Sets `isGameOver` to `true`.
3. Activates the Game Over panel.
4. Sets `Time.timeScale` to zero.

Setting `Time.timeScale = 0f` pauses gameplay systems that depend on scaled time. It does not automatically disable every script, input handler, or UI interaction.

### Checking the Game Over State

The `IsGameOver()` method returns the value of `isGameOver`.

Other gameplay scripts can use this method to prevent actions such as attacking or interacting while the Game Over screen is active.

For example, input-handling scripts can check the Game Over state before processing gameplay input.

### Respawning

The `Respawn()` method is intended to be called by the Respawn button.

It only runs when the player is in the Game Over state.

The method:
1. Checks that the player and health controller references are assigned.
2. Restores the player's position to `respawnPosition`.
3. Resets the player's Rigidbody2D linear and angular velocity, if a Rigidbody2D was found.
4. Calls `HealthController.RespawnHealth()` to restore full health.
5. Resets `isGameOver` to `false`.
6. Hides the Game Over panel.
7. Sets `Time.timeScale` to `1f` to resume gameplay.

If the player or health controller reference is missing, the method logs an error and stops without completing the respawn.

**Current limitation:** Respawning restores the player's position and health, but does not reset inventory, enemies, world objects, or other gameplay progress.

---

## 3. Unity Setup

### Step 1: Configure HealthController

1. Select the player GameObject.
2. Add the `HealthController` component if it is not already attached.
3. Assign the `Health Bar` field to the UI Image used for the health bar.
4. Assign the `Health Text` field to the TextMeshPro text object.
5. Ensure the health bar Image uses the Filled image type and has an appropriate fill method.

Both UI references should be assigned before entering Play mode.

### Step 2: Configure GameOverController

1. Create or select a GameObject to hold the `GameOverController` component.
2. Attach `GameOverController.cs`.
3. Assign the player's `HealthController` to the **Health Controller** field.
4. Assign the Game Over UI panel to the **Game Over Panel** field.
5. Assign the player's root Transform to the **Player** field.
6. Make sure the Game Over panel starts inactive in the scene, or allow the script to deactivate it in `Awake()`.

The controller subscribes to the health death event automatically. No manual event connection is required in the Inspector.

### Step 3: Configure the Respawn Button

1. Select the Respawn button inside the Game Over panel.
2. Find the button's **On Click()** event in the Inspector.
3. Add the GameObject containing `GameOverController`.
4. Select `GameOverController` → `Respawn()`.

When clicked during Game Over, the button should restore the player's starting position and full health, close the panel, and resume gameplay.

### Step 4: Check Other Gameplay Scripts

Any gameplay input scripts that should stop accepting input during Game Over should check `GameOverController.IsGameOver()`.

Pausing with `Time.timeScale = 0f` is useful, but it does not guarantee that every custom input handler will stop running.

---

## 4. Testing Checklist

### Health System

- [ ] The health bar and text show the correct starting health.
- [ ] Taking damage decreases health and updates the UI.
- [ ] Health cannot fall below zero.
- [ ] Healing restores health without exceeding maximum health.
- [ ] Healing does not work after death has been triggered.
- [ ] Increasing campfire level updates maximum health when `UpdateMaxHealth()` is called.

### Game Over System

- [ ] When health reaches zero, the Game Over panel appears.
- [ ] The game pauses when the panel appears.
- [ ] Death does not trigger the Game Over process repeatedly.
- [ ] Gameplay input is blocked by any scripts configured to check `IsGameOver()`.

### Respawn System

- [ ] Clicking Respawn returns the player to the position saved at scene startup.
- [ ] The player's movement stops after respawning.
- [ ] Current health is restored to maximum health.
- [ ] The Game Over panel disappears.
- [ ] Gameplay resumes.
- [ ] The player can take damage and trigger Game Over again.

---

## 5. Important Notes

- `HealthController` manages health; `GameOverController` manages the Game Over and respawn flow.
- `OnDeath` connects the two scripts without requiring `HealthController` to directly control the Game Over UI.
- `UpdateMaxHealth()` must be called by the relevant campfire or progression system when the campfire level changes.
- Respawn currently returns the player to the initial scene position, not a saved checkpoint.
- Respawn resets `Time.timeScale` to `1f`. If the project later supports other pause states, the pause system may need to coordinate this behavior.
- The current respawn implementation does not reload the scene or reset the world.