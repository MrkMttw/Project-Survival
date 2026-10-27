# Item Generator System

## Overview

The Item Generator is a building feature in Project Survival that generates physical item drops over time. The player activates the generator through interaction, waits for the generation timer to finish, and collects the spawned item.

Each activation produces one generation cycle. The generator automatically stops after attempting to generate an item and must be activated again to begin another cycle.

## Features

- **E-key interaction:** Activate the generator when the player is within its interaction collider.
- **Single-cycle generation:** Each activation runs one timer and attempts to generate one item drop.
- **No repeated activation:** Pressing E while the generator is working does not interrupt or restart the cycle.
- **Configurable item pool:** Select which items the generator can produce.
- **Random quantity:** Configure minimum and maximum quantities for generated item stacks.
- **Configurable delay:** Set the generation time in seconds.
- **Vibration animation:** The generator visually vibrates while active and returns to its original position when inactive.
- **Building save-state integration:** The generator stores its active state and remaining timer through `BuildingSaveData`.

## Scripts

### `ItemsGeneratorController.cs`

Handles player interaction with supported generator buildings.

Responsibilities:
- Finds generator behaviors in the active scene.
- Checks whether the player is inside the building's `BoxCollider2D`.
- Validates the building through its `BuildingObject`, item ID, and supported prefab list.
- Calls the generator's activation method when interaction is valid.

### `ItemsGeneratorBehavior.cs`

Controls the generator's behavior and generation cycle.

Responsibilities:
- Stores generation settings.
- Tracks whether the generator is active.
- Counts down the generation timer using real-time seconds.
- Randomly selects an item and quantity.
- Spawns the generated item at the generator's position.
- Stops after one generation attempt.
- Saves the active state and remaining timer through `WorldGenerator`.
- Applies the vibration animation while active.

### `BuildingSaveData.cs`

Stores generator state alongside other building data.

Generator-related fields:
- `generatorIsActive`: Whether the generator was active when its state was saved.
- `generatorRemainingTime`: The remaining generation time.

These fields support runtime state restoration through the existing world-building system.

## Configuration

Select the generator building prefab and configure its `ItemsGeneratorBehavior` component.

| Setting | Description |
|---|---|
| Possible Items | Items that the generator can produce |
| Minimum Quantity | Minimum number of items per generated stack |
| Maximum Quantity | Maximum number of items per generated stack |
| Generation Delay | Time in seconds required for one generation cycle |
| Is Active | Current generator state |
| Vibration Amount | Maximum positional offset used by the vibration animation |
| Vibration Speed | Controls the speed of the vibration animation |

The `ItemsGeneratorController` is attached to the global `GameController`.

Configure its references:
- **Player:** The player's Transform.
- **World Generator:** The scene's `WorldGenerator`.
- **Generator Building Prefabs:** The building prefabs allowed to use the generator interaction.

Each supported generator building must have the required components:
- `ItemsGeneratorBehavior`
- `BuildingObject`
- `BoxCollider2D`

The building must also be registered correctly in the project's item system.

## Generation Cycle

1. The player approaches the generator.
2. The player presses E.
3. The generator becomes active and starts its timer.
4. While active, the timer counts down and the generator vibrates.
5. When the timer reaches zero, the generator attempts to spawn an item stack.
6. The generator becomes inactive and stops vibrating.
7. The player can press E again to start another cycle.

Pressing E while the generator is already active has no effect.

## Item Generation Logic

The generator randomly selects an entry from `possibleItems`.

The quantity is randomly selected between `minimumQuantity` and `maximumQuantity`, inclusive.

The selected item is cloned using `Item.CloneItem(quantity)` and spawned at the generator's position. The item is physically dropped into the world rather than added directly to the player's inventory.

If the item pool is empty or the selected entry is null, the generation attempt does not produce an item. The cycle still ends and the generator becomes inactive.

## Interaction Priority

The player's E-key interaction system checks actions in this order:

1. Sleeping bag interaction
2. Eating held food
3. Toggling a held item's light
4. Item generator interaction

If an earlier interaction succeeds, the later interactions are skipped.

## Testing Checklist

- [ ] Press E near an inactive generator to activate it.
- [ ] Confirm the generator vibrates while active.
- [ ] Press E while it is working and confirm the cycle continues uninterrupted.
- [ ] Wait for the timer to finish.
- [ ] Confirm the generator attempts to spawn one item stack.
- [ ] Confirm the generator stops and returns to its original position.
- [ ] Press E again and confirm another cycle begins.
- [ ] Verify the generated quantity respects the configured minimum and maximum.
- [ ] Test unloading and reloading the building's chunk to check state restoration.

## Current Limitations

- The generator produces one item stack per activation.
- The generator only processes while its GameObject is active in the scene.
- State is stored through the existing runtime world-building save system; this does not by itself provide permanent disk saving.
- A generation attempt can finish without spawning an item if the configured item pool is invalid.
- The vibration animation modifies the generator's local position. Other systems that control the same Transform may require coordination.