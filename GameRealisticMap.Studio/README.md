# GameRealisticMap Studio

Game Realistic Map Studio is the WPF desktop application for GameRealisticMap. It covers the full lifecycle of an Arma 3 terrain: initial configuration, automated generation from real-world data, and direct interactive editing of the result.

---

## What It Does

### Map Generation
- Configure the terrain area (center/SW corner, grid size, resolution) with an interactive map picker
- Select and configure region asset libraries (3D models, surface materials)
- Run the full generation pipeline (OSM download → builder pipeline → WRP/PBO compilation) with live progress reporting
- Browse the Arma 3 P3D model library from the configured game installation

### Interactive Terrain Editing
The **Arma3WorldEditor** module allows direct editing of a generated terrain without re-running the full pipeline:
- Move, replace, or delete placed objects
- Change ground material across selected areas
- Adjust elevation
- All changes are reflected as WRP patches applied on top of the generated base

Changes made in-game via the **@ArmaMapStudio** Arma 3 mod can be imported back into Studio (via Eden editor export), allowing an in-game → Studio editing workflow.

### Asset Configuration
- Edit building definitions, forest cluster collections, material libraries, fence/rail/row definitions
- Define multi-component object assemblies (`Composition` tool)
- Configure object placement density for forests, scrub, and other generated layers
- Edit OSM tag-filter conditions for fine-grained feature control

---

## Module Structure

| Module | Description |
|--------|-------------|
| `Main` | Application shell, document management, menu/toolbar |
| `MapConfigEditor` | Visual editor for `Arma3MapConfig` (area, resolution, world name, PBO prefix, etc.) |
| `GenericMapConfigEditor` | Equivalent config editor for generic (non-Arma3) profiles |
| `Arma3WorldEditor` | Interactive WRP terrain editor with live object/material/elevation editing |
| `Arma3Data` | Loads and caches Arma 3 game data from a configured game installation |
| `AssetBrowser` | Browses the loaded P3D model library |
| `AssetConfigEditor` | Edits region asset configuration files (`IArma3RegionAssets` JSON) |
| `CompositionTool` | Builds multi-component object assemblies |
| `DensityConfigEditor` | Adjusts object placement density parameters |
| `ConditionTool` | Visual editor for OSM tag-filter conditions |
| `Explorer` | File system browser panel |
| `Reporting` | Progress and error reporting panel |

---

## Development Conventions

For development conventions (MVVM patterns, XAML, MEF, localization with `Labels.resx`), see [`INSTRUCTIONS.md`](INSTRUCTIONS.md).

---

## See Also

- [`@ArmaMapStudio/README.md`](../@ArmaMapStudio/README.md) — the companion in-game Arma 3 mod
- [`docs/architecture/studio-guide.md`](../docs/architecture/studio-guide.md) — detailed module reference
- [`GameRealisticMap/README.md`](../GameRealisticMap/README.md) — core engine and builder pipeline
- [`GameRealisticMap.Arma3/README.md`](../GameRealisticMap.Arma3/README.md) — Arma 3 output layer
