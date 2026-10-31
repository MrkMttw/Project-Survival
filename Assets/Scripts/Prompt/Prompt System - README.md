# Centralized Prompt System

## Overview

The Centralized Prompt System manages the game's on-screen keybind
prompts through a single PromptController.

Instead of having each gameplay script directly activate or deactivate
UI objects, scripts request and release prompts through the central
controller. This allows multiple systems to use the same key prompt
without unintentionally hiding it while another system still needs it.

The system supports: - Centralized management of keybind prompt
GameObjects. - Multiple scripts requesting the same prompt type. -
Automatic prompt visibility based on active requests. - Cleanup of
requests when gameplay scripts are disabled. - Protection against
destroyed prompt GameObject references.

## Features

### Supported Prompts
  Prompt Type             Key                     Intended Action
  E                     E                       Interact, eat, toggle
lights, use nearby
objects, or relocate a
building

  F                     F                       Pick up items or
retrieve a building

  G                     G                       Drop the selected item

  ShiftG                Shift + G               Drop the selected item
stack

  B                     B                       Open or close the menu

  Tab                   Tab                     Toggle the placement
grid
R                     R                       Rotate directional
building presets
The prompt text and icons are configured in the UI. The
PromptController only manages the active state of each prompt
GameObject.

### Multiple Requesters

Multiple gameplay systems can request the same prompt type.

For example, several systems may request the E prompt when their
respective interactions are available. The prompt remains visible while
at least one requester still has an active request.

When a script releases its request, only that script's request is
removed. The prompt is hidden only after all requesters have released
it.

### Dynamic Visibility

Prompts are shown or hidden according to gameplay conditions.

Examples: - The E prompt appears when an interaction is available. -
The F prompt appears when a pickup is nearby. - The G prompt appears
when an item is selected. - The Shift + G prompt appears when the
selected item has a stack greater than one. - The B prompt appears
while the menu is closed. - The R prompt appears while placing or
relocating a directional building.

## Main Scripts

### PromptController.cs

Manages all registered prompts and tracks which scripts currently need
them.

Responsibilities: - Registers prompt GameObjects using the PromptType
enum. - Stores prompt requests separately for each requester. - Shows a
prompt when at least one requester needs it. - Hides a prompt when no
requesters need it. - Removes destroyed requester references during
prompt updates. - Checks for destroyed prompt GameObjects before calling
SetActive(). - Hides all registered prompts when HideAllPrompts() is
called.

### PlayerInteractionController.cs

Handles the player's E-key interactions.

Responsibilities: - Checks whether the held item can be eaten. -
Requests the E prompt when eating is available. - Releases its request
when eating is unavailable.

### GateController.cs

Handles nearby gate interactions.

Responsibilities: - Finds the nearest gate within interaction range. -
Requests the E prompt when a gate can be interacted with. - Releases
its request when no gate is in range.

### FurnaceController.cs

Handles furnace interactions.

Responsibilities: - Requests the E prompt when the player is near a
furnace. - Releases its request while the furnace interface is open. -
Releases its prompt request when disabled.

### SleepingBagHitbox.cs

Detects when the player is near a sleeping bag.

Responsibilities: - Tracks the sleeping bag currently within the
interaction trigger. - Requests the E prompt while a sleeping bag is
available. - Releases its request when the player leaves the interaction
area.

### ItemsGeneratorController.cs

Handles interactions with supported item generators.

Responsibilities: - Detects supported generator buildings within
interaction range. - Requests the E prompt when a generator can be
activated. - Releases its request when no supported generator is
available.

### ItemLightController.cs

Handles held-item light interactions.

Responsibilities: - Checks whether the held item has a configured light
preset. - Requests the E prompt when the held item can toggle its
light. - Releases its request when the held item cannot toggle a light.

### PlayerItemCollector.cs

Handles nearby item pickups.

Responsibilities: - Detects nearby pickup items. - Requests the F
prompt when an item can be collected. - Releases its request when no
pickup is nearby.

### HotbarController.cs

Handles selected-item dropping.

Responsibilities: - Requests the G prompt when an item is selected. -
Requests the ShiftG prompt when the selected item is stackable and has
more than one item. - Releases the corresponding requests when the
selected item or stack condition changes.

