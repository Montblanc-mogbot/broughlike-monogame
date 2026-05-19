# Web Hosting Notes

## Goal
Host a playable build on GitHub Pages.

## Constraint
Official MonoGame templates installed locally currently target desktop/mobile, not web. A GitHub Pages deployment therefore needs either:

1. a MonoGame-compatible WebAssembly target/fork, or
2. a separate browser host layer that can run the shared game code.

## Current candidate paths
- MonoGame-Wasm / related WebAssembly forks
- KNI browser/blazor-based hosting path

## Practical next step
Finish the desktop implementation first, keep gameplay code isolated from the host layer, then add a web-specific frontend once the exact runtime path is proven in this environment.
