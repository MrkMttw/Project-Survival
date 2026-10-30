# Crafting System

The crafting system allows the player to view available recipes, see their required ingredients, check whether they are unlocked by the campfire level, and craft items using materials from both the Inventory and Hotbar.

The crafting UI is divided into two sides:

* **Left Side:** Crafting level and recipe list
* **Right Side:** Selected recipe details and crafting requirements

---

# 1. Crafting System Overview

The crafting system uses these main scripts:

```text
CraftingController
CraftingRecipe
CraftingLevel
RecipeButtonUI
InventoryController
HotbarController
CampfireController
```

The general flow is:

```text
Campfire Level
      ↓
CraftingController
      ↓
Crafting Level
      ↓
Recipe Button
      ↓
Select Recipe
      ↓
Recipe Details
      ↓
Check Inventory + Hotbar
      ↓
Remove Ingredients
      ↓
Create Crafted Item
      ↓
Add Crafted Item to Hotbar
```

---

# 2. Crafting UI Layout

The crafting UI uses two main sections.

```text
CraftingUI
├── LeftPanel
│   └── Scroll View
│       ├── Viewport
│       │   └── Content
│       └── Scrollbar Vertical
│
└── RightPanel
    ├── RecipeDetails
    │   ├── RecipeIcon
    │   ├── ItemName
    │   ├── CraftingQuantity
    │   ├── RecipeText
    │   ├── DescriptionDetails
    │   └── CraftButton
```

The exact visual hierarchy can be adjusted depending on the UI design, but the important references must be assigned to the `CraftingController`.

---

# 3. Create the CraftingController

Create a GameObject for the crafting system.

For example:

```text
CraftingUI
└── CraftingController
```

Add:

```text
CraftingController
```

to the GameObject.

The `CraftingController` contains the recipe list, left-side UI references, and right-side recipe detail references.

---

# 4. CraftingController Inspector

The `CraftingController` contains the following sections.

## Recipe List

```text
Recipe Levels
```

This is a list of `CraftingLevel` entries.

Each entry contains:

```text
Level
Recipes
```

Example:

```text
Recipe Levels
├── Element 0
│   Level: 0
│   Recipes:
│   ├── Recipe: Wooden Axe
│   └── Recipe: Wooden Pickaxe
│
├── Element 1
│   Level: 1
│   Recipes:
│   ├── Recipe: Stone Axe
│   └── Recipe: Stone Pickaxe
│
└── Element 2
    Level: 2
    Recipes:
    ├── Recipe: Iron Axe
    └── Recipe: Iron Pickaxe
```

The recipes are grouped under their campfire level.

---

# 5. Left Side References

Assign the following fields:

```text
Content Panel
Crafting Level Button Prefab
Recipe Button Prefab
```

### Content Panel

Assign:

```text
Scroll View → Viewport → Content
```

This is where the `CraftingController` automatically creates the crafting level headers and recipe buttons.

Do not manually create every recipe button in the final UI.

---

# 6. Right Side References

Assign the following:

```text
Recipe Details
Recipe Icon
Item Name
Crafting Quantity
Recipe Text
Description Details
```

These are used when the player selects a recipe.

For example:

```text
Recipe Details
├── Recipe Icon
├── Item Name
├── Crafting Quantity
├── Recipe Text
├── Description Details
└── Craft Button
```

The `Recipe Details` object starts disabled/hidden.

The `CraftingController` automatically hides it when the crafting system starts.

When the player selects a recipe, it becomes visible.

---

# 7. CraftingRecipe

`CraftingRecipe` is a ScriptableObject.

Create one by right-clicking in the Project window:

```text
Create
→ Crafting
→ Crafting Recipe
```

A recipe contains:

```text
Crafted Item
├── Item Prefab
├── Recipe Icon
└── Crafting Quantity

Ingredients
└── Ingredient List

Recipe Info
├── Description
└── Required Campfire Level
```

---

# 8. CraftingRecipe Setup

