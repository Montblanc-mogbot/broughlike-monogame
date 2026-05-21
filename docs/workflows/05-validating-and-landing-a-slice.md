# 05. Validating and Landing a Slice

Every slice should land with proof.

## Minimum required loop

For most content slices:

```bash
dotnet run --project tests/GameplayCheck
dotnet build BroughlikeMonoGame.sln
```

## What each step protects

### `GameplayCheck`

Protects against:
- bad routing
- broken content references
- invalid authoring assumptions
- regression in run/world-state flow

### `dotnet build`

Protects against:
- compile breaks across desktop/web/core
- stale API assumptions
- integration drift

## When a new test is worth adding

Add or update a test when the slice changes any of these:

- dungeon routing
- world-state checkpoint behavior
- item-conditioned exits
- authored prop behavior with real gameplay meaning
- hub entry/exit flow
- content validation behavior

If the new slice changes player flow and there is no test for that change, the slice is under-protected.

## Content review checklist before commit

- ids are readable and stable
- routes point to real dungeon/floor targets
- item refs are valid
- authored blockers are intentionally placed
- exit banners/labels make sense
- the new content has one obvious playable role
- the slice is small enough to explain in one commit message

## Commit guidance

Use commit messages that describe the content/toolkit change clearly.

Good examples:
- `feat(hubs): add authored post-apartment landing hub`
- `feat(authoring): validate content references`
- `docs(samples): add canonical toolkit sample pack`

## After landing

Update:

- project task notes in the workspace
- project context notes in the workspace
- daily memory if the slice changed direction or unlocked a new pattern

The point is not paperwork. The point is making the next slice easier.
