# Interactables and Authored Floors

This is the practical follow-on to `game-development-workflows.md` and `toolkit-sample-pack.md`.

It focuses on two content slices that are especially important for PMD-style hub and story work:

- **weird/scripted interactables**
- **authored floors**

The goal is to make these easy to copy, validate, and extend without drifting back into one-off `GameSession` hacks.

---

## 1. Scripted interactables

### Current source of truth

- content model: `src/BroughlikeMonoGame.Core/Content/WorldObjectDefinition.cs`
- runtime object: `src/BroughlikeMonoGame.Core/World/ScriptedInteractableWorldObject.cs`
- factory wiring: `src/BroughlikeMonoGame.Core/World/WorldObjectFactory.cs`
- movement/interaction hook: `src/BroughlikeMonoGame.Core/GameSession.cs`
- validation: `src/BroughlikeMonoGame.Core/Content/ContentValidator.cs`
- canonical examples:
  - `Content/Dungeons/ApartmentIntro/ApartmentIntroDefinition.cs`
  - `Content/Dungeons/ToolkitSample/ToolkitSampleFloors.cs`

### What a scripted interactable can currently do

A `WorldObjectDefinitionKind.ScriptedInteractable` can currently:

- show a display name
- optionally show a message
- optionally block movement
- optionally spawn an item by id
- optionally place that spawned item at an offset
- choose a visual kind for rendering

That means the current system is best for:

- furniture
- NPC bump-text
- simple item-grant props
- short authored story beats

It is **not yet** a full event/cutscene system.

### Workflow

1. **Decide the interaction role**
   - purely decorative blocker
   - bump-text NPC/prop
   - item-grant prop
   - short authored story beat

2. **Place it in an authored floor**
   Add a `WorldObjectPlacement` inside a `FixedLevelSource(...)` world-object list.

3. **Choose the minimum fields needed**

Common fields:

- `DisplayName` — required
- `Message` — optional text shown on interaction
- `VisualKind` — renderer-facing visual identity
- `BlocksMovement` — whether the player bumps into it instead of stepping through
- `ItemId` — optional spawned item
- `SpawnItemOffset` — where the granted item appears relative to the prop

4. **Prefer authored content over session code**
   If the interaction only needs text + optional item grant, keep it fully in content.

5. **Add or update a regression check if behavior matters**
   Example: the dresser in apartment intro has a real `GameplayCheck` because it grants a key item that affects progression.

6. **Run validation**
   - `dotnet run --project tests/GameplayCheck`
   - `dotnet build BroughlikeMonoGame.sln`

### Canonical example patterns

#### Decorative blocker

Use this for furniture or props that simply occupy space:

- `Bed`
- `Armchair`
- `Side Table`

Pattern:
- `DisplayName`
- `VisualKind`
- `BlocksMovement: true`
- no item grant
- no message required

#### Bump-text NPC

Use this for talk-on-bump story characters:

- `Stranger`
- `Inspector`
- `Warden One/Two/Three`

Pattern:
- `DisplayName`
- `Message`
- `VisualKind: Npc`
- `BlocksMovement: true`

#### Item-grant prop

Use this for authored prop rewards and key items:

- apartment `Dresser` → grants `black-suit`
- sample `Supply Crate` → grants `power`

Pattern:
- `DisplayName`
- `Message`
- `BlocksMovement: true`
- `ItemId`
- `SpawnItemOffset`

### Design rules

- keep each interactable legible from content alone
- prefer short clear text over vague placeholder narration
- use item-grant props for authored rewards before inventing new event plumbing
- when in doubt, make the simplest version first and add framework later

### What to avoid

- writing custom `GameSession` logic for text-only props
- using interactables for behavior that should really be exit routing or world state
- stacking multiple responsibilities into one prop if two smaller props would read better

### When to improve the framework

If a real interaction needs any of the following, that is the sign for the next seam:

- grant/remove multiple flags
- trigger routing directly
- change room state after interaction
- support one-time vs repeat behavior explicitly
- run multi-step scripted sequences

That likely belongs in the future reusable hub interaction/event layer, not as ad hoc prop logic.

---

## 2. Authored floors

### Current source of truth