### MenuController.cs

Handles the B-key menu prompt.

Responsibilities: - Requests the B prompt while the menu is closed. -
Releases the request while the menu is open. - Releases its request when
disabled.

### WrenchFunction.cs

Handles building relocation and retrieval using the wrench.

Responsibilities: - Requests the E prompt when the player holds the
wrench and hovers over a building. - Requests the F prompt under the
same condition for building retrieval. - Releases both requests when the
wrench is no longer held or no building is targeted.

### PlacementController.cs

Handles building placement and relocation prompts.

Responsibilities: - Requests the R prompt while placing a directional
building. - Requests the R prompt while relocating a directional
building. - Releases the request when placement ends, is canceled, or
the active building does not use directional presets.

## Unity Setup

### 1. PromptController

Create a GameObject in the scene and attach the PromptController
component.

Assign the following references in the Inspector:

- Prompt Container: The parent GameObject containing all keybind
prompt objects. - Prompts: The list of prompt entries to register.

Add one entry for each supported prompt type:

  Prompt Type   Prompt Object
  E           E-Key
F           F-Key
G           G-Key
ShiftG      Shift+G-Key
B           B-Key
Tab         Tab-Key
R           R-Key

Drag each corresponding key prompt parent GameObject into its Prompt
Object field.

### 2. Prompt UI Hierarchy

Organize the prompt objects under the prompt container.

Example hierarchy:
Prompt
├── E-Key
├── F-Key
├── G-Key
├── Shift+G-Key
├── B-Key
├── Tab-Key
└── R-Key
The prompt container and its individual prompt objects are controlled by
the PromptController. The controller does not modify the TextMeshPro
text or the child icon Images.

### 3. Gameplay Scripts

Ensure that each gameplay script using the prompt system: - Has access
to PromptController.Instance. - Requests the correct PromptType. -
Uses this as the requester when requesting and releasing. - Releases
its requests when the interaction is no longer available. - Releases its
requests in OnDisable() when applicable.

## How It Works

1. The PromptController registers the prompt GameObjects when the
scene starts. 2. Each gameplay script checks whether its interaction is
currently available. 3. When an interaction becomes available, the
script calls RequestPrompt(). 4. The controller records the requester
and displays the prompt. 5. When the interaction is no longer available,
the script calls ReleasePrompt(). 6. The controller removes that
request and checks whether other requesters still need the prompt. 7.
The prompt is hidden only when no active requests remain.

## Main Methods

### RequestPrompt()

Requests a prompt for a specific requester.
PromptController.Instance.RequestPrompt(PromptType.E, this);
The requester should be the script making the request. Different scripts
can request the same prompt type independently.

### ReleasePrompt()

Releases a prompt request belonging to a specific requester.
PromptController.Instance.ReleasePrompt(PromptType.E, this);
A script should release the same prompt type using the same requester
reference it used when requesting it.

### HideAllPrompts()

Clears all tracked requests and hides every registered prompt.
PromptController.Instance.HideAllPrompts();
Scripts that remain active may request their prompts again during their
next update.

## Performance Considerations

The system updates prompt visibility when scripts request or release
prompts. It does not need to search the scene for prompt objects every
frame.

Gameplay scripts may check their own interaction conditions during
Update(), but the central controller only changes prompt visibility
when a request changes.

## Current Limitations

- The prompt UI GameObjects should remain assigned and available for
the lifetime of the PromptController. - The PromptController
protects against destroyed prompt references, but other scripts should
still avoid unexpectedly destroying registered prompt objects. - The
Tab prompt is registered for the placement grid key, but it must
receive a request from a gameplay script to become visible. - The R
prompt only appears for items that use directional building presets. -
The wrench's R prompt appears during directional building relocation,
not merely while hovering over a building. - Calling HideAllPrompts()
clears every request. Active gameplay scripts may request their prompts
again on a subsequent update.

## Future Improvements

- Add prompt priority or context rules for interactions that share the
same key. - Add automatic prompt registration for dynamically created
prompt UI. - Add optional prompt animations when prompts appear or
disappear. - Add a centralized way to pause or suppress prompts while
blocking UI is open.