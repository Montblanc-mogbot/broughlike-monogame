# Game-Building Workflows

This folder is the practical instruction set for building the actual game on top of the current toolkit.

Use these docs when turning an idea into a playable slice.

## Recommended order

1. `01-planning-a-content-slice.md`
2. `02-building-a-hub-variant.md`
3. `03-building-a-dungeon-package.md`
4. `04-connecting-a-full-loop.md`
5. `05-validating-and-landing-a-slice.md`

## What these workflows assume

The repo already supports:

- authored floors via `FixedLevelSource`
- generated floors via `ProceduralLevelSource`
- dungeon-to-dungeon routing via `ExitRoute` and `PortalDestination`
- readable world-state checkpoints via `WorldState.currentStart`
- simple scripted props/NPC bump text via `ScriptedInteractable`
- content validation via `ContentValidator`
- sample copyable content under `Content/Dungeons/ToolkitSample/**`

## When to use the older docs

These workflow docs sit on top of the more focused references:

- `docs/game-development-workflows.md`
- `docs/interactables-and-authored-floors.md`
- `docs/toolkit-sample-pack.md`

Use those when you need API-level or seam-level details.