## Item Prefab

Assign the actual item prefab that will be created when crafting.

Example:

```text
Item Prefab: Wooden Axe
```

The prefab should contain the normal `Item` component.

## Recipe Icon

Assign the image that should appear in the recipe button and recipe details.

## Crafting Quantity

This determines how many items are created.

Example:

```text
Crafting Quantity: 1
```

or:

```text
Crafting Quantity: 4
```

If the quantity is `4`, crafting the recipe produces four items.

---

# 9. Ingredients

Expand the `Ingredients` list.

Each ingredient contains:

```text
Item Prefab
Amount
```

Example:

```text
Ingredients
├── Element 0
│   Item Prefab: Wood
│   Amount: 5
│
└── Element 1
    Item Prefab: Stone
    Amount: 2
```

The crafting system identifies ingredients using the `Item.ID` from their prefab.

---

# 10. Recipe Description

The `Description` field contains the text shown on the right side of the crafting UI.

Example:

```text
A basic wooden tool.
```

This field supports multiple lines.

---

# 11. Required Campfire Level

The `Required Campfire Level` determines when the recipe becomes available.

Example:

```text
Required Campfire Level: 2
```

If the player's campfire is below level 2, the recipe button is locked.

Once the campfire reaches level 2, the recipe becomes unlocked.

The crafting system checks:

```text
CampfireLevel >= RequiredCampfireLevel
```

---

# 12. CraftingLevel

`CraftingLevel` is a serializable class rather than a ScriptableObject.

It is configured directly inside the `CraftingController`.

Each entry contains:

```text
Level
Recipes
```

Example:

```text
CraftingController
└── Recipe Levels
    ├── Level 0
    │   ├── Wooden Axe
    │   └── Wooden Pickaxe
    │
    ├── Level 1
    │   ├── Stone Axe
    │   └── Stone Pickaxe
    │
    └── Level 2
        └── Iron Pickaxe
```

The level value is used to create the level header:

```text
Campfire Level 0
Campfire Level 1
Campfire Level 2
```

---

# 13. Recipe Button Prefab

Create one recipe button prefab that will be used for every recipe.

The prefab needs:

```text
RecipeButton
├── Icon
└── Item Name
```

The root object must contain:

```text
Button
RecipeButtonUI
```

The `RecipeButtonUI` component contains:

```text
Icon
Item Name
```

Assign:

* `Icon` → the recipe icon Image
* `Item Name` → the TextMeshPro text displaying the item name

The button does not need to be manually connected to a crafting recipe.

The `CraftingController` assigns the recipe automatically when it generates the list.

---

# 14. How Recipe Buttons Work

When the `CraftingController` generates a recipe button, it calls:

```text
RecipeButtonUI.Setup()
```

The button receives:

* Recipe
* Recipe icon
* Item name
* Unlock state

The button then automatically creates its click event.

When clicked:

```text
Recipe Button
      ↓
RecipeButtonUI
      ↓
CraftingController.SelectRecipe()
      ↓
Display Recipe Details
```

---

# 15. Recipe Locking

Recipes are locked using the campfire level.

For example:

```text
Current Campfire Level: 1

Recipe A
Required Level: 0
→ UNLOCKED

Recipe B
Required Level: 1
→ UNLOCKED

Recipe C
Required Level: 2
→ LOCKED
```

The button's `interactable` property is used to control whether the player can select the recipe.

---

# 16. Campfire Level Integration

The crafting system finds the global:

```text
CampfireController
```

and reads:

```text
CampfireLevel
```

The `CampfireController` remains responsible for the actual campfire progression.

The crafting system only reads the level and uses it to determine which recipes are unlocked.

When the campfire levels up, `CampfireController` calls:

```text
CraftingController.RefreshRecipes()
```

This immediately updates the recipe buttons.

For example:

```text
Campfire Level 1
      ↓
Level 2 reached
      ↓
RefreshRecipes()
      ↓
Level 2 recipes become unlocked
```

---

