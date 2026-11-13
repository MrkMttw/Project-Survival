# Android Port — Mobile Support

## Overview

Project Survival is being adapted for Android devices while maintaining compatibility with the existing PC version.

The implementation follows a shared gameplay architecture with separate input methods and UI layouts.

**Architecture Goals:**
- One shared gameplay system.
- Separate PC and Mobile UI layouts.
- Keyboard and mouse support for PC.
- Touchscreen support for Android.
- No duplicated gameplay controllers or gameplay systems.

---

## 1. Android Development Environment

### Installed Components

The following Unity modules were installed through Unity Hub:

- Android Build Support
- Android SDK & NDK Tools
- OpenJDK

### Project Configuration

- Build Platform: Android
- Active Input Handling: Both
- Input System: Unity New Input System

---

## 2. Mobile HUD Integration

Separate mobile UI references were added to existing gameplay controllers.

The mobile UI displays the same gameplay information as the PC UI without duplicating gameplay systems.

### Implemented HUD Elements

| Script | Implementation |
|---|---|
| `HealthController.cs` | Added mobile health bar and health text references. |
| `HungerController.cs` | Added mobile hunger bar and hunger text references. |
| `PlayerMovement.cs` | Added a separate mobile sprint bar reference. |
| `GameClock.cs` | Added mobile clock and day counter references. |
| `CoordinateDisplay.cs` | Added mobile coordinate text reference. |
| `GameOverController.cs` | Added mobile respawn notification reference. |

### HUD Behavior

- PC and Mobile UI update simultaneously.
- Both layouts use the same gameplay values.
- Health, hunger, stamina, time, and coordinates remain synchronized.
- Existing gameplay calculations and mechanics are preserved.

---

## 3. Mobile Hotbar Positioning

**Modified Script:** `HotbarController.cs`

Added a mobile-specific positioning option.

### New Fields

- `isMobile` — Determines whether mobile positioning is enabled.
- `hotbarRect` — References the hotbar RectTransform.
- `originalPosition` — Stores the original hotbar position.

### Behavior

During initialization:

1. The original hotbar position is saved.
2. If `isMobile` is enabled, the horizontal anchored position is set to zero.
3. The original vertical position is preserved.

This allows the hotbar to be centered horizontally on mobile without changing its original PC positioning.

The existing inventory and crafting positioning behavior remains untouched.

---

## 4. Mobile Respawn Notification

**Modified Script:** `GameOverController.cs`

Added:

- `mobileRespawnNotificationText`

### Behavior

The PC and mobile notification displays:

- Show the same respawn message.
- Use the same notification duration.
- Appear simultaneously.
- Disappear simultaneously.

The existing respawn system remains unchanged.

---

## 5. Virtual Joystick Implementation

**Status:** Implemented and tested successfully.

The virtual joystick uses Unity's Input System and the built-in `OnScreenStick` component.

### 5.1 Joystick UI Structure

```text
Mobile_UI
└── TouchControls
    └── Joystick (Background)
        └── Handle
```

| Object | Responsibility |
|---|---|
| Background | Stationary outer joystick circle. |
| Handle | Movable inner joystick circle. |

### 5.2 OnScreenStick Configuration

The `OnScreenStick` component was added to the Handle GameObject.

| Setting | Configuration |
|---|---|
| Component | OnScreenStick |
| Control Path | `<Gamepad>/leftStick` |
| Movement Range | 50 |

The joystick generates virtual Gamepad Left Stick input.

### 5.3 Input Actions Integration

The existing `Move` action was reused.

Its existing bindings include:

- Gamepad Left Stick
- WASD
- Arrow Keys
- XR Controller
- Physical Joystick

No additional Move action was required.

### 5.4 PlayerInput Configuration

The existing Player GameObject uses the following configuration:

| Setting | Value |
|---|---|
| Actions | TopDownTemplate |
| Default Map | Player |
| Behavior | Invoke Unity Events |

The Move event is connected to:

```csharp
PlayerMovement.OnMove
```

The existing movement callback receives input from both keyboard and virtual joystick controls.

### 5.5 Joystick Testing Results

The following behaviors were tested successfully:

- Joystick handle responds to dragging.
- Player moves according to joystick direction.
- Diagonal movement works.
- Player stops when the joystick is released.
- Existing keyboard movement remains functional.

No separate mobile movement controller was created.

---

## 6. Mobile Sprint Button

**Modified Script:** `PlayerMovement.cs`

Added mobile sprint input while preserving the existing stamina and movement systems.

### New Input

- `mobileSprintHeld` — Tracks whether the mobile sprint button is being held.
- `OnMobileSprintPressed()` — Activates mobile sprint input.
- `OnMobileSprintReleased()` — Releases mobile sprint input.

### Sprint Behavior

| Input | Behavior |
|---|---|
| PC Left Shift | Sprint while held. |
| Android Sprint Button | Sprint while held. |
| Releasing Sprint | Returns to normal movement. |

### Button Configuration

The mobile Sprint Button uses Unity's Event Trigger component.

