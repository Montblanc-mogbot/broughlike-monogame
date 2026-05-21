# 03. Building a Dungeon Package

Use this workflow when adding a new playable dungeon package, whether generated, authored, or mixed.

## Best references right now

- generated pattern: `Content/Dungeons/TutorialDungeon/**`
- mixed sample pattern: `Content/Dungeons/ToolkitSample/**`
- authored sequence pattern: `Content/Dungeons/ApartmentIntro/**`

## Decide the dungeon role first

A dungeon package should have a clear job:

- tutorial/training
- retrieval run
- survival run
- story sequence
- special authored room chain

If the role is fuzzy, the content will sprawl.

## Step-by-step

1. **Create a new package directory**
   Recommended shape:
   - `<DungeonName>Definition.cs`
   - `<DungeonName>Floors.cs`
   - optional `<DungeonName>SpawnProfiles.cs`

2. **Pick the floor source type per floor**
   - `FixedLevelSource` for authored rooms and scenes
   - `ProceduralLevelSource` for normal run floors

3. **Define the spawn ecology**
   Right now this means a `SpawnProfile`.
   Choose:
   - monster table
   - item table if needed
   - floor item count
   - enemy item drop count
   - treasure count

4. **Define the run outcome**
   Every real dungeon should answer:
   - where does success go?
   - where does failure go?
   - does either outcome update `currentStart`?

5. **Add exit routing**
   Use `ExitDefinition` + ordered `ExitRoute`s.
   Keep routes explicit and readable.

6. **Register the package**
   Add it to `DungeonCatalog` when it should be part of the default playable game.

7. **Make it reachable from a hub or intro path**
   A package that nothing enters is not part of the game yet.

8. **Add a test**
   Prove one real contract:
   - routing
   - loading
   - progression change
   - inventory-conditioned outcome

## Design rules

- Prefer one strong dungeon identity over generic filler.
- Keep the first version shallow.
- Route to different hub variants instead of overcomplicating one hub.
- Use items and exits to prove progression before inventing larger systems.

## When to stop and improve the framework

Pause and improve seams if the dungeon needs:

- richer objectives than “reach exit”
- room-type-aware spawn logic
- special rooms inserted into generated content
- more expressive encounter scripting

That is the signal for the next dungeon-ecology layer.