# 17. Creating the Scrollable Recipe List

The left side uses a Unity Scroll View.

Hierarchy:

```text
CraftingUI
└── LeftPanel
    └── Scroll View
        ├── Viewport
        │   └── Content
        └── Scrollbar Vertical
```

## Add the Scroll View

Right-click `LeftPanel`:

```text
UI → Scroll View
```

Rename it:

```text
Scroll View
```

Make the Scroll View fill the entire LeftPanel.

Set its RectTransform:

```text
Left: 0
Right: 0
Top: 0
Bottom: 0
```

---

# 18. Configure Content

Select:

```text
Scroll View
→ Viewport
→ Content
```

Add:

```text
Vertical Layout Group
```

Set:

```text
Child Alignment: Upper Center
Spacing: 5

Control Child Size
├── Width: ON
└── Height: OFF
```

The height should remain controlled by each individual button.

Add:

```text
Content Size Fitter
```

Set:

```text
Horizontal Fit: Unconstrained
Vertical Fit: Preferred Size
```

This allows Content to become taller as more recipe buttons are created.

---

# 19. Test the Scroll View

Before using the actual crafting buttons, create a temporary button.

Right-click:

```text
Content
→ UI
→ Button - TextMeshPro
```

Rename it:

```text
RecipeButton_Test
```

Set:

```text
Width: 200
Height: 50
```

Duplicate it several times.

Example:

```text
Content
├── RecipeButton_Test
├── RecipeButton_Test
├── RecipeButton_Test
├── RecipeButton_Test
├── RecipeButton_Test
├── RecipeButton_Test
├── RecipeButton_Test
└── RecipeButton_Test
```

If the buttons extend beyond the visible Viewport, the Scroll View should allow vertical scrolling.

Remove the temporary buttons after testing.

---

# 20. Important Scroll View Setting

Select:

```text
Scroll View
```

Check the `Scroll Rect` component.

Set:

```text
Content: Content
Viewport: Viewport

Horizontal: OFF
Vertical: ON
```

The vertical scrollbar should also be assigned:

```text
Vertical Scrollbar: Scrollbar Vertical
```

The important references are:

```text
Scroll Rect
├── Content → Content
├── Viewport → Viewport
└── Vertical Scrollbar → Scrollbar Vertical
```

---

# 21. Viewport

Select:

```text
Scroll View → Viewport
```

The Viewport should contain:

```text
Rect Mask 2D
```

This keeps recipe buttons from appearing outside the visible crafting area.

The Viewport should fill the Scroll View.

---

# 22. Final Scroll View Structure

The final structure should be similar to:

```text
LeftPanel
└── Scroll View
    ├── Viewport
    │   └── Content
    │       ├── Campfire Level Button
    │       ├── Recipe Button
    │       ├── Recipe Button
    │       ├── Campfire Level Button
    │       ├── Recipe Button
    │       └── Recipe Button
    │
    └── Scrollbar Vertical
```

The `CraftingController` creates these objects automatically.

Do not manually create a button for every recipe.

---

# 23. Selecting a Recipe

When the player clicks an unlocked recipe:

```text
Recipe Button
      ↓
SelectRecipe()
      ↓
selectedRecipe is assigned
      ↓
Recipe Details becomes visible
```

The right side is then filled with:

```text
Recipe Icon
Item Name
Crafting Quantity
Description
Ingredients
```

Example:

```text
┌─────────────────────────────┐
│       [ Wooden Axe ]        │
│                             │
│       Wooden Axe            │
│            x1               │
│                             │
│  Wood x5                    │
│  Stone x2                   │
│                             │
│  A basic wooden tool.       │
│                             │
│          [ CRAFT ]          │
└─────────────────────────────┘
```

---

# 24. Ingredient Colors

When a recipe is selected, the crafting system checks the player's:

```text
Inventory
+
Hotbar
```

The total quantity of each ingredient is calculated.

If enough materials are available:

```text
Wood x5
```

is displayed in green.

