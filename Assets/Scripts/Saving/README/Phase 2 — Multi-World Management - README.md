# Phase 2 — Multi-World Management

## Overview

Phase 2 implements a multi-world management system for Project Survival. It allows players to create multiple worlds, view their saved worlds, select a world, and enter the game using that world's saved information.

Each world has its own:

- World ID
- World name
- World seed
- Creation date
- Last played date
- World save directory

The system uses `WorldSaveManager` to manage world information and `SelectedWorldData` to transfer the selected world between scenes.

---

# 1. System Workflow

The multi-world management system follows this sequence:

1. Player opens the Main Menu.
2. Player clicks Play.
3. `WorldSaveManager.Initialize()` initializes the save system.
4. The World Selection screen displays registered worlds.
5. Player selects an existing world or creates a new one.
6. Player clicks Continue.
7. `WorldSelectionController` stores the selected world in `SelectedWorldData`.
8. Unity loads the GameScene.
9. `WorldGenerator` retrieves the selected world data.
10. The selected world's seed is applied before chunk generation.

---

# 2. Required Scripts

| Script | Responsibility |
|---|---|
| `WorldSaveData.cs` | Stores world metadata and catalog information. |
| `WorldSaveManager.cs` | Creates, retrieves, renames, deletes, and validates worlds. |
| `StartMenuController.cs` | Controls Main Menu navigation and scene transitions. |
| `WorldSelectionController.cs` | Displays and manages world selection. |
| `WorldEntryUI.cs` | Displays individual world information and handles selection. |
| `CreateWorldController.cs` | Handles world creation and input validation. |
| `SelectedWorldData.cs` | Temporarily transfers selected world information between scenes. |
| `WorldGenerator.cs` | Applies the selected world's seed before generating chunks. |

---

# 3. Main Menu Scene Hierarchy

The Main Menu Scene contains the navigation UI, world selection screen, and world creation screen.

Suggested hierarchy:

```text
MainMenu
│
├── Main Camera
│
├── EventSystem
│
├── Canvas
│   │
│   ├── MainMenu
│   │   ├── PlayButton
│   │   └── ExitButton
│   │
│   ├── PlayScreen
│   │   │
│   │   ├── Title
│   │   │
│   │   ├── WorldScrollView
│   │   │   ├── Viewport
│   │   │   │   └── Content
│   │   │   │       ├── WorldEntry(Clone)
│   │   │   │       ├── WorldEntry(Clone)
│   │   │   │       └── WorldEntry(Clone)
│   │   │   │
│   │   │   └── Scrollbar Vertical
│   │   │
│   │   ├── CreateWorldButton
│   │   ├── ContinueButton
│   │   └── BackButton
│   │
│   └── CreateWorld
│       │
│       ├── WorldNameInput
│       ├── SeedInput
│       ├── CreateButton
│       └── BackButton
│
├── StartMenuController
├── WorldSelectionController
└── CreateWorldController
```

The hierarchy is a recommended organization. GameObjects can be arranged differently as long as the Inspector references and script connections are correct.

## 3.1 StartMenuController

Attach `StartMenuController` to a GameObject in the Main Menu Scene.

Assign these references in the Inspector:

| Field | Assigned GameObject |
|---|---|
| `MainMenu` | MainMenu panel |
| `PlayScreen` | PlayScreen panel |
| `CreateWorld` | CreateWorld panel |

The script controls which panel is active.

### Button connections

| Button | OnClick Function |
|---|---|
| Play | `StartMenuController.OnClickPlay()` |
| Main Menu Back | `StartMenuController.OnClickBack()` |
| Create World | `StartMenuController.OnClickCreateWorld()` |
| Create World Back | `StartMenuController.OnClickCreateWorldBack()` |
| Exit | `StartMenuController.OnExitClick()` |

The Play button initializes `WorldSaveManager` before opening the world selection screen.

---

# 4. Setting Up the Scrollable World List

The Scroll View is responsible for displaying registered worlds. Each world is represented by an individual prefab.

## 4.1 Create the Scroll View

Inside the Canvas:

1. Right-click the PlayScreen panel.
2. Select UI → Scroll View.
3. Rename it to `WorldScrollView`.

Unity automatically creates the following hierarchy:

