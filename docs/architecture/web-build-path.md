# Web build path

## Recommendation
Use a KNI BlazorGL browser host in-repo so the MonoGame-style game loop and most gameplay code can stay in C# while publishing static files to GitHub Pages.

## Why this path won
- It honors the MonoGame requirement more closely than a JavaScript rewrite.
- KNI is MonoGame-compatible and explicitly supports browser/WebAssembly targets.
- GitHub Pages can host the produced static `wwwroot` output.
- This repo's architecture already isolates most gameplay logic enough to reuse in a second host.

## What is now in the repo
- `src/BroughlikeMonoGame.Web/` — initial Blazor WebAssembly + KNI host scaffold.
- Solution entries for `BroughlikeMonoGame.Core` and `BroughlikeMonoGame.Web`.
- Browser-specific static shell files under `src/BroughlikeMonoGame.Web/wwwroot/`.
- Score persistence abstraction in core (`IScoreStorage`) so desktop can keep file saves and browser can swap storage later.

## Current blocker
The shared `BroughlikeMonoGame.Core` project still depends directly on MonoGame graphics/input types (`Microsoft.Xna.Framework.*`). That compiles fine when referenced from the desktop MonoGame host, but a standalone core project without a graphics framework reference does not build yet, and adding MonoGame references collides with KNI's `Xna.Framework.*` assemblies in the web project.

In short: the fastest reliable next step is a type-boundary split, not more host scaffolding.

## Exact next refactor to finish the web path
1. Split `BroughlikeMonoGame.Core` into:
   - a pure gameplay/domain assembly with no MonoGame/KNI graphics/input types, and
   - a renderer/host-facing assembly per platform (desktop MonoGame, web KNI).
2. Replace direct `Color`, `SpriteBatch`, `Texture2D`, `SpriteFont`, and `KeyboardState` usage in core-facing code with host adapters or plain DTOs.
3. Make the KNI web host own the render loop and call into the pure gameplay session.
4. Publish the web project and copy `bin/Release/net8.0/publish/wwwroot/` to GitHub Pages.

## Validation evidence
- `dotnet build BroughlikeMonoGame.sln -c Debug` succeeds for the desktop solution before the host split.
- KNI docs and example project confirm the browser host pattern (`BlazorWebAssembly` + `nkast.Kni.Platform.Blazor.GL`).
- The current repo now contains a concrete web host scaffold, but the build is blocked by MonoGame-vs-KNI type collisions until the core split is completed.