| Event | Method |
|---|---|
| Pointer Down | `PlayerMovement.OnMobileSprintPressed()` |
| Pointer Up | `PlayerMovement.OnMobileSprintReleased()` |
| Pointer Exit | `PlayerMovement.OnMobileSprintReleased()` |

### Preserved Mechanics

- Existing sprint multiplier.
- Sprint duration.
- Stamina regeneration.
- Exhaustion system.
- Sprint animation.
- PC sprint functionality.

The mobile sprint button reuses the existing stamina system.

---

## 7. Mobile Inventory Button

**Modified Script:** `MenuController.cs`

Added mobile inventory access and an explicit menu exit method.

### New Methods

- `OnInventoryButtonPressed()` — Opens the inventory through the existing inventory page logic.
- `ExitTab()` — Closes the menu canvas.
- `OpenInventoryPage()` — Opens the inventory page and resets the selected tab.

### Inventory Behavior

| Input | Result |
|---|---|
| PC B | Opens Inventory Page `[1]`. |
| Android Inventory Button | Opens Inventory Page `[1]`. |
| Exit Button | Closes the menu. |
| Settings Tab | Opens Settings Page `[2]`. |

### Tab Selection Integration

The existing `TabController.ActiveTab()` method is reused.

Opening the inventory now:

1. Activates the menu canvas.
2. Closes applicable furnace, container, and crafting interfaces.
3. Selects Inventory Page `[1]`.
4. Updates the inventory tab indicator.
5. Resets the selected tab to Inventory whenever the inventory shortcut is used.

The existing settings tab behavior remains functional.

No duplicate inventory UI or inventory logic was introduced.

---

## 8. Mobile Attack and Break Button

**Modified Script:** `ClickInteractionController.cs`

Added a dedicated mobile attack method and separated projectile input from melee and breaking input.

### New Method

```csharp
OnMobileAttackPressed()
```

The method reuses the existing melee and breaking interaction logic.

### Input Separation

| Input | Action |
|---|---|
| PC Left Click | Projectile shooting only. |
| PC Spacebar | Melee attack and breaking. |
| Android Basic Attack Button | Melee attack and breaking. |
| Android Directional Tap | Existing projectile shooting behavior. |

### Attack Priority

When a melee weapon is equipped:

1. Check for enemies within attack range.
2. Attack an enemy if one is available.
3. Otherwise, check for breakable objects.
4. Attempt breaking if a valid target exists.

When no melee weapon is equipped, the existing breaking system is used.

### Preserved Mechanics

- Existing projectile aiming and shooting.
- Melee weapon detection.
- Enemy attack priority.
- Object breaking.
- Menu restrictions.
- Game-over restrictions.

ProjectileWeaponController remains unchanged.

The PC and mobile controls share the same melee and breaking logic.

---

## 9. Mobile Interaction Button

**Modified Script:** `PlayerInteractionController.cs`

Added mobile interaction input and a contextual button highlight animation.

### New Method

- `OnMobileInteractionPressed()` — Executes the existing interaction logic.

### Interaction Systems

The existing interaction controller supports:

- Crafting interactions.
- Gate interactions.
- Furnace interactions.
- Container interactions.
- Sleeping bag interactions.
- Eating held items.
- Held-item light toggling.
- Items Generator interactions.

### Mobile Button Highlight

Added the following configurable fields:

- `mobileInteractionButton` — References the mobile button Image.
- `highlightColor` — Defines the interaction highlight color.
- `blinkSpeed` — Controls the animation speed.

### Highlight Behavior

- The button slowly transitions between its default color and highlight color.
- The animation uses a smooth back-and-forth color transition.
- The button returns to its default color when no E interaction is available.
- The animation does not modify the existing interaction mechanics.

### PromptController Integration

**Modified Script:** `PromptController.cs`

Updated the prompt registration system to support invisible prompts.

This allows prompt requests to remain functional without requiring a visual prompt GameObject.

Added:

- `IsPromptActive(PromptType type)` — Checks whether a prompt has active requesters.
- Support for prompt entries without assigned visual GameObjects.

The E prompt can now be used internally to control mobile interaction highlighting without displaying the original keyboard prompt.

The existing PC prompt system remains available.

---

## 10. Mobile Item Pickup Button

**Modified Script:** `PlayerItemCollector.cs`

Added:

```csharp
OnMobilePickupPressed()
```

The method reuses the existing `PickupItem()` functionality.

### Pickup Behavior

| Situation | Result |
|---|---|
| Nearby item detected | Mobile pickup button can collect it. |
| No nearby item | Nothing happens. |
| Stackable item | Collects available quantity. |
| Hotbar cannot accept item | Attempts inventory storage. |
| Both hotbar and inventory are full | Item remains on the ground. |
| Item is still animating | Pickup is ignored. |

### Preserved Mechanics

- Existing pickup radius.
- Closest-item detection.
- Hotbar priority.
- Inventory fallback.
- Stackable item handling.
- Loot persistence updates.
- Existing PC F-key functionality.