- authored floor runtime: `src/BroughlikeMonoGame.Core/Content/FixedLevelSource.cs`
- floor model: `src/BroughlikeMonoGame.Core/Content/FloorDefinition.cs`
- world objects: `src/BroughlikeMonoGame.Core/Content/WorldObjectDefinition.cs`
- monster placement: `src/BroughlikeMonoGame.Core/Content/MonsterPlacement.cs`
- validation: `src/BroughlikeMonoGame.Core/Content/ContentValidator.cs`
- canonical examples:
  - `Content/Dungeons/ApartmentIntro/ApartmentIntroDefinition.cs`
  - `Content/Dungeons/HubStart/HubStartDefinition.cs`
  - `Content/Dungeons/HubSuccess/HubSuccessDefinition.cs`
  - `Content/Dungeons/HubFailure/HubFailureDefinition.cs`
  - `Content/Dungeons/ToolkitSample/ToolkitSampleFloors.cs`

### Current authored-floor model

An authored floor is a `FloorDefinition` using `FixedLevelSource`.

A fixed floor currently supports:

- square row data
- `#` wall tiles
- `.` floor tiles
- `@` player start
- `>` exit tile
- authored monster placements
- authored world-object placements

### Workflow

1. **Choose whether this should be authored or generated**

Use `FixedLevelSource` when you want:
- a hub room
- a story room
- a puzzle-like room
- bespoke furniture/NPC placement
- tight control over pacing and movement

2. **Sketch the floor as rows first**

Current supported markers:

- `#` = wall
- `.` = floor
- `@` = player start
- `>` = exit

Rules:
- rows must be square and same width
- there must be one `@`
- there must be one `>`

3. **Add authored content placements**

Then layer in:
- `worldObjects:` for props/NPCs/portals/item pickups
- `monsters:` for exact enemy placement when needed

4. **Attach a spawn profile even if the floor is calm**

Even fixed floors still need a `SpawnProfile`.

For non-combat hubs/scenes, it is fine to use a quiet profile like:
- no monsters
- no treasure
- no item table usage

5. **Define exit routing clearly**

Use `ExitDefinition` + `ExitRoute` to say:
- where the floor exits go
- what item/flag conditions apply
- what message/label the route uses
- whether the route updates `WorldState.currentStart`

6. **Validate and smoke-test**

Minimum loop:
- `dotnet run --project tests/GameplayCheck`
- `dotnet build BroughlikeMonoGame.sln`

If the room matters narratively or mechanically, add a targeted regression check too.

### Canonical example patterns

#### One-room hub

See:
- `HubStartDefinition`
- `HubSuccessDefinition`
- `HubFailureDefinition`

Use this pattern for:
- return hubs
- small staging areas
- temporary landing rooms

#### Multi-floor authored sequence

See:
- `ApartmentIntroDefinition`

Use this pattern for:
- story sequences
- movement-controlled transitions between rooms
- key-item-gated authored progression

#### Authored reference room

See:
- `ToolkitSampleFloors` → `Sample Antechamber`

Use this as the copy-from-here version when you want the cleanest current example.

### Practical authored-floor checklist

Before calling a floor done, verify:

- the row layout is square
- `@` and `>` both exist
- important props are on passable tiles
- blockers intentionally block movement
- portals/exits point to real destinations
- item ids are valid
- progression conditions are explicit
- the floor validates through `ContentValidator`

### What to avoid

- hiding authored-room logic in `GameSession`
- using a generated floor when the goal is precise narrative staging
- mixing too many unrelated room ideas into one floor file
- relying on memory for portal/item ids instead of validation/tests

---

## 3. Recommended division of responsibility

### Use interactables when:
- the thing is local to a tile/object
- the result is text, blockage, or a single item grant

### Use exit routes when:
- the thing is really a destination decision
- the result is progression-based travel
- the next default start location should change

### Use world state when:
- the thing should persist beyond the current run
- future hub/dungeon availability depends on it

That split keeps authored content understandable.

---

## 4. Best current examples to copy

If you only want one file to imitate for each slice:

- **scripted interactable:** apartment `Dresser` in `ApartmentIntroDefinition.cs`
- **authored floor:** `Sample Antechamber` in `ToolkitSampleFloors.cs`
- **multi-room authored flow:** `ApartmentIntroDefinition.cs`
- **simple hub floor:** `HubSuccessDefinition.cs`

---

## 5. Validation loop

Whenever you add or change interactables/authored floors:

```bash
dotnet run --project tests/GameplayCheck
dotnet build BroughlikeMonoGame.sln
```

The content validator should catch common wiring problems early, but if the floor has real gameplay/progression meaning, still add a focused `GameplayCheck`.
