# 02. Building a Hub Variant

Use this workflow when adding a new authored hub or a new authored variant of an existing hub state.

## Best references right now

- `src/BroughlikeMonoGame.Core/Content/Dungeons/HubStart/HubStartDefinition.cs`
- `src/BroughlikeMonoGame.Core/Content/Dungeons/HubSuccess/HubSuccessDefinition.cs`
- `src/BroughlikeMonoGame.Core/Content/Dungeons/HubFailure/HubFailureDefinition.cs`
- `src/BroughlikeMonoGame.Core/Content/Dungeons/ToolkitSample/ToolkitSampleFloors.cs`

## The pattern

A hub variant should usually be:

- one authored dungeon package or floor
- one clear floor name
- a few authored props/NPCs
- one or more clear onward routes
- no bespoke session code unless the slice proves a missing seam

## Step-by-step

1. **Choose whether this is a new hub id or a new variant id**
   - new hub id when it is a distinct destination
   - variant id when success/failure should land in a visibly different authored space

2. **Create or edit the hub definition file**
   Use `FixedLevelSource`.

3. **Name the space clearly**
   Use a dungeon display name and floor display name that communicate tone and role.

4. **Sketch the room rows**
   Include:
   - one `@`
   - one `>`
   - enough room for props and bump interactions

5. **Add the authored interactables**
   Prefer:
   - 1-3 NPC/prop interactions
   - short text
   - clear blocking objects
   - optional item-grant props only when needed

6. **Define the onward route**
   Most hub variants should have one obvious next route.
   Use `ExitDefinition` + `ExitRoute`.

7. **Register or reuse the dungeon id in routing**
   Make sure some earlier content can actually send the player here.

8. **Add a regression check**
   Usually prove:
   - the player can route into the hub, or
   - the player can route out of the hub, or
   - the hub becomes the right `currentStart`

## Design rules

- Use separate hub variants for clearly different states before inventing finer-grained hub mutation.
- Let the route decide which hub variant to use.
- Treat hub props as authored scene-setting and soft guidance.
- Keep each hub’s exit easy to discover during tests.

## Good first questions while building

- What does this hub variant mean emotionally?
- Why is the player here after this outcome?
- What does the player read or learn before leaving?
- Is this actually a different hub, or just different text in the same room?

## Validation loop

```bash
dotnet run --project tests/GameplayCheck
dotnet build BroughlikeMonoGame.sln
```

Also run `ContentValidator` indirectly via `GameplayCheck` by ensuring the default registry still validates.
