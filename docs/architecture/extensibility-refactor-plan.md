# Extensibility refactor plan

## Why this pass exists
The tutorial recreation is now playable on desktop and web, but the current core is still shaped around tutorial assumptions:
- one procedural dungeon flow
- monsters as the only meaningful actors besides treasure/exit tiles
- spells as a fixed list of equipped slots instead of carried items
- spawn rules embedded directly in `GameSession`
- level generation and level runtime tightly coupled
- progression/save state limited to scoreboard-style persistence

That shape is fine for the tutorial, but it will fight the next goals:
- hub zone + persistent progression
- portal-based dungeon selection
- fixed/authored levels as well as generated floors
- non-enemy interactables
- granular spawn tuning
- items/pickups/inventory replacing spell-slot assumptions
- easy addition of new mobs and items by hand

## Current architecture audit

### Good foundations already present
- Shared gameplay core exists and is reused by desktop + web hosts.
- Web input now has a dedicated browser-event queue at the host boundary.
- Regression checks exist in `tests/GameplayCheck` and already caught a real combat kill-path bug.
- Rendering, input host code, persistence, and simulation are at least partially separated.

### Main coupling problems to remove
1. **`GameSession` is still the god object**
   - Owns run state, level generation, monster updates, spawn rules, treasure progression, spell resolution, win/death flow, and save-facing score data.
2. **Content is hardcoded in code-first catalogs**
   - `MonsterCatalog` and `SpellBook` are static code lists.
   - Good for bootstrap, bad for authoring lots of content.
3. **The map/runtime model is tile-centric but not entity-centric enough**
   - Treasure and exit are tile flags/kinds.
   - Future hub props, pickups, portals, switches, shrines, NPCs, etc. need a more general interaction model.
4. **Spawn logic is embedded in the session loop**
   - `SpawnRate`, `SpawnCounter`, and monster selection all live in `GameSession`.
   - No notion of dungeon type, room theme, floor rules, or authored encounter tables.
5. **Inventory assumptions are too narrow**
   - `PlayerSpells` is a list of names with null holes.
   - That blocks normal pickups, stackable items, equipment, consumables, and richer item behaviors.
6. **Level source is implicit procedural generation only**
   - `StartLevel()` always creates a fresh generated `LevelGrid`.
   - There is no level-definition abstraction that could point to a fixed map, hub layout, or scripted floor.
7. **Progression persistence is too thin**
   - Current persistence is basically scores only.
   - Hub state, unlocked portals, discovered content, and save slots need their own model.
8. **Repo cleanup still needed**
   - `src/BroughlikeMonoGame.Desktop/Core/**` duplicate copies still exist in-tree even though the csproj excludes them.

## Target architecture shape

### 1) Split runtime state from content definitions
Introduce content/data definitions that describe what can exist, and runtime instances that describe what currently exists.

Suggested direction:
- `Content/Monsters/*` -> monster definitions
- `Content/Items/*` -> item definitions
- `Content/Interactables/*` -> portal/shrine/chest/NPC definitions
- `Content/Dungeons/*` -> dungeon descriptors, spawn tables, generation rules
- runtime classes for live actors/entities that reference those definitions

This can still start code-first if needed, but the interfaces should stop assuming static tutorial catalogs.

### 2) Replace spell slots with inventory + usable items
Move from:
- `List<string?> PlayerSpells`

to something more like:
- `Inventory`
- `InventorySlot`
- `ItemDefinition`
- `ItemStack` or runtime item instance
- `IUsableEffect` / item action definitions

Design target:
- tutorial spells become a subset of usable items
- items can be floor pickups instead of automatic rewards
- hub/dungeon rewards can grant items without special-case code

### 3) Add a general interactable/entity layer
Support world objects that are not monsters:
- pickups
- portals
- hub props
- NPCs
- switches/shrines/chests

Minimum requirement:
- stop treating all interaction as either moving into empty tile, attacking occupant, or stepping on treasure/exit tile flags
- add a clearer interaction pipeline for tile contents and/or non-hostile entities

### 4) Add a level-source abstraction
A floor should come from a descriptor, not directly from `GameSession.GenerateLevel()`.

