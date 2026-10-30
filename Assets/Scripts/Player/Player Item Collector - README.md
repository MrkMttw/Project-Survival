# Player Item Collector - README

This system allows the player to pick up nearby items by pressing **F**.

## Features

* Picks up items within a configurable radius.
* Automatically targets the **closest item**.
* Press **F** to collect the selected item.
* Tries to add the item to the **Hotbar first**.
* If the Hotbar is full, tries the **Inventory** instead.
* The world item is only destroyed after it is successfully added.
* Shows the pickup UI when an item is within range.

## Setup

1. Add `PlayerItemCollector.cs` to the Player.
2. Create an **Item** Layer.
3. Set your item prefabs to the **Item** Layer.
4. Assign the **Item Layer** in the `PlayerItemCollector` Inspector.
5. Assign the `pickUp` UI GameObject.
6. Adjust `Pickup Radius` to control how close an item needs to be.

### Default Controls

| Action       | Key   |
| ------------ | ----- |
| Pick Up Item | **F** |

## Important

The pickup system uses a **radius check** instead of requiring the player's collider to physically touch the item. This makes item collection more responsive and prevents the player from having to reposition themselves just to pick up an item.