If there are not enough:

```text
Wood x5
```

is displayed in red.

The calculation is:

```text
Inventory Amount + Hotbar Amount
```

Example:

```text
Inventory:
Wood x3

Hotbar:
Wood x2

Required:
Wood x5

Total:
3 + 2 = 5

Result:
Enough
```

---

# 25. Crafting

When the player presses the Craft button:

```text
CraftSelectedRecipe()
```

is called.

The system first checks every ingredient.

This happens before removing anything.

Example:

```text
Wood x5
Stone x2
Iron x1
```

If all requirements are met, crafting continues.

If even one ingredient is missing, nothing is removed.

This prevents partial ingredient consumption.

---

# 26. Ingredient Removal Priority

Ingredients are removed in this order:

```text
HOTBAR
   ↓
INVENTORY
```

Example:

```text
Required Wood: 5

Hotbar:    Wood x3
Inventory: Wood x4
```

The crafting system removes:

```text
Hotbar:
3 Wood

Inventory:
2 Wood
```

Total removed:

```text
5 Wood
```

This keeps the Hotbar as the first source for crafting materials.

---

# 27. Creating the Crafted Item

After the ingredients are successfully removed, the system creates a temporary copy of the recipe's item prefab.

The quantity is set using:

```text
Crafting Quantity
```

For example:

```text
Crafting Quantity: 4
```

creates:

```text
Item x4
```

The crafted item is then added to the Hotbar.

After being added, the temporary object is destroyed.

The Hotbar handles the actual stack creation.

---

# 28. Crafted Item Destination

Crafted items currently go directly to:

```text
Hotbar
```

The `HotbarController.AddItem()` method handles:

* Existing stacks
* New stacks
* Maximum stack size
* Empty slots

The current crafting system does not automatically move crafted items to the Inventory if the Hotbar is full.

---

# 29. Inventory and Hotbar Requirements

The crafting system uses:

```text
InventoryController
```

and:

```text
HotbarController
```

Both systems need to exist in the scene.

The Inventory provides:

```text
GetItemQuantity()
RemoveItem()
```

The Hotbar provides:

```text
GetItemQuantity()
RemoveItem()
AddItem()
```

These methods allow crafting to treat the Inventory and Hotbar as two separate storage areas while checking both for materials.

---

# 30. Hotbar Must Stay Active

The Hotbar must remain active while the Crafting page is open.

The crafting system searches for:

```text
HotbarController
```

and uses it to:

* Check ingredient quantities
* Remove ingredients
* Add crafted items

Therefore, disabling the entire Hotbar GameObject while the crafting UI is open will prevent crafting from working correctly.

The Hotbar UI can remain visible or be positioned according to the menu design, but its GameObject must remain active.

---

# 31. Example Recipe Setup

Example recipe:

```text
Recipe: Wooden Axe
```

Set:

```text
Item Prefab:
Wooden Axe

Recipe Icon:
Wooden Axe Sprite

Crafting Quantity:
1

Required Campfire Level:
1
```

Ingredients:

```text
Wood
Amount: 5

Stone
Amount: 2
```

Description:

```text
A basic wooden axe.
```

Then add the recipe to:

```text
CraftingController
→ Recipe Levels
→ Campfire Level 1
→ Recipes
```

---

# 32. Example Crafting Level Setup

Example:

```text
Recipe Levels

Element 0
Level: 0
Recipes:
- Wooden Axe
- Wooden Pickaxe

Element 1
Level: 1
Recipes:
- Stone Axe
- Stone Pickaxe

Element 2
Level: 2
Recipes:
- Iron Axe
- Iron Pickaxe
```

When the crafting UI opens, the system automatically generates:

```text
Campfire Level 0

Wooden Axe
Wooden Pickaxe

Campfire Level 1

Stone Axe
Stone Pickaxe

Campfire Level 2

Iron Axe
Iron Pickaxe
```

Locked recipes remain unclickable until the campfire reaches the required level.

---