Suggested concepts:
- `LevelDefinition` or `FloorDefinition`
- `ILevelSource` with procedural and fixed implementations
- procedural generator config separated from runtime session logic

Needed outcomes:
- authored hub map
- authored special floors
- procedural dungeon floors using different rule sets

### 5) Extract spawn rules into configurable dungeon descriptors
Instead of session-global spawn counters and direct random monster picks, create per-dungeon/floor configuration:
- eligible monster/item/interactable tables
- spawn cadence
- floor-specific limits
- weighted content pools
- portal/dungeon identity

That enables:
- “forest portal spawns snakes + herbs more often”
- “ruins portal uses fixed floor 1, generated floors later”
- “hub spawns nothing hostile at all”

### 6) Add persistent progression state deliberately
Separate score history from save progression.

Suggested model:
- `SaveGame`
- `HubProgress`
- `UnlockedPortals`
- `RunState` or `ActiveRun`

Need to support:
- hub visual/state changes
- unlock flags
- persistent inventory/meta progression as chosen later
- multiple save slots if desired

## Recommended refactor order

### Slice 1 — structural cleanup
1. Remove duplicate `src/BroughlikeMonoGame.Desktop/Core/**` source copies from the repo.
2. Add/update docs for the new architecture target.
3. Keep `tests/GameplayCheck` as the seed of regression protection.

### Slice 2 — inventory foundation
1. Introduce item definitions + inventory model alongside existing spells.
2. Adapt tutorial spells into item-like usable definitions.
3. Replace `PlayerSpells` usage in UI/session logic.

### Slice 3 — level content model
1. Introduce dungeon/level descriptors.
2. Move spawn logic out of `GameSession` into dungeon/floor config.
3. Add support for pickups/interactables as content types.

### Slice 4 — fixed levels + hub
1. Add fixed level loading path.
2. Build an authored hub map.
3. Introduce portal definitions that choose dungeon rule sets.

### Slice 5 — persistence expansion
1. Add save game model for progression.
2. Persist hub state / unlocked portals / active run as needed.
3. Keep score history as a separate concern.

## Landed foundation
The current codebase now has the first real extensibility foundation in place:
- `DungeonDefinition` + `FloorDefinition` describe a run as explicit floors instead of hardcoding everything inside `GameSession`.
- `DungeonRegistry` lets a session know about multiple dungeon definitions instead of a single tutorial run.
- `SpawnProfile` holds per-floor monster tables plus initial spawn/treasure cadence data.
- `ILevelSource` provides a shared runtime path for both `ProceduralLevelSource` and `FixedLevelSource`.
- `LevelPlan` separates layout/content planning from live runtime actors.
- Fixed/authored floors can now load through the same `GameSession` path as procedural floors.
- `PortalDestination` + `PortalWorldObject` now allow authored floors to transition into other dungeon definitions, which is the first real hub/gate plumbing.
- `SaveGame` snapshots plus `GameSession.CreateSaveGame()` / `LoadSaveGame()` now preserve the active dungeon, floor, hp, score, and inventory state across sessions, which establishes the first run-state persistence boundary.
- Progress flags now live in `GameSession` and save with `SaveGame`, and world objects can declare simple required/granted flags. That gives hub gates a first real world-state hook instead of being purely static authored geometry.
- `ExitDefinition` + ordered `ExitRoute` rules now let authored/generated dungeon exits route back into different hubs based on inventory/progression conditions. That is the first explicit story-outcome transition model.

This is intentionally still code-first, but the architecture boundary is now pointed in the right direction for later hand-authored content and data-file loading.

## Near-term coding recommendation
The next useful refactor steps are:
1. replace hardcoded tutorial/hub dungeon construction with a more explicit content catalog/layout folder structure,
2. define richer hub-specific interactables/NPC props on top of the world-object layer,
3. expand save-state models beyond run snapshots so authored hub floors, world-state gates, unlock flags, active dungeon runs, and branch-specific story consequences can all persist cleanly.

That keeps the current gameplay stable while moving toward the hub + portal + authored-content shape.
