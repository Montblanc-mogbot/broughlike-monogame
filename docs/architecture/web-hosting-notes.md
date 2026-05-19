# Web Hosting Notes

## Goal
Host a playable build on GitHub Pages.

## Chosen runtime direction
Use KNI's BlazorGL WebAssembly target as the browser-facing MonoGame-compatible host.

## Why
- Static-site output fits GitHub Pages.
- KNI keeps the game loop in C# and stays close to MonoGame/XNA APIs.
- This is a more direct path than maintaining a separate JavaScript gameplay port.

## Repo status
- Added `src/BroughlikeMonoGame.Web/` as an initial browser host scaffold.
- Added score-storage abstraction so browser persistence can diverge from desktop file persistence.
- Confirmed the needed package/tool pattern from live KNI example docs.

## Remaining engineering task
The current shared core still imports MonoGame framework types directly, which prevents clean reuse from the KNI web host without assembly conflicts. Finish the host/domain split before expecting a successful wasm publish.

## GitHub Pages publish target
Once the type split is complete:
1. `dotnet publish src/BroughlikeMonoGame.Web/BroughlikeMonoGame.Web.csproj -c Release`
2. Deploy `src/BroughlikeMonoGame.Web/bin/Release/net8.0/publish/wwwroot/`
3. Ensure the site is served with `<base href="./" />` for project-page compatibility
