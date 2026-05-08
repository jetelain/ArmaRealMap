# GameRealisticMap — Architecture Overview

## What Is It?

GameRealisticMap (GRM) is a C# toolchain that **generates and edits** realistic game maps from real-world geospatial data. It downloads OpenStreetMap cartography, NASA SRTM elevation data, and Sentinel-2 satellite imagery, then processes them into playable Arma 3 terrain packages (`.pbo` mods).

Beyond automated generation, GRM supports **interactive terrain editing** through two complementary tools:
- **Game Realistic Map Studio** (`GameRealisticMap.Studio`) — a WPF desktop application for configuring, generating, and **directly editing** the terrain (objects, materials, elevation) with live preview, without a full regeneration.
- **@ArmaMapStudio Arma 3 mod** (`@ArmaMapStudio/`) — an in-game mod that integrates with Arma 3's Eden editor to export placed objects back to GRM Studio, and supports in-game hidden-object editing.

The system is designed for fully-automated, batch-driven generation while remaining configurable for custom map designs. **Currently only Arma 3 is supported as a generation target**, but the architecture is intentionally split between a game-agnostic core (`GameRealisticMap`) and game-specific output layers (`GameRealisticMap.Arma3`, `GameRealisticMap.Generic`) to enable support for other games in the future.

---

## Data Sources (all downloaded automatically)

| Source | What it provides | License requirement |
|--------|-----------------|---------------------|
| [OpenStreetMap](https://www.openstreetmap.org/) via [Overpass API](https://overpass-api.de/) | Roads, buildings, land-use, water, railways, trees, etc. | © OpenStreetMap Contributors — ODbL |
| [NASA SRTM](https://www2.jpl.nasa.gov/srtm/) | Elevation (height) data at 30 m resolution | NASA — Shuttle Radar Topography Mission |
| [Sentinel-2 cloudless](https://s2maps.eu) by EOX | Real satellite imagery for texture | CC-BY 4.0 |

Generated maps **must** credit all three sources. See [README.md](../../README.md) for the exact credit strings required.

---

## Solution Structure

Each project has its own `README.md` with a focused description. The table below summarises roles and links to those files.

| Project | Type | Role |
|---------|------|------|
| [`GameRealisticMap`](../../GameRealisticMap/README.md) | Class library | Core engine: builder pattern, geometry, terrain features (man-made & nature), elevation, satellite |
| [`GameRealisticMap.Arma3`](../../GameRealisticMap.Arma3/README.md) | Class library | Arma 3 integration: WRP/PBO generation, TerrainBuilder output, material layers, asset system |
| [`GameRealisticMap.Generic`](../../GameRealisticMap.Generic/README.md) | Class library | Non-Arma3 export formats — foundation for future game targets |
| [`GameRealisticMap.Studio`](../../GameRealisticMap.Studio/README.md) | WPF application | GUI for map configuration, generation, asset browser, and **interactive terrain editing** (objects, materials, elevation) — see [studio-guide.md](studio-guide.md) |
| `GameRealisticMap.CommandLine` | Console app | Command-line interface for batch map generation (preview rendering) |
| [`GameRealisticMap.Arma3.CommandLine`](../../GameRealisticMap.Arma3.CommandLine/README.md) | Console app | Arma 3-specific CLI (`genmod`, `genwrp`, `gentb`, `layer` verbs) |
| [`@ArmaMapStudio`](../../@ArmaMapStudio/README.md) | Arma 3 mod | In-game mod: Eden editor export to GRM Studio, in-game hidden-object editing |
| `bis-file-formats` | Git submodule | Low-level Arma 3 file format readers/writers (WRP, P3D, PAA, PBO, RTM, SQFC) |

---

## Key Design Principles

1. **Plugin-based builder pattern** — Every data type is produced by a dedicated `IDataBuilder<T>`. Builders declare their output type; the framework resolves and caches them automatically. See [data-pipeline.md](data-pipeline.md).

2. **Lazy evaluation** — Data is only built when first requested via `IContext.GetData<T>()`. Unused data types cost nothing.

3. **Separation of concerns** — Generic map features (forests, roads, buildings…) live in `GameRealisticMap`. Arma 3-specific rendering (WRP generation, PBO packaging, object placement) lives in `GameRealisticMap.Arma3`.

4. **Coordinate abstraction** — All geometry uses local terrain space (`TerrainPoint`, metres from south-west origin). Coordinate conversion to/from WGS-84 is handled by `ITerrainArea`. See [coordinate-system.md](coordinate-system.md).

---

## High-Level Pipeline

```
  ┌──────────────────────────────────────────────────────────┐
  │  Configuration (Arma3MapConfig / GenericMapConfig)       │
  │    World name, area center/SW, grid size, resolution…    │
  └───────────────────────┬──────────────────────────────────┘
                          │
  ┌───────────────────────▼──────────────────────────────────┐
  │  BuildContext + BuildersCatalog                          │
  │    OSM data (OverPass) + SRTM (NASA) + Sentinel-2        │
  └───────────────────────┬──────────────────────────────────┘
                          │  lazy GetData<T>() calls
             ┌────────────┼────────────┐
             │            │            │
  ┌──────────▼──┐  ┌──────▼──┐  ┌──────▼───────┐
  │ Nature data │  │ ManMade │  │  Elevation   │
  │ Forests     │  │ Roads   │  │  + Satellite │
  │ Lakes, etc. │  │ Bldgs…  │  │              │
  └──────────┬──┘  └──────┬──┘  └──────┬───────┘
             └────────────┴────────────┘
                          │
  ┌───────────────────────▼──────────────────────────────────┐
  │  Arma3LayerGeneratorCatalog                              │
  │    Generic *Data → TerrainBuilderObjects (CSV)           │
  └───────────────────────┬──────────────────────────────────┘
                          │
  ┌───────────────────────▼──────────────────────────────────┐
  │  WRP + PBO compilation                                   │
  │    Elevation grid, imagery tiles, object placement       │
  └──────────────────────────────────────────────────────────┘
```

For detailed builder inventory, see [builders-reference.md](builders-reference.md).  
For Arma 3-specific output details, see [arma3-integration.md](arma3-integration.md).

---

## Technology Stack

| Technology | Purpose |
|-----------|---------|
| .NET 8 / C# | Runtime |
| [OsmSharp](https://github.com/OsmSharp/core) | Parsing OSM XML/PBF data |
| [CoordinateSharp](https://github.com/Tronald/CoordinateSharp) | UTM ↔ WGS-84 coordinate conversion |
| [NetTopologySuite](https://github.com/NetTopologySuite/NetTopologySuite) / GeoAPI | Geometry operations |
| [ClipperLib](http://www.angusj.com/delphi/clipper.php) | Polygon boolean operations (union, intersect, diff) |
| [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp) | Raster image processing |
| [Pmad.HugeImages](https://github.com/jetelain/HugeImages) | Memory-mapped huge image storage |
| [Pmad.ProgressTracking](https://github.com/jetelain/ProgressTracking) | Progress reporting (`IProgressScope`) |
| [Gemini](https://github.com/tgjones/gemini) | WPF IDE framework for Studio |
| `bis-file-formats` | Arma 3 binary file format I/O |
