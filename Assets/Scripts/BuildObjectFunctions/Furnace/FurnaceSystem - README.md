# Furnace System

## Overview

The Furnace System is a Minecraft-style smelting system for Project Survival, a 2D top-down survival game developed in Unity.

It allows players to place items into a furnace, supply fuel, and convert smeltable items into new items using configurable smelting recipes. The system reuses the existing item, crafting recipe, and world persistence systems.

## Features

- **Input Slot:** Accepts items marked as smeltable.
- **Fuel Slot:** Accepts items marked as fuel.
- **Output Slot:** Displays the results of completed smelting recipes.
- **Configurable Smelting Time:** Each smeltable item has its own smelting duration.
- **Configurable Fuel Duration:** Each fuel item has its own burn duration.
- **Recipe-Based Smelting:** Uses `CraftingRecipe` ScriptableObjects to define inputs and outputs.
- **Live Progress UI:** Displays fuel and smelting progress.
- **Cook Button:** Allows the player to start furnace processing.
- **Runtime State Persistence:** Preserves furnace contents and processing state through the existing `WorldGenerator` building data.
- **Chunk-Based Processing:** Furnaces process only while their world chunk is loaded.
- **Reusable Configuration:** New smelting recipes can be added through the Unity Inspector without creating new code for each recipe.

## Requirements

The furnace system uses the following existing project components:

- `Item`
- `ItemDictionary`
- `CraftingRecipe`
- `CraftingController`
- `BuildingObject`
- `BuildingSaveData`
- `WorldGenerator`
- `Slot`
- `FurnaceSlot`
- `FurnaceBehavior`
- `FurnaceController`
- `ItemDragHandler`

The furnace also requires a `BoxCollider2D` for player interaction.

## Architecture

### FurnaceBehavior

Responsible for the furnace's core functionality:

- Managing input, fuel, and output item IDs and quantities.
- Finding matching smelting recipes.
- Consuming fuel and processing smeltable items.
- Tracking smelting progress and remaining fuel duration.
- Updating furnace state in `BuildingSaveData`.
- Loading and synchronizing furnace slot contents.
- Exposing progress information to the furnace UI.

### FurnaceController

Responsible for the furnace interface:

- Opening and closing the furnace UI.
- Detecting nearby furnaces.
- Loading furnace contents into the UI slots.
- Starting cooking through the Cook button.
- Updating the fuel and smelting progress bars.
- Refreshing the displayed slots when furnace contents change.
- Resizing the hotbar while the furnace UI is open.

### FurnaceSlot

Determines which items each slot accepts:

| Slot Type | Accepted Items |
|---|---|
| Input | Items with `isSmeltable` enabled |
| Fuel | Items with `isFuel` enabled |
| Output | No incoming items |

### CraftingRecipe

Defines the input item, output item, and output quantity for each smelting recipe.

### BuildingSaveData

Stores furnace runtime state alongside the existing building data managed by `WorldGenerator`.

## Item Configuration

The `Item` component contains the following furnace-related fields.

| Field | Description |
|---|---|
| `isSmeltable` | Determines whether an item can be smelted. |
| `smeltingTime` | Time in seconds required to smelt one input item. |
| `isFuel` | Determines whether an item can be used as fuel. |
| `fuelBurnDuration` | Duration in seconds that one fuel item burns. |

### Example Configuration

| Item | Configuration |
|---|---|
| Raw Iron | `isSmeltable = true`; configure `smeltingTime`. |
| Iron Ingot | Configure as the output item in the smelting recipe. |
| Wood | `isFuel = true`; configure `fuelBurnDuration`. |

These values are configured on the respective item prefabs in the Unity Inspector.

## Creating a Smelting Recipe

Smelting recipes use the existing `CraftingRecipe` ScriptableObject.

### Step 1: Create the Recipe

In the Unity Project window, right-click and select:

`Create > Crafting > Crafting Recipe`

Give the asset a descriptive name, such as `RawIron_SmeltingRecipe`.

### Step 2: Configure the Recipe

Set the following fields:

| Field | Example Value |
|---|---|
| `isSmeltingRecipe` | Enabled |
| `ingredients` | Size: 1 |
| `ingredients[0].itemPrefab` | Raw Iron prefab |
| `ingredients[0].amount` | 1 |
| `itemPrefab` | Iron Ingot prefab |
| `craftingQuantity` | 1 |

This configuration converts one raw iron item into one iron ingot.

### Step 3: Register the Recipe

1. Select the GameObject containing `CraftingController`.
2. Locate its `recipeLevels` list.
3. Expand an existing level.
4. Add the new recipe asset to that level's `recipes` list.

