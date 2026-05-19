# Broughlike MonoGame

MonoGame recreation of Jeremiah Reid's JavaScript broughlike tutorial, rebuilt with a more extensible architecture and prepared for eventual web publishing.

## Project goals
- Recreate the completed tutorial feature set.
- Keep rules, rendering, content, and persistence decoupled enough to extend.
- Preserve local markdown copies of every tutorial page under `docs/tutorial/`.
- Produce a build path that can ultimately be hosted on GitHub Pages.

## Current structure
- `docs/tutorial/` — mirrored tutorial pages in markdown
- `docs/architecture/` — implementation notes, hosting notes, and GitHub delivery status
- `src/BroughlikeMonoGame.Desktop/` — MonoGame desktop implementation
- `src/BroughlikeMonoGame.Core/` — shared gameplay/render code currently being separated for multi-host support
- `src/BroughlikeMonoGame.Web/` — initial KNI/Blazor WebAssembly host scaffold for GitHub Pages

## GitHub delivery
- GitHub Actions Pages workflow: `.github/workflows/pages.yml`
- Delivery status notes: `docs/architecture/github-delivery-status.md`
- Current Pages deployment is a placeholder/documentation site until a browser-capable host project is added.

## Source tutorial
- <https://nluqo.github.io/broughlike-tutorial/>
