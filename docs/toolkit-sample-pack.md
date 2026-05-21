# Toolkit Sample Pack

This is the canonical copy-from-here reference pack for the current content model.

It is intentionally small. The goal is not to be a new real game area; the goal is to show the preferred structure for the five most common authoring slices:

- one **item** pattern
- one **mob** pattern
- one **scripted prop** pattern
- one **authored floor** pattern
- one **generated dungeon** pattern

## Where the sample lives

### Sample dungeon package

- `src/BroughlikeMonoGame.Core/Content/Dungeons/ToolkitSample/ToolkitSampleDefinition.cs`
- `src/BroughlikeMonoGame.Core/Content/Dungeons/ToolkitSample/ToolkitSampleFloors.cs`

### Supporting central registries the sample relies on

- items: `src/BroughlikeMonoGame.Core/Items/ItemCatalog.cs`
- mobs: `src/BroughlikeMonoGame.Core/Entities/MonsterCatalog.cs`
- mob behavior: `src/BroughlikeMonoGame.Core/GameSession.cs`
- mob rendering: `src/BroughlikeMonoGame.Core/Graphics/GameRenderer.cs`

The sample package deliberately uses existing content ids so it stays copyable without forcing unrelated gameplay changes.

---

## What this sample demonstrates

### 1. Item pattern: `power`

The sample uses `power` as the canonical reference item because it is already wired through the live runtime and easy to reason about.

Use it as the template for:

- adding an item definition to `ItemCatalog`
- referencing an item by id from authored content
- referencing an item by id from generated item tables
- gating exit outcomes on inventory state

In the sample pack, `power` appears in three places:

- spawned by a scripted prop (`Supply Crate`)
- available in the generated dungeon item table
- used as an exit condition for the success route

### 2. Mob pattern: `jester`

The sample uses `jester` as the canonical enemy reference because it already has a distinct behavior rule and renderer path.

Use it as the template for:

- adding a `MonsterKind`
- adding a `MonsterArchetype`
- wiring behavior in `GameSession.UpdateMonster(...)`
- placing the mob in a spawn table

In the sample pack, `jester` appears in the generated floor monster table with a lower weight than `bird`.

### 3. Scripted prop pattern: `Supply Crate`

The authored sample floor contains a scripted interactable that:

- blocks movement
- shows text
- spawns an item by id
- uses an explicit item spawn offset

That is the current preferred pattern for simple authored props that should grant an item or deliver a short narrative beat without bespoke session code.

### 4. Authored floor pattern: `Sample Antechamber`

The first floor in the sample package is a `FixedLevelSource` floor. It demonstrates:

- local layout ownership in one package
- authored world-object placement
- a simple exit to another floor in the same dungeon package
- a portal object pointing into another dungeon

Use this as the template for hubs, bespoke scenes, puzzle rooms, and narrative beats.

### 5. Generated dungeon pattern: `Sample Depth 1`

The second floor is a `ProceduralLevelSource` floor. It demonstrates:

- a local spawn profile
- weighted enemy tables
- weighted item tables
- floor items + enemy item drops
- outcome-based exit routing
- world-state checkpoint updates through `SetsCurrentStart`

Use this as the template for a real generated dungeon package.

---

## Copying the sample

### If you want a new authored room/hub

Start from:

- `ToolkitSampleDefinition.cs`
- the `Sample Antechamber` floor in `ToolkitSampleFloors.cs`

Then change:

- dungeon id / display name
- floor ids / display names
- layout rows
- world objects
- exit routes

### If you want a new generated dungeon

Start from:

- the `Sample Depth 1` floor in `ToolkitSampleFloors.cs`

Then change:

- spawn profile
- item table
- monster table
- exit routing
- checkpoint behavior

### If you want a new item

Start from the way `power` is used here, then update:

1. `ItemCatalog.cs`
2. any authored/world-object/item-table references
3. `GameplayCheck`

### If you want a new mob

Start from the way `jester` is used here, then update:

1. `GameEnums.cs`
2. `MonsterCatalog.cs`
3. `GameRenderer.cs`
4. `GameSession.UpdateMonster(...)`
5. spawn tables
6. `GameplayCheck`

---

## Validation loop

Whenever you copy this sample into real content, run:

```bash
dotnet run --project tests/GameplayCheck
dotnet build BroughlikeMonoGame.sln
```

The content validator should catch common bad refs before manual playtesting does.
