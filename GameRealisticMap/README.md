# GameRealisticMap — Core Engine

This is the game-agnostic core library of GameRealisticMap. It provides the full map-data pipeline: downloading real-world geospatial data, processing it into typed feature datasets, and exposing those datasets to game-specific output layers.

---

## Responsibilities

- Downloading and caching **OpenStreetMap**, **NASA SRTM**, and **Sentinel-2** data for a given geographic area
- Parsing OSM ways/nodes/relations into typed terrain features (roads, buildings, forests, lakes, elevation…)
- Providing the **builder pattern** infrastructure: `IDataBuilder<T>`, `IBuidersCatalog`, `BuildContext`
- Geometry and coordinate-space utilities (`TerrainPoint`, `ITerrainArea`, `TerrainAreaUTM`)
- Progress reporting integration via `IProgressScope`

This library has **no dependency on any specific game**. Game-specific output (Arma 3 WRP/PBO, etc.) lives in `GameRealisticMap.Arma3` and `GameRealisticMap.Generic`.

---

## Builder Pattern

Every terrain feature type is produced by exactly one `IDataBuilder<T>`:

```csharp
public class ForestData { public IReadOnlyList<TerrainPolygon> Polygons { get; } }

public class ForestBuilder : IDataBuilder<ForestData>
{
    public ForestData Build(IBuildContext context, IProgressScope scope)
    {
        // read context.OsmSource, return ForestData
    }
}
```

Builders are registered in `BuildersCatalog` and resolved lazily and concurrently by `BuildContext.GetData<T>()`. Each data type is built **at most once** per run; results are cached for the lifetime of the context.

Dependencies between builders are declared naturally: a builder calls `context.GetData<OtherData>()` inside its `Build()` method, and the framework waits for that dependency automatically.

`BuildContext.SetData<T>(value)` can inject pre-built data, bypassing the builder — used in tests and in Studio live editing.

See [`docs/architecture/data-pipeline.md`](../docs/architecture/data-pipeline.md) for full details.

---

## Key Interfaces

| Interface | File | Role |
|-----------|------|------|
| `IContext` | `IContext.cs` | Base: `GetData<T>()`, `HugeImageStorage` |
| `IBuildContext` | `IBuildContext.cs` | Extends `IContext` with `Area`, `OsmSource`, `Options` |
| `IDataBuilder<T>` | `IDataBuilder.cs` | One per data type, implements `Build()` |
| `IBuidersCatalog` | `IBuidersCatalog.cs` | Registry: `Register<T>()`, `Get<T>()` |
| `ITerrainArea` | `ITerrainArea.cs` | Geographic bounds + `LatLngToTerrainPoint()` |
| `IMapProcessingOptions` | `IMapProcessingOptions.cs` | Resolution, road thresholds, satellite config |

> **Note:** `IBuidersCatalog` has a historical typo ("Buiders", missing 'd'). Do not rename it.

---

## Coordinate System

All internal geometry uses **`TerrainPoint`** — a 2D coordinate in metres from the south-west corner of the terrain:

- `X` = East, `Y` = North, origin `(0, 0)` = south-west corner
- Backed by `System.Numerics.Vector2` (`float`), ~1 cm precision up to 83 km
- `ITerrainArea.LatLngToTerrainPoint(Coordinate)` converts WGS-84 → terrain space
- Standard implementation: `TerrainAreaUTM` (UTM projection via CoordinateSharp)

See [`docs/architecture/coordinate-system.md`](../docs/architecture/coordinate-system.md) for full details.

---

## Directory Structure

| Directory | Contents |
|-----------|----------|
| `ManMade/` | Builders + data types for roads, buildings, railways, fences, airports, etc. |
| `Nature/` | Builders + data types for forests, lakes, watercourses, rocks, scrub, etc. |
| `ElevationModel/` | `ElevationGrid`, elevation builders, lake flattening, watercourse grading |
| `Geometries/` | `TerrainPoint`, `TerrainPolygon`, `TerrainPath`, Clipper integration |
| `Osm/` | OSM data loading (OverPass API), `IOsmDataSource`, tag parsing |
| `Satellite/` | Sentinel-2 tile downloading and assembly |
| `Images/` | `HugeImage` helpers, raster processing |
| `Conditions/` | Tag-filter DSL (`TagFilterLanguage`) used in asset config |
| `Configuration/` | `IMapProcessingOptions`, `ISourceLocations`, `DefaultBuildersConfig` |
| `Algorithms/` | Geometry algorithms (polygon offset, radial generation, density placement) |
| `Preview/` | HTML preview renderer (for debugging / CLI preview) |
| `Reporting/` | `IProgressScope` wrappers and console rendering |

---

## Adding a New Feature

1. Create `MyFeatureData.cs` in `ManMade/` or `Nature/`.
2. Create `MyFeatureBuilder.cs` implementing `IDataBuilder<MyFeatureData>`.
3. Register in `BuildersCatalog.cs`.
4. (Game-specific) Add a generator in `GameRealisticMap.Arma3/` or `GameRealisticMap.Generic/`.

See [`docs/architecture/data-pipeline.md`](../docs/architecture/data-pipeline.md) for conventions.