No separate mobile pickup system was created.

---

## 11. Mobile Building Direction Cycle Button

**Modified Script:** `PlacementController.cs`

Added mobile access to the existing directional building preset system.

### New Method

- `OnMobileCyclePressed()` — Calls the existing `CycleDirectionalPreset()` method.

### Directional Cycling

The mobile button reuses the existing directional preset implementation.

- Cycles through available directional building presets.
- Updates the ghost prefab.
- Preserves the current ghost position and rotation.
- Returns to the first preset after reaching the last preset.

### Contextual Button Visibility

Added:

- `mobileCycleButton` — References the mobile direction cycle button.

The button automatically appears when:

- A building ghost exists.
- An item is selected.
- The selected item uses directional prefabs.

The button automatically disappears when:

- No building is being placed.
- A non-directional building is selected.
- Placement is cancelled.
- Placement is completed.

The existing PC R-key functionality remains unchanged.

---

## 12. Mobile UI Placement Protection

**Modified Script:** `PlacementController.cs`

Added a configurable UI exclusion system to prevent mobile UI interactions from accidentally placing buildings.

### New Field

- `ignoreUIPanels` — An array of UI GameObjects that should block building placement when interacted with.

### UI Detection

The system uses:

- `EventSystem`
- `PointerEventData`
- `RaycastResult`
- `EventSystem.RaycastAll()`

The placement system checks whether the pointer is over any configured ignored UI element before processing a placement click.

### Configurable UI Exclusion

The following UI elements can be registered in the Inspector:

- Attack button.
- Interaction button.
- Pickup button.
- Sprint button.
- Inventory button.
- Direction cycle button.
- Joystick background.
- Joystick handle.
- Other mobile UI panels.

Child elements of registered parent GameObjects are also ignored.

This prevents mobile button interactions from unintentionally triggering building placement.

---

## 13. Current Implementation Summary

| Feature | Status |
|---|---|
| Android Build Support | Installed |
| Android SDK & NDK | Installed |
| OpenJDK | Installed |
| Mobile Health UI | Implemented |
| Mobile Hunger UI | Implemented |
| Mobile Sprint UI | Implemented |
| Mobile Day Counter | Implemented |
| Mobile Clock Display | Implemented |
| Mobile Coordinates | Implemented |
| Mobile Hotbar Positioning | Implemented |
| Mobile Respawn Notification | Implemented |
| Virtual Joystick | Implemented and tested |
| Mobile Sprint Button | Implemented |
| Mobile Inventory Button | Implemented and tested |
| Mobile Attack / Break Button | Implemented and tested |
| Mobile Interaction Button | Implemented |
| Mobile Pickup Button | Implemented |
| Mobile Direction Cycle Button | Implemented |
| Contextual Cycle Button Visibility | Implemented |
| Invisible Prompt Support | Implemented |
| Mobile UI Placement Protection | Implemented |
| Existing PC Keyboard Controls | Preserved |
| Existing Projectile Controls | Preserved |

---

## 14. Remaining Implementation Tasks

### Touch Controls

- [ ] Grid Toggle Button — Tab equivalent.
- [ ] Mobile Hotbar Selection — Slots 1–7.
- [ ] Verify contextual interaction button highlighting.
- [ ] Verify pickup button behavior on Android.
- [ ] Verify directional cycle button visibility and interaction.

### Gameplay Integration

- [ ] Inventory and crafting touchscreen interactions.
- [ ] Settings and menu touchscreen interactions.
- [ ] Container touchscreen interactions.
- [ ] Verify all interaction prompts and mobile button highlights.
- [ ] Verify building placement and relocation controls.
- [ ] Verify mobile UI does not interfere with gameplay input.

### Android Testing and Optimization

- [ ] Test different Android screen resolutions.
- [ ] Test different aspect ratios.
- [ ] Verify inventory and container interactions.
- [ ] Verify building placement.
- [ ] Verify combat and object breaking.
- [ ] Test sprinting and movement.
- [ ] Test pickup and interaction controls.
- [ ] Profile Android performance.
- [ ] Perform final Android build testing.

---

## 15. Development Architecture

Project Survival uses one shared gameplay architecture with separate platform inputs and UI layouts.

```text
PC INPUT                    ANDROID INPUT
Keyboard / Mouse            Virtual Joystick / Touch Buttons
       |                            |
       +-------------+--------------+
                     |
              SHARED GAMEPLAY
                     |
       +-------------+--------------+
       |                            |
     PC UI                       MOBILE UI
```

### Architecture Principles

1. Gameplay systems remain shared between platforms.
2. Mobile controls reuse existing gameplay functionality.
3. UI references are separated where different layouts are required.
4. Existing PC controls and gameplay behavior are preserved.
5. No duplicate inventory, health, hunger, or world systems are introduced.
6. Platform-specific input methods call existing gameplay functionality.
7. Contextual mobile controls only appear when their associated actions are available.

**Current Development Stage:** Android Touch Controls Implementation

**Next Milestone:** Grid Toggle Button and Mobile Hotbar Selection.