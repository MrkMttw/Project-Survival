Project Survival --- Centralized Loot Drop System
Overview
The centralized loot drop system handles the creation and placement of
items dropped by breakable world objects and mobs.

DropLootController is responsible for spawning the actual dropped
item, placing it in the world, parenting it under the global
dropped-items container, and starting its loot-drop animation.

The scripts that cause drops remain responsible for their own drop
rules, including drop chance and quantity.
Main Components
DropLootController
DropLootController is the central spawning component.

Responsibilities:
- Validate the item prefab and requested amount.
- Clone the item using the existing Item.CloneItem(int amount)
  method.
- Set the dropped item's world position.
- Parent the item under OverWorld/DroppedItems.
- Add LootDropAnimation if the cloned item does not already have
  one.
- Play the loot-drop animation.
The controller exposes:
- SpawnLoot(GameObject itemPrefab, int amount, Vector3 position)
- GetLootChunk(Vector3 worldPosition)
SpawnLoot returns the spawned GameObject, or null if spawning cannot
be completed.
Drop Sources
The following scripts calculate their own drop chance and quantity, then
request spawning through DropLootController:

  Script                    Drop source
  BreakSystem             Breakable environmental objects
ExplodingMobBehavior    Exploding mobs
HostileMobBehavior      Hostile mobs
ProjectileMobBehavior   Projectile-based mobs
PassiveMobBehavior      Passive mobs

These scripts retain their existing gameplay and death behavior. The
centralized controller handles only the actual loot spawning.
Scene Setup
Create a scene GameObject named DropLootController and attach the
DropLootController component.

Assign its World Generator reference to the scene's WorldGenerator.

The Dropped Items Parent reference can be left empty. During
initialization, the controller searches for a child named DroppedItems
under WorldGenerator.overWorld. If it cannot find one, it creates that
child automatically.

Expected hierarchy:
OverWorld
├── NaturalObjects
├── ActiveMobs
├── PlayerBuilds
└── DroppedItems
The DroppedItems container must be under the global OverWorld, not
under an individual chunk.
Drop Flow
1. A breakable object or mob reaches its existing drop logic.
2. Its source script checks the configured drop entries.
3. The source script rolls the configured drop chance.
4. The source script calculates the drop quantity.
5. The source script calls
   DropLootController.Instance.SpawnLoot(...).
6. The controller clones the item, positions it, parents it under
   DroppedItems, and plays LootDropAnimation.
Chunk Unloading Behavior
Dropped items are parented under the global OverWorld/DroppedItems
container rather than a chunk GameObject.

As a result:
- Unloading a world chunk does not destroy its dropped items.
- Dropped items remain in the world until collected or destroyed by
  another system.
- The current implementation does not perform chunk-based loot
  despawning.
- The current implementation does not save dropped items to disk or
  restore them after restarting the game.
This is runtime survival across chunk unloading, not save/load
persistence.
Existing Systems Preserved
The centralized implementation keeps the source scripts responsible for
their original drop rules.

It does not replace or redesign:
- Drop chance configuration.
- Minimum and maximum drop amounts.
- Item definitions and cloning behavior.
- Player pickup and inventory handling.
- Mob combat, movement, or death behavior.
- Breakable object damage and destruction behavior.
- Tree fall and smoke effects.
LootDropAnimation remains responsible for the visual movement of
spawned loot.
Testing Checklist
- [ ] Confirm the scene contains exactly one active
  DropLootController.
- [ ] Confirm the controller references the correct WorldGenerator.
- [ ] Confirm DroppedItems is created under OverWorld.
- [ ] Break an environmental object and verify its configured drops.
- [ ] Defeat an exploding mob and verify its drops.
- [ ] Defeat a hostile mob and verify its drops.
- [ ] Defeat a projectile mob and verify its drops.
- [ ] Defeat a passive mob and verify its drops.
- [ ] Confirm configured drop chances and quantity ranges still work.
- [ ] Confirm dropped items play their loot animation.
- [ ] Move away until the original chunk unloads and confirm its
  dropped items remain.
- [ ] Collect the dropped items and confirm the existing pickup and
  inventory behavior works.
Current Scope and Limitations
- Loot spawning is centralized, but drop-table decisions remain
  distributed across the source scripts.
- Dropped items survive chunk unloading during the current runtime
  session.
- Dropped items are not currently included in disk save data.
- No automatic chunk-based despawn or cleanup is performed by
  DropLootController.