# GameRealisticMap — AI Assistant Instructions

## What This Project Does

GameRealisticMap (GRM) is a C# toolchain that **generates and edits** realistic Arma 3 game maps from real-world geospatial data. It combines:
- **OpenStreetMap** data (roads, buildings, land use, water, trees…) via the Overpass API
- **NASA SRTM** elevation data (automatic download)
- **Sentinel-2 cloudless** satellite imagery (automatic download)

The output is a fully playable Arma 3 map distributed as a `.pbo` mod. **Arma 3 is currently the only supported generation target**, but the architecture separates a game-agnostic core (`GameRealisticMap`) from game-specific output layers to enable support for other games in the future.

Beyond generation, GRM also supports **interactive map editing** through two complementary tools:
- **Game Realistic Map Studio** (`GameRealisticMap.Studio`) — a WPF desktop application that lets users configure, generate, and **directly edit** the terrain (objects, materials, elevation) with live preview, without requiring a full regeneration.
- **@ArmaMapStudio Arma 3 mod** (`@ArmaMapStudio/`) — an in-game mod that integrates with Arma 3's Eden editor to export placed objects back to GRM Studio, and supports in-game hidden-object editing.

---

## Project Structure

Each project has a `README.md` at its root with a focused description.

| Project | Purpose |
|---------|---------|
| `GameRealisticMap/` | Core engine: builder pattern, geometry, terrain feature data builders |
| `GameRealisticMap.Arma3/` | Arma 3 output: WRP/PBO generation, object placement, satellite textures |
| `GameRealisticMap.Generic/` | Non-Arma3 export formats — foundation for future game targets |
| `GameRealisticMap.Studio/` | WPF GUI application (Gemini framework) — map generation, configuration, and interactive terrain editing |
| `GameRealisticMap.CommandLine/` | CLI entry point |
| `GameRealisticMap.Arma3.CommandLine/` | Arma 3-specific CLI (`genmod`, `genwrp`, `gentb`, `layer` verbs) |
| `@ArmaMapStudio/` | Arma 3 mod — Eden editor export to Studio, in-game object editing |
| `bis-file-formats/` | Git submodule — low-level Arma 3 file format I/O (WRP, PBO, P3D, PAA…) |

---

## Core Pattern: Typed Data Builders

The entire pipeline is driven by this pattern:

1. **Every feature type** has a `*Data` class (plain container) and a `*Builder : IDataBuilder<*Data>` class.
2. All builders are registered in `BuildersCatalog` (or Arma3-specific catalog).
3. `BuildContext` resolves and lazily builds data on demand via `GetData<T>()`.
4. Results are **cached** — each type is built at most once per run.
5. Builders declare dependencies by calling `context.GetData<OtherData>()` inside `Build()`.

```csharp
// Typical builder structure:
public class ForestData { public IReadOnlyList<TerrainPolygon> Polygons { get; } }

public class ForestBuilder : IDataBuilder<ForestData>
{
    public ForestData Build(IBuildContext context, IProgressScope scope)
    {
        var ways = context.OsmSource.Ways; // OSM input
        // ...parse, clip, return
        return new ForestData(polygons);
    }
}
```

---

## Key Interfaces (always start here)

| Interface | File | Role |
|-----------|------|------|
| `IContext` | `GameRealisticMap/IContext.cs` | Base: `GetData<T>()`, `HugeImageStorage` |
| `IBuildContext` | `GameRealisticMap/IBuildContext.cs` | Extends `IContext` with `Area`, `OsmSource`, `Options` |
| `IDataBuilder<T>` | `GameRealisticMap/IDataBuilder.cs` | One per data type, implements `Build()` |
| `IBuidersCatalog` | `GameRealisticMap/IBuidersCatalog.cs` | Registry: `Register<T>()`, `Get<T>()` |
| `ITerrainArea` | `GameRealisticMap/ITerrainArea.cs` | Geographic bounds + `LatLngToTerrainPoint()` |
| `IMapProcessingOptions` | `GameRealisticMap/IMapProcessingOptions.cs` | Resolution, road thresholds, satellite config |

---

## Coordinate System

