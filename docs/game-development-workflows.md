# Game Development Workflows

This project is not trying to ship lots of placeholder content quickly.

The near-term goal is to make `broughlike-monogame` easy to extend into a real mystery-dungeon framework, so new game content should follow repeatable workflows instead of one-off engine surgery.

This document covers the current practical workflows for:

- adding an item
- adding an enemy
- adding a dungeon

Use the live repo structure as the source of truth. If the repo changes, update this doc.

## General rule

For each new content slice:

1. **Define the smallest real content example**
2. **Wire it through the existing content model**
3. **Add or update a regression check** in `tests/GameplayCheck`
4. **Run the smallest meaningful validation loop**
5. **Only then broaden the system** if the example proves a missing framework seam

If a new idea requires changing five unrelated engine files before any example exists, the framework is probably missing a clean authoring seam.

---

## 1. Add an item

### Current source of truth

- Item definitions: `src/BroughlikeMonoGame.Core/Items/ItemDefinition.cs`
- Item catalog: `src/BroughlikeMonoGame.Core/Items/ItemCatalog.cs`
- Inventory/runtime use: `src/BroughlikeMonoGame.Core/GameSession.cs`
- Existing tests: search `tests/GameplayCheck/Program.cs` for item-related checks

### Current item model

Right now an item is:

- an `id`
- a display name
- a `Use(GameSession)` action

That means the current workflow is code-first, but still centralized and predictable.

### Workflow

1. **Choose the item role**
   - combat action
   - utility action
   - key item / progression item
   - support / recovery item

2. **Add the item definition to `ItemCatalog.CreateTutorialItems()`**
   - keep the id stable and lowercase
   - keep the display name player-facing
   - keep the effect small and explicit

3. **Use existing `GameSession` verbs where possible**
   Examples already in use:
   - `TeleportActor(...)`
   - `BoltTravel(...)`
   - `TransformAdjacentWallsToTreasure(...)`
   - `StartLevel(...)`
   - `PlaceEffect(...)`
   - direct player state like `BonusAttack`, `Shield`, `Heal(...)`

4. **Make the item reachable in content**
   Depending on need:
   - add it to an authored `ItemPickup`
   - add it to a spawn profile item table
   - add it as an enemy death drop
   - use it as a progression/key item in authored content

5. **Add a regression check**
   Prefer one specific test that proves the item's real contract.

6. **Run validation**
   - `dotnet run --project tests/GameplayCheck`
   - `dotnet build BroughlikeMonoGame.sln`

### Example patterns in the repo

- `black-suit`
  - progression/key item
  - exists in `ItemCatalog`
  - spawned from an authored dresser interaction in apartment intro
  - used by exit routing in `ApartmentIntroDefinition`

- `power`
  - simple buff item
  - used both in gameplay and as a routing condition in tutorial success/failure exits

- `mulligan`
  - level-reset style item
  - currently restarts the level with current inventory and max HP preserved

### What to avoid

- scattering item ids across unrelated systems without a test
- adding item-specific logic directly into the renderer
- introducing a new subsystem before proving a single useful item with the current seams

### If the current model becomes too limiting

That is the signal to introduce the next framework seam, likely:

- typed item categories
- item metadata beyond `Use(GameSession)`
- targeting rules
- passive vs consumable vs key-item distinction

But do that only after one concrete item exposes the limitation.

---

## 2. Add an enemy

### Current source of truth

- Enemy kinds enum: `src/BroughlikeMonoGame.Core/Data/GameEnums.cs`
- Enemy archetypes: `src/BroughlikeMonoGame.Core/Entities/MonsterArchetype.cs`
- Enemy catalog: `src/BroughlikeMonoGame.Core/Entities/MonsterCatalog.cs`
- Enemy behavior logic: `src/BroughlikeMonoGame.Core/GameSession.cs` (`UpdateMonster`)
- Rendering cases: `src/BroughlikeMonoGame.Core/Graphics/GameRenderer.cs`
- Dungeon spawn tables: `src/BroughlikeMonoGame.Core/Content/SpawnProfile.cs` and dungeon-specific spawn-profile files

### Current enemy model

An enemy currently has three main layers:

1. **identity** via `MonsterKind`
2. **stats/look** via `MonsterCatalog` / `MonsterArchetype`
3. **behavior** via `GameSession.UpdateMonster(...)`

So the current workflow is still code-driven, but it is at least localized.

### Workflow

1. **Add a new `MonsterKind`**
   - keep the name stable and concise

2. **Add a `MonsterArchetype` to `MonsterCatalog`**
   Choose:
   - display name
   - palette/color
   - max HP

3. **Add renderer support**
   Update `GameRenderer` so the enemy has a distinct visible representation.

4. **Add behavior in `GameSession.UpdateMonster(...)`**
   Use the smallest possible behavior rule.
   Existing examples:
   - default chaser (`Bird`)
   - double-move chaser (`Snake`)
   - alternating/stun cadence (`Tank`)
   - wall-eating behavior (`Eater`)
   - random adjacent motion (`Jester`)

5. **Make the enemy spawn somewhere**
   Usually through a spawn table in a dungeon-specific `SpawnProfile`.

6. **Add or update a regression check**
   Prefer a test that proves the enemy's real behavior instead of only checking catalog presence.

7. **Run validation**
   - `dotnet run --project tests/GameplayCheck`
   - `dotnet build BroughlikeMonoGame.sln`

### Recommended enemy design approach

When adding a new enemy, write down:

- what does it do that is tactically different?
- what player habit does it punish or reward?
- what is the minimum behavior rule that creates that identity?

If the answer requires a huge bespoke branch, consider whether the framework needs a reusable AI/ability seam instead.

### Example workflow checklist

For a hypothetical `Painter` enemy:

- add `MonsterKind.Painter`
- add `MonsterCatalog.Painter`
- render it in `GameRenderer`
- define one behavior rule in `UpdateMonster`
  - e.g. leaves hazards, retreats, or paints walls/passable tiles
- add it to one spawn profile with low weight
- add one deterministic check in `GameplayCheck`

### What to avoid

- changing spawn tables before the enemy has defined behavior
- adding behavior only in rendering or only in data without runtime support
- making every new enemy require broad rewrites across all combat code

### When to stop and improve the framework

If multiple new enemies want similar behavior patterns, that is the sign to extract:

- reusable AI strategies
- status/effect hooks
- ability-driven enemy actions

That should happen after two or three real examples, not before the first one.

---

## 3. Add a dungeon

### Current source of truth

- Dungeon registry/catalog: `src/BroughlikeMonoGame.Core/Content/DungeonCatalog.cs`
- Dungeon model: `src/BroughlikeMonoGame.Core/Content/DungeonDefinition.cs`
- Floor model: `src/BroughlikeMonoGame.Core/Content/FloorDefinition.cs`
- Procedural level generation: `src/BroughlikeMonoGame.Core/Content/ProceduralLevelSource.cs`
- Authored level generation: `src/BroughlikeMonoGame.Core/Content/FixedLevelSource.cs`
- Exit routing: `src/BroughlikeMonoGame.Core/Content/ExitRoute.cs`, `ExitDefinition.cs`, `PortalDestination.cs`
- Spawn logic: `src/BroughlikeMonoGame.Core/Content/SpawnProfile.cs`
- Canonical examples:
  - `Content/Dungeons/ApartmentIntro/**`
  - `Content/Dungeons/TutorialDungeon/**`
  - `Content/Dungeons/HubStart/**`
  - `Content/Dungeons/HubSuccess/**`
  - `Content/Dungeons/HubFailure/**`

### Current dungeon model

A dungeon is:

- one `DungeonDefinition`
- one or more `FloorDefinition`s
- each floor uses either:
  - `FixedLevelSource` for authored layouts, or
  - `ProceduralLevelSource` for generated floors
- each floor has a `SpawnProfile`
- each floor may have an `ExitDefinition`

### Workflow

1. **Create a new content package directory**

Recommended pattern:

- `src/BroughlikeMonoGame.Core/Content/Dungeons/<YourDungeon>/`

2. **Add the main dungeon definition file**

Follow the pattern of:

- `ApartmentIntroDefinition.cs` for authored/multi-floor narrative content
- `TutorialDungeonDefinition.cs` + helper files for generated dungeons

3. **Choose the floor source type per floor**

Use:
- `FixedLevelSource` for hubs, story rooms, bespoke sequences, puzzle-like layouts
- `ProceduralLevelSource` for standard generated dungeon floors

4. **Define a spawn profile**

Choose:
- initial spawn rate
- initial monster count
- treasure count
- optional floor items
- optional enemy item drops
- monster weight table
- optional item weight table

5. **Define exits and progression behavior**

Use `ExitRoute` to describe:
- destination dungeon/floor
- required item
- required progress flag
- granted progress flag
- label
- whether this exit should set `WorldState.currentStart`

This is now the key progression seam.

6. **Register the dungeon in `DungeonCatalog`**

If it is not in the catalog, the game cannot load it.

7. **Make it reachable**

That can happen through:
- a hub exit
- an authored portal world object
- an existing dungeon route
- world-state `currentStart`

8. **Add a regression check**

Depending on the dungeon type, test one of:
- it loads through the runtime
- its exit routes correctly
- its authored interactions work
- its progression checkpoint updates world state correctly

9. **Run validation**
   - `dotnet run --project tests/GameplayCheck`
   - `dotnet build BroughlikeMonoGame.sln`

### Canonical examples

#### `ApartmentIntro`
Use this as the pattern for:
- authored narrative floors
- scripted interactables
- authored world objects
- key-item-driven routing
- progression checkpoint updates via `SetsCurrentStart`

#### `TutorialDungeon`
Use this as the pattern for:
- procedural floors
- shared spawn profiles
- multi-floor generated dungeon flow
- success/failure routing back into hubs

#### `HubStart` / `HubSuccess` / `HubFailure`
Use these as the pattern for:
- hub-like one-floor authored dungeons
- return destinations
- progression-dependent boot locations

### Recommended folder structure for a new dungeon

At minimum:

- `<DungeonName>Definition.cs`

When the dungeon grows:

- `<DungeonName>Definition.cs`
- `<DungeonName>Floors.cs`
- `<DungeonName>SpawnProfiles.cs`
- optional helper files for authored props or routing logic

Keep the package local and avoid editing unrelated central files except for catalog registration.

### What to avoid

- hiding progression in `GameSession` defaults instead of exit routes/world state
- mixing authored and generated logic in one giant file without a reason
- creating a dungeon by editing random session code and never adding a content package

---

## Recommended validation loop by content type

### Item
- one `GameplayCheck` proving behavior
- solution build

### Enemy
- one deterministic `GameplayCheck` for the AI/behavior contract
- solution build

### Dungeon
- one smoke-test for load/routing/progression
- solution build

Commands:

```bash
dotnet run --project tests/GameplayCheck
dotnet build BroughlikeMonoGame.sln
```

---

## Current limitations to remember

These workflows are valid for the current framework, but the framework is still evolving.

Known likely next seams:

- richer item metadata and targeting
- reusable enemy AI/ability patterns
- broader world-state beyond flags/currentStart/activeRun
- content validation tooling for bad ids/routes
- better Matt-facing examples for authored interactables and full hub changes

If a new content request gets awkward, prefer improving the seam once, then documenting the better workflow.
