# Web build path

## Recommendation
Use the community WebAssembly path rather than blocking the core game architecture on browser export now.

## Why
- Stock MonoGame DesktopGL is the fastest way to validate tutorial mechanics locally.
- This machine currently has the standard MonoGame templates available, but no first-party browser target installed.
- A GitHub Pages build is still practical later via a WebAssembly-capable MonoGame fork/template (for example a MonoGame-WASM style setup) once the desktop version is stable.

## Proposed path
1. Keep gameplay code in reusable core systems under `src/BroughlikeMonoGame.Desktop/Core/`.
2. When ready for publishing, add a browser-facing host project that reuses those systems.
3. Publish the browser host as static files to GitHub Pages.

## Current status
- Desktop validation path works now.
- Web publishing path is documented but not yet scaffolded in-repo.