- All geometry uses **`TerrainPoint`** (local space, metres from south-west corner).
- X = East, Y = North. Origin (0,0) = south-west corner of the terrain.
- `ITerrainArea.LatLngToTerrainPoint(Coordinate)` converts WGS-84 → terrain space.
- Default implementation: `TerrainAreaUTM` (UTM projection via CoordinateSharp).
- Max usable range: ~83 km with ~1 cm precision (float-backed).

---

## Registered Builders

See `GameRealisticMap/BuildersCatalog.cs` for the authoritative list.

Key categories:
- **Remote data**: `RawElevationBuilder`, `RawSatelliteImageBuilder`, `WeatherBuilder`
- **ManMade**: `RoadsBuilder`, `BuildingsBuilder`, `RailwaysBuilder`, `FencesBuilder`, `FarmlandsBuilder`, `AirportBuilder`, …
- **Nature**: `ForestBuilder`, `LakesBuilder`, `WatercoursesBuilder`, `RocksBuilder`, `ScrubBuilder`, …
- **Surfaces**: `SandSurfacesBuilder`, `MeadowsBuilder`, `GrassBuilder`, `IceSurfaceBuilder`, …
- **Elevation**: `ElevationWithLakesBuilder` → `ElevationBuilder` → `ElevationContourBuilder`
- **Urban fill**: `DefaultResidentialAreasBuilder`, `DefaultCommercialAreasBuilder`, …

---

## Arma 3 Output Pipeline

```
*Data objects (from BuildersCatalog)
    ↓  Arma3LayerGeneratorCatalog (ITerrainBuilderLayerGenerator per feature)
TerrainBuilderObject[] (P3D path + X/Y/rotation/scale)
    ↓  WrpCompiler
.wrp binary terrain file (heightmap + objects + material grid)
    ↓  PboCompiler
.pbo mod package (binarized configs + textures + terrain)
```

Layer generators live in `GameRealisticMap.Arma3/ManMade/` and `GameRealisticMap.Arma3/Nature/`.

---

## Navigation Tips

- To find a feature's data model: look for `*Data.cs` in `GameRealisticMap/ManMade/` or `GameRealisticMap/Nature/`.
- To find how a feature is generated from OSM: look for `*Builder.cs` next to the data class.
- To find how a feature is rendered in Arma 3: look for `*Generator.cs` in `GameRealisticMap.Arma3/ManMade/` or `GameRealisticMap.Arma3/Nature/`.
- To find all available builders at once: `GameRealisticMap/BuildersCatalog.cs`.
- To find all Arma 3 layer generators: `GameRealisticMap.Arma3/Arma3LayerGeneratorCatalog.cs`.
- To understand asset definitions (models, materials): `GameRealisticMap.Arma3/Assets/`.

---

## Adding a New Feature (checklist)

1. Create `MyFeatureData.cs` in the appropriate `ManMade/` or `Nature/` subdirectory.
2. Create `MyFeatureBuilder.cs` implementing `IDataBuilder<MyFeatureData>`.
3. Register in `BuildersCatalog.cs`: `Register(new MyFeatureBuilder(...))`.
4. (Arma 3) Create `MyFeatureGenerator.cs` in `GameRealisticMap.Arma3/` implementing `ITerrainBuilderLayerGenerator`.
5. (Arma 3) Register in `Arma3LayerGeneratorCatalog.cs`.

---

## Important Notes

- `IBuidersCatalog` has a **typo**: "Buiders" (missing 'd'). This is intentional/historical — do not rename.
- `IDataBuilderAsync<T>` is **internal** — used only for builders that do HTTP downloads. The sync `Build()` method on it calls `.Result` on the async task.
- `BuildContext.SetData<T>()` can inject pre-built data, bypassing the builder — used in tests and Studio live-editing.
- `HugeImageStorage` is used for satellite and elevation images that are too large for normal RAM — they are memory-mapped to disk.
- All documentation lives in `docs/architecture/`. The external wiki at https://github.com/jetelain/ArmaRealMap/wiki covers end-user topics.
- For Studio-specific conventions (MVVM, XAML, MEF, localization): see `GameRealisticMap.Studio/INSTRUCTIONS.md`.
