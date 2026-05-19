# Broughlike MonoGame

MonoGame recreation of Jeremiah Reid's JavaScript broughlike tutorial, now serving as the foundation for a more original mystery-dungeon-style roguelike with an extensible architecture and web publishing support.

## Project goals
- Recreate the completed tutorial feature set.
- Use that tutorial build as a stepping stone toward an original mystery-dungeon framework.
- Keep rules, rendering, content, generation, and persistence decoupled enough to extend.
- Preserve local markdown copies of every tutorial page under `docs/tutorial/`.
- Support browser publishing on GitHub Pages.

## Current structure
- `docs/tutorial/` — mirrored tutorial pages in markdown
- `docs/architecture/` — implementation notes, hosting notes, GitHub delivery status, and refactor plans
- `src/BroughlikeMonoGame.Desktop/` — MonoGame desktop host
- `src/BroughlikeMonoGame.Core/` — shared gameplay core
- `src/BroughlikeMonoGame.Web/` — browser host for GitHub Pages builds
- `tests/GameplayCheck/` — focused regression checks for core gameplay behavior

## GitHub delivery
- GitHub Actions Pages workflow: `.github/workflows/pages.yml`
- Delivery status notes: `docs/architecture/github-delivery-status.md`
- Current Pages deployment targets the browser host.
+
+## Next architecture pass
+The next major priority is extensibility rather than tutorial parity. See:
+- `docs/architecture/extensibility-refactor-plan.md`
+
+This pass is aimed at enabling:
+- inventory-driven items/pickups instead of fixed spell slots
+- non-enemy interactables
+- configurable spawn tables and dungeon descriptors
+- fixed/authored levels alongside generated floors
+- hub progression and persistent save state

## Source tutorial
- <https://nluqo.github.io/broughlike-tutorial/>