```text
WorldScrollView
│
├── Viewport
│   │
│   └── Content
│
└── Scrollbar Vertical
```

The Scroll View contains three important components:

| Object | Responsibility |
|---|---|
| WorldScrollView | Handles scrolling and viewport behavior. |
| Viewport | Defines the visible area of the world list. |
| Content | Holds the instantiated world entry prefabs. |

## 4.2 Configure the Viewport

Select `Viewport`.

Recommended components:

- Rect Transform
- Image
- Mask

The Mask component ensures that world entries outside the visible area are hidden while scrolling.

Make sure the Viewport has a suitable Rect Transform size to define the visible world list area.

## 4.3 Configure the Content

Select `Content` inside the Viewport.

Add these components:

- Rect Transform
- Vertical Layout Group
- Content Size Fitter

### Rect Transform

Set the Content anchors to stretch horizontally at the top of the Viewport.

Recommended anchor configuration:

- Anchor Min: X = 0, Y = 1
- Anchor Max: X = 1, Y = 1
- Pivot: X = 0.5, Y = 1

This makes the content expand downward as additional world entries are added.

### Vertical Layout Group

Recommended settings:

| Property | Setting |
|---|---|
| Child Alignment | Upper Center |
| Spacing | 10 |
| Control Child Size - Width | Enabled |
| Control Child Size - Height | Enabled |
| Child Force Expand - Width | Enabled |
| Child Force Expand - Height | Disabled |

Adjust spacing according to your UI design.

### Content Size Fitter

Set:

| Property | Setting |
|---|---|
| Horizontal Fit | Unconstrained |
| Vertical Fit | Preferred Size |

This allows the Content object's height to grow according to the number and size of its world entries.

**Important:** The Content object should be a child of Viewport. Do not place the world entries directly under WorldScrollView.

## 4.4 Configure the Scroll Rect

Select `WorldScrollView`.

On its Scroll Rect component:

| Property | Setting |
|---|---|
| Content | Content Rect Transform |
| Viewport | Viewport Rect Transform |
| Horizontal | Disabled |
| Vertical | Enabled |
| Movement Type | Clamped |
| Scroll Sensitivity | Adjust to preference |

Assign the Scrollbar Vertical if you want a visible scrollbar.

The Vertical Scrollbar can also be hidden if you prefer scrolling with the mouse wheel.

---

# 5. Creating the World Entry Prefab

Each registered world is displayed using a reusable UI prefab.

Suggested hierarchy:

```text
WorldEntry
│
├── Border
│
├── WorldName
│
├── Created
│
├── LastPlayed
│
└── Button
```

The exact visual arrangement is flexible. The important part is that the required UI components are assigned correctly.

## 5.1 Required Components

### WorldEntry (Root)

Add:

- Rect Transform
- WorldEntryUI script

Set an appropriate width and height for the entry.

### Border

Add an Image component.

This Image is used to display the selection color.

### WorldName

Add a TextMeshPro - Text (UI) component.

Displays the world's name.

### Created

Add a TextMeshPro - Text (UI) component.

Displays the world's creation date.

### LastPlayed

Add a TextMeshPro - Text (UI) component.

Displays the last played date.

### Button

Add a Button component.

The button should cover the clickable area of the world entry.

The script registers its own click listener in `Awake()`. You do not need to manually connect the Button's OnClick event to `WorldEntryUI.SelectWorld()`.

## 5.2 Configure WorldEntryUI

Attach `WorldEntryUI` to the WorldEntry root.

Assign the following Inspector references:

| Field | Assigned Component |
|---|---|
| `border` | Border Image |
| `worldName` | WorldName TextMeshProUGUI |
| `created` | Created TextMeshProUGUI |
| `lastPlayed` | LastPlayed TextMeshProUGUI |
| `button` | Button component |

### Selection Colors

The script includes two color settings:

- `normalColor`: Default appearance of the world entry.
- `selectedColor`: Appearance when the world is selected.

The default selected color is yellow.

When the player selects another world, the previous entry returns to its normal color.

## 5.3 Make It a Prefab

1. Drag the WorldEntry GameObject from the Hierarchy into your Project window.
2. Store it in a suitable Prefabs folder.
3. Delete the temporary scene instance if necessary.