# 33. Final System Flow

The complete system works like this:

```text
Crafting UI Opens
        ↓
CraftingController.Start()
        ↓
Find CampfireController
        ↓
Generate Recipe List
        ↓
Create Level Headers
        ↓
Create Recipe Buttons
        ↓
Check Campfire Level
        ↓
Lock / Unlock Recipes
```

When a recipe is selected:

```text
Recipe Button
        ↓
Select Recipe
        ↓
Display Recipe Details
        ↓
Check Inventory
        ↓
Check Hotbar
        ↓
Display Ingredients
        ↓
Green / Red Requirements
```

When crafting:

```text
Craft Button
        ↓
Check ALL Ingredients
        ↓
Enough?
   ┌────┴────┐
   No        Yes
   ↓          ↓
Stop     Remove Hotbar Items
              ↓
       Remove Inventory Items
              ↓
        Create Crafted Item
              ↓
          Add to Hotbar
              ↓
       Destroy Temporary Item
```

---

# 34. Important Objects and Responsibilities

## CraftingController

Responsible for:

* Generating the recipe list
* Creating level headers
* Creating recipe buttons
* Selecting recipes
* Displaying recipe information
* Checking ingredient requirements
* Removing ingredients
* Creating crafted items
* Refreshing recipe unlock states

## CraftingRecipe

Stores:

* Crafted item
* Recipe icon
* Crafting quantity
* Ingredients
* Description
* Required campfire level

## CraftingLevel

Groups recipes by campfire level.

## RecipeButtonUI

Controls an individual recipe button.

Responsible for:

* Recipe icon
* Item name
* Locked/unlocked state
* Selecting the recipe

## InventoryController

Responsible for inventory quantities and removing ingredients from inventory.

## HotbarController

Responsible for hotbar quantities, removing crafting ingredients from the hotbar, and receiving crafted items.

## CampfireController

Responsible for:

* Campfire level
* Campfire progression
* Wood
* Fuel
* Healing
* Updating crafting recipe unlocks after leveling up

---

# 35. Final Checklist

Before testing the crafting system, make sure:

### Crafting Controller

```text
[ ] CraftingController exists
[ ] Recipe Levels are configured
[ ] Content Panel assigned
[ ] Crafting Level Button Prefab assigned
[ ] Recipe Button Prefab assigned
[ ] Recipe Details assigned
[ ] Recipe Icon assigned
[ ] Item Name assigned
[ ] Crafting Quantity assigned
[ ] Recipe Text assigned
[ ] Description Details assigned
```

### Recipe Assets

```text
[ ] CraftingRecipe assets created
[ ] Item Prefab assigned
[ ] Recipe Icon assigned
[ ] Crafting Quantity set
[ ] Ingredients assigned
[ ] Description written
[ ] Required Campfire Level set
```

### Recipe Button Prefab

```text
[ ] Button component exists
[ ] RecipeButtonUI component exists
[ ] Icon assigned
[ ] Item Name assigned
```

### Scroll View

```text
[ ] Scroll View exists
[ ] Viewport exists
[ ] Content exists
[ ] Vertical Layout Group added to Content
[ ] Content Size Fitter added to Content
[ ] Vertical Fit = Preferred Size
[ ] Horizontal = OFF
[ ] Vertical = ON
[ ] Scroll Rect Content assigned
[ ] Scroll Rect Viewport assigned
[ ] Vertical Scrollbar assigned
[ ] Viewport has Rect Mask 2D
```

### Inventory / Hotbar

```text
[ ] InventoryController exists
[ ] HotbarController exists
[ ] Hotbar remains active while crafting
[ ] Item IDs are correctly configured
```

### Campfire

```text
[ ] CampfireController exists
[ ] CampfireLevel works
[ ] Recipe required levels are configured
[ ] Leveling up refreshes recipe unlocks
```

Once these are configured, the crafting system can generate the recipe list automatically and handle recipe selection, ingredient checking, crafting, and campfire-level unlocking.