The current furnace implementation searches all configured recipe levels for matching recipes marked with `isSmeltingRecipe`. The recipe's crafting level is not currently used as a furnace requirement.

## Unity UI Setup

The `FurnaceController` requires the following references to be assigned in the Inspector.

### References

- `player`: The player's Transform.
- `furnacePanel`: The furnace UI panel.

### Furnace Slots

- `inputSlot`: Input slot's `Slot` component.
- `fuelSlot`: Fuel slot's `Slot` component.
- `outputSlot`: Output slot's `Slot` component.

Ensure the corresponding slot objects have the appropriate `FurnaceSlot` configuration.

### Progress UI

- `fuelFill`: Image displaying remaining fuel progress.
- `arrowFill`: Image displaying smelting progress.

Set both progress Images to **Image Type: Filled** and configure their fill method and direction to match the UI artwork.

### Hotbar Resize

- `hotbarRect`: The hotbar's RectTransform.
- `furnaceOpenHotbarScale`: The scale applied while the furnace UI is open.

### Cook Button

Connect the Cook button's `Button > On Click()` event to:

`FurnaceController.StartCooking()`

The button starts processing for the currently open furnace.

## Smelting Process

1. The player interacts with a nearby furnace by pressing the interaction key.
2. The furnace UI opens and restores its stored slot contents.
3. The player places a valid smeltable item in the input slot.
4. The player places a valid fuel item in the fuel slot.
5. The player presses Cook.
6. The furnace consumes fuel as needed and tracks the smelting progress.
7. When the smelting duration is reached, one input item is consumed and the recipe output is added to the output slot.
8. The process continues while cooking is enabled and the required resources and output capacity are available.

Fuel can be consumed even when the furnace has no input item. If fuel runs out during smelting, the partial smelting progress is retained until processing can continue.

If the output slot cannot accept the recipe result, processing pauses to avoid consuming additional input for an unavailable output.

## Runtime Persistence and Chunk Behavior

Furnace state is stored in the existing `BuildingSaveData` associated with each building.

The stored fields include:

- Input item ID and quantity.
- Fuel item ID and quantity.
- Output item ID and quantity.
- Cooking state.
- Smelting progress.
- Remaining fuel duration.

The furnace updates this data through `WorldGenerator.TryGetBuildingSaveData()`.

### Chunk Loading

Furnace processing uses `Time.deltaTime` while the furnace GameObject is active and its world chunk is loaded.

- Loaded chunk: Processing can continue.
- Unloaded chunk: Processing pauses when the furnace is unloaded.
- Chunk loaded again: The furnace restores its stored state and can resume processing.

This is runtime persistence through the existing world-building data. It is not a complete disk-based save system for furnace state.

## Current Limitations

- Only recipes with exactly one ingredient are supported by the current furnace implementation.
- Furnace fuel is configured through item properties; wood is the initial intended fuel.
- Furnace experience rewards are not implemented.
- Furnace state is stored in runtime world data; persistence across closing and reopening the entire game depends on a separate disk save system.
- Processing is not intended to continue while a chunk is unloaded.
- The current output capacity check pauses processing when the next complete recipe result cannot fit.
- The current implementation does not enforce `requiredCampfireLevel` as a furnace requirement.

## Testing Checklist

- [ ] Furnace opens and closes correctly.
- [ ] Input accepts smeltable items only.
- [ ] Fuel accepts fuel items only.
- [ ] Output rejects incoming dragged items.
- [ ] Cook button starts furnace processing.
- [ ] Fuel duration follows the configured fuel item.
- [ ] Smelting duration follows the configured input item.
- [ ] Fuel and smelting progress bars update correctly.
- [ ] Completed recipes consume input and produce the configured output.
- [ ] Multiple input items can be processed in sequence.
- [ ] Processing pauses when fuel is unavailable.
- [ ] Processing pauses when output capacity is insufficient.
- [ ] Slot contents remain correct after closing and reopening the furnace UI.
- [ ] Smelting progress is retained when fuel runs out.
- [ ] Furnace processing pauses when its chunk unloads.
- [ ] Furnace state resumes correctly when its chunk loads again.

## Future Improvements

Potential extensions include:

- Disk-based persistence for furnace state.
- Support for recipes with multiple ingredients.
- Additional fuel types.
- Furnace audio and visual effects.
- Improved output claiming and inventory transfer behavior.
- Optional furnace experience rewards.
- More detailed status indicators for unavailable fuel, invalid recipes, and blocked output.

## Summary

The Furnace System provides configurable, recipe-driven smelting while reusing Project Survival's existing item definitions, crafting recipes, building data, and chunk-loading architecture. Its main behavior is controlled by `FurnaceBehavior`, while `FurnaceController` manages the player-facing interface.