The resulting prefab will be instantiated automatically by `WorldSelectionController`.

---

# 6. Setting Up WorldSelectionController

Attach `WorldSelectionController` to a GameObject in the Main Menu Scene.

Assign the following Inspector references:

| Field | Assigned Object |
|---|---|
| `content` | Content Transform inside WorldScrollView |
| `worldEntryPrefab` | WorldEntry prefab |
| `continueButton` | ContinueButton |

### How It Works

When the World Selection screen becomes active, `OnEnable()` calls `RefreshWorldList()`.

The method:

1. Clears existing world entries.
2. Resets the selected world.
3. Retrieves registered worlds using `WorldSaveManager.GetAllWorlds()`.
4. Instantiates one WorldEntry prefab for every registered world.
5. Assigns the world's information to the prefab through `WorldEntryUI.Setup()`.

When the player clicks an entry:

1. `WorldEntryUI` invokes its selection callback.
2. `WorldSelectionController.SelectWorld()` receives the selected world.
3. The previously selected entry is deselected.
4. The new entry is highlighted.
5. The Continue button becomes interactable.

---

# 7. Setting Up World Creation

The CreateWorld panel allows players to register a new world.

Suggested hierarchy:

```text
CreateWorld
│
├── WorldNameInput
├── SeedInput
├── CreateButton
└── BackButton
```

Use TextMeshPro Input Fields for both inputs.

## 7.1 World Name Input

Assign a TMP_InputField to:

`CreateWorldController.worldNameField`

The world name is required.

If the input is empty, the script displays a warning and does not create the world.

## 7.2 Seed Input

Assign another TMP_InputField to:

`CreateWorldController.seedField`

The seed is optional.

- If left empty, a random seed is generated.
- If a valid integer is entered, that value is used.
- If an invalid value is entered, world creation is cancelled.

## 7.3 Configure CreateWorldController

Attach `CreateWorldController` to a GameObject in the Main Menu Scene.

Assign these references:

| Field | Assigned Object |
|---|---|
| `worldNameField` | WorldNameInput |
| `seedField` | SeedInput |
| `worldSelectionController` | GameObject containing WorldSelectionController |
| `startMenuController` | GameObject containing StartMenuController |

### Create Button

Connect the Create Button to:

`CreateWorldController.OnClickCreate()`

When clicked, the method:

1. Reads and validates the world name.
2. Generates or parses the world seed.
3. Calls `WorldSaveManager.CreateWorld()`.
4. Refreshes the world selection list.
5. Returns to the world selection screen.

### Back Button

Connect the Back Button to:

`StartMenuController.OnClickCreateWorldBack()`

---

# 8. Selected World Data Handoff

The selected world must be transferred from the Main Menu Scene to the GameScene.

## 8.1 SelectedWorldData

Create a script named `SelectedWorldData.cs`.

```csharp
public static class SelectedWorldData
{
    public static WorldSaveData world;
}
```

This is a static class.

**Do not attach it to a GameObject.**

Its purpose is to temporarily hold the selected world data while Unity transitions between scenes.

## 8.2 Continue Button

The Continue Button must call:

`WorldSelectionController.OnClickContinue()`

Do not connect it to `StartMenuController.OnClickLoadScene()`.

The `OnClickContinue()` method:

1. Checks whether a world is selected.
2. Assigns the selected world to `SelectedWorldData.world`.
3. Loads the GameScene.

This ensures the selected world information is available after the scene transition.

---

# 9. Connecting WorldGenerator

In the GameScene, the `WorldGenerator` retrieves the selected world before generating its initial chunks.

At the beginning of `WorldGenerator.Start()`:

```csharp
if (SelectedWorldData.world != null)
{
    worldData = SelectedWorldData.world;
    worldSeed = worldData.worldSeed;

    Debug.Log("WORLD LOADED: " + worldData.worldName);
    Debug.Log("WORLD SEED APPLIED: " + worldSeed);
}
```

This must execute before:

```csharp
currentPlayerChunk = GetChunkCoordinate(player.position);

UpdateChunks();
```

The reason is that `UpdateChunks()` immediately starts generating chunks.

Applying the selected seed beforehand ensures that the initial chunks use the selected world's seed instead of the default Inspector value.

