# GameRealisticMap.Generic — Generic Export Layer

This library provides a game-agnostic export path for terrain data produced by `GameRealisticMap`. It is the foundation for adding support for games other than Arma 3 in the future.

---

## Responsibilities

- Running the standard `GameRealisticMap` builder pipeline against a `GenericMapConfig`
- Exporting terrain features to generic formats (GeoJSON, PNG previews, shapefiles, etc.) via a pluggable exporter system
- Defining `ExportProfile` — a configuration file that selects which exporters to run and with what parameters

---

## Entry Point

`GenericMapGenerator` drives the full pipeline:

1. Downloads OSM data via OverPass API
2. Creates a `BuildContext` with the standard `BuildersCatalog`
3. Runs all exporters registered in the `ExportProfile`
4. Writes output files to `GenericMapConfig.TargetDirectory`

---

## Export Profile

An `ExportProfile` (JSON) lists one or more `ExportEntry` items. Each entry specifies:
- An exporter type (by name, from `ExporterCatalog`)
- Per-exporter configuration (output filename, format options, etc.)

`ExporterCatalog` discovers available exporters. Custom exporters implement `IExporter` and `IExporterInfo`.

---

## Available Exporters

Base classes in `Exporters/` cover the common patterns:

| Base class | Description |
|------------|-------------|
| `PolygonExporterBase` | Exports polygon feature data (forests, lakes, land-use, etc.) |
| `PathExporterBase` | Exports line feature data (roads, watercourses, fences, etc.) |
| `PointExporterBase` | Exports point feature data (trees, oriented objects, etc.) |
| `ImageExporterBase` | Exports raster data (elevation, satellite imagery) |
| `ShapeExporterBase` | Exports to shapefile format |
| `BasicTerrainExporter` | Exports a combined terrain overview |

Feature-specific exporters live in `Exporters/ManMade/` and `Exporters/Nature/`, mirroring the builder structure of `GameRealisticMap`.

---

## Configuration: `GenericMapConfig`

`GenericMapConfig` mirrors `Arma3MapConfig` for the generic pipeline:

| Property | Description |
|----------|-------------|
| `TerrainArea` | `ITerrainArea` — geographic bounds and coordinate conversion |
| `TargetDirectory` | Output directory for exported files |
| `ExportProfileFile` | Path to the `ExportProfile` JSON |

---

## Role in Multi-Game Architecture

`GameRealisticMap.Generic` is the designated extension point for future game targets:
- Add a new `IExporter` implementation for the target game's format
- Register it in `ExporterCatalog`
- The full builder pipeline (OSM, elevation, satellite, all feature types) is already available through `BuildContext`

See [`GameRealisticMap/README.md`](../GameRealisticMap/README.md) for the core pipeline and builder pattern.