---

# 10. World Save Directory

World information is managed by `WorldSaveManager`.

The world directory is created under:

```text
Application.persistentDataPath
│
└── Worlds
    │
    ├── worlds.json
    │
    ├── World_ID_1
    │   └── world.json
    │
    ├── World_ID_2
    │   └── world.json
    │
    └── World_ID_3
        └── world.json
```

The actual directory names are generated using each world's unique ID.

`worlds.json` contains the registered world catalog.

Each world's `world.json` stores its metadata.

---

# 11. Testing Procedures

Perform the following tests to verify the multi-world management system.

## Test 1 — World List Display

1. Open the Main Menu.
2. Click Play.
3. Verify that registered worlds appear in the scrollable list.
4. Scroll through the list if multiple worlds exist.

Expected result: All registered worlds are displayed with their corresponding information.

## Test 2 — World Selection

1. Select a world.
2. Verify that its border changes to the selected color.
3. Select another world.
4. Verify that the previous world returns to its normal color.

Expected result: Only one world is highlighted at a time.

## Test 3 — Continue Button

1. Open the World Selection screen.
2. Verify that Continue is disabled initially.
3. Select a world.
4. Verify that Continue becomes interactable.
5. Click Continue.

Expected result: The selected world is passed to the GameScene.

## Test 4 — World Creation

1. Open the Create World screen.
2. Enter a world name.
3. Leave the seed field empty.
4. Click Create.

Expected result: A new world is created with a generated seed and appears in the world list.

## Test 5 — Custom Seed

1. Open the Create World screen.
2. Enter a world name.
3. Enter a valid integer seed.
4. Click Create.

Expected result: The world is created using the specified seed.

## Test 6 — Invalid Input

Test the following:

- Empty world name.
- Invalid seed input.

Expected result: The system prevents invalid world creation.

## Test 7 — Selected World Handoff

1. Select an existing world.
2. Click Continue.
3. Check the Console.

Expected logs:

```text
CONTINUE BUTTON CLICKED!
SELECTED WORLD: Jackyline
WORLD SEED: 1234567890
WORLD HANDOFF SUCCESSFUL!
SELECTED WORLD RECEIVED!
WORLD NAME: Jackyline
WORLD ID: ...
WORLD SEED: 1234567890
```

Expected result: The GameScene receives the correct selected world information.

## Test 8 — World Seed Application

1. Select a world.
2. Enter the GameScene.
3. Check the Console.

Expected logs:

```text
WORLD LOADED: Jackyline
WORLD SEED APPLIED: 1234567890
```

Expected result: The WorldGenerator applies the selected world's seed before generating chunks.

---

# 12. Common Setup Mistakes

| Problem | Possible Cause |
|---|---|
| World list is empty | WorldSaveManager has not been initialized or no worlds are registered. |
| World entries do not appear | Incorrect Content or WorldEntry prefab reference. |
| World entry text is missing | Missing TextMeshPro references in WorldEntryUI. |
| Selection highlighting does not work | Border Image is not assigned. |
| Continue remains disabled | No world has been selected. |
| Continue loads the wrong scene behavior | Incorrect Button OnClick connection. |
| Selected world is null in GameScene | OnClickContinue was not called or the handoff assignment was not executed. |
| Default seed is used | Selected world data was not applied before UpdateChunks(). |
| Scroll View does not scroll | Incorrect Content, Viewport, or Scroll Rect configuration. |
| World entries overlap | Vertical Layout Group or Content Size Fitter is missing or incorrectly configured. |

---

# 13. Phase 2 Completion Status

The following features have been implemented and tested:

- [x] World save system initialization.
- [x] Retrieve registered worlds.
- [x] Display registered worlds.
- [x] Scrollable world selection list.
- [x] World selection and highlighting.
- [x] Continue button validation.
- [x] World creation with custom or generated seed.
- [x] Refresh world list after creation.
- [x] Selected world data handoff between scenes.
- [x] Apply selected world seed before initial chunk generation.

**Phase 2 — Multi-World Management: Complete**

The system can now create and select worlds and pass the selected world's metadata into the GameScene.

Note: This phase establishes world selection and seed initialization. Full restoration of player progress and persistent world changes is handled by the subsequent save system phases.