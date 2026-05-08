# GameRealisticMap.Arma3 — Arma 3 Output Layer

This library converts the game-agnostic terrain data produced by `GameRealisticMap` into a fully playable Arma 3 mod package.

---

## Responsibilities

- Running the Arma 3-specific generation pipeline (`Arma3MapGenerator`)
- Converting `*Data` objects (from `GameRealisticMap`) into Arma 3 object placements via `ITerrainBuilderLayerGenerator`
- Managing the Arma 3 asset library (`IArma3RegionAssets`): 3D models, materials, building definitions, fence/rail/row definitions
- Generating satellite texture tiles (`.paa`) and material ID maps
- Compiling the heightmap + objects into a `.wrp` terrain file
- Packaging everything as a `.pbo` mod
- Post-generation WRP editing without full regeneration

---

## Entry Point

`Arma3MapGenerator` is the top-level class for a full generation run. It orchestrates:

1. Loading OSM data and building the `IBuildContext`
2. Running all layer generators to collect `TerrainBuilderObject` lists
3. Generating satellite/material imagery tiles (`.paa`)
4. Compiling the `.wrp` terrain file
5. Packaging the `.pbo` mod

`Arma3TerrainBuilderGenerator` is an alternative entry point that outputs TerrainBuilder-compatible CSV files instead of a compiled WRP, for manual workflow integration.

---

## Configuration: `Arma3MapConfig`

`Arma3MapConfig` implements `IMapProcessingOptions` and `IPboConfig`. Key properties:

| Property | Description |
|----------|-------------|
| `TerrainArea` | `ITerrainArea` — geographic bounds and coordinate conversion |
| `WorldName` | Arma 3 world identifier (e.g. `mymap`) |
| `PboPrefix` | PBO prefix (e.g. `z\arm\addons\mymap`) |
| `Resolution` | Metres per pixel for imagery (default `1.0`) |
| `FakeSatBlend` | `0` = procedural colours, `1` = real satellite, `0–1` = blend |
| `AssetConfigFile` | Path to the region asset configuration JSON |
| `TargetModDirectory` | Output directory for the compiled mod |

---

## Layer Generation

`Arma3LayerGeneratorCatalog` holds all `ITerrainBuilderLayerGenerator` implementations. Each generator:

1. Receives the current `IArma3MapConfig`, `IContext`, and `IProgressScope`.
2. Calls `context.GetData<TData>()` to fetch the relevant feature data.
3. Returns `Task<IEnumerable<TerrainBuilderObject>>` (object placement records).

```csharp
public interface ITerrainBuilderLayerGenerator
{
    Task<IEnumerable<TerrainBuilderObject>> Generate(IArma3MapConfig config, IContext context, IProgressScope scope);
}
```

Generators live in `ManMade/` and `Nature/`. See [`docs/architecture/builders-reference.md`](../docs/architecture/builders-reference.md#arma-3-layer-generators) for the full list.

---

## Asset System

`IArma3RegionAssets` supplies all region-specific 3D models and surface materials. A region represents a geographic/biome configuration (e.g. Central Europe, Mediterranean).

| Asset Type | Description |
|------------|-------------|
| `BuildingDefinition` | P3D model(s) for a building category + size class |
| `ClusterCollectionDefinition` | Model sets for density-clustered placement (forests, scrub) |
| `BasicCollectionDefinition` | Simple model sets for fill placement |
| `TerrainMaterialLibrary` | Maps material names → texture paths and material IDs |
| `FenceDefinition` | Model + spacing for fence/wall lines |
| `RowDefinition` | Model + spacing for vineyard/orchard rows |
| `RailwaysDefinition` | Rail track model + crossing signal models |

`Composition` groups multiple `TerrainBuilderObject` records into a single logical assembly (e.g. a building with sub-components), used by `BuildingGenerator`.

---

## Imagery Pipeline

Imagery is controlled by `FakeSatBlend`:

- **`1.0` — Real satellite**: uses Sentinel-2 tiles from `RawSatelliteImageData`
- **`0.0` — Fake satellite**: `FakeSatRender` generates a procedural texture from material colours
- **`0–1` — Blended**: linear mix of both; roads are drawn on top at full opacity

The pipeline produces:
- **SatMap** tiles (`.paa`) — colour satellite texture
- **IdMap** — per-pixel terrain material ID image

---

## WRP & PBO Compilation

`WrpCompiler` (in `GameEngine/`) writes the binary `.wrp` file:
1. Heightmap grid from `ElevationData`
2. Material/texture cell grid (from IdMap)
3. All placed `TerrainBuilderObject` entries

`PboCompiler` (in `GameEngine/`) binarizes and packs the mod:
1. Generates `config.cpp`, `cfgWorlds`, material configs
2. Binarizes textures and models
3. Archives everything into a `.pbo` file

Binary format I/O is handled by the `bis-file-formats` submodule.

---

## WRP Edit Tools

`Edit/` contains tools for post-generation WRP patching:
- Mass-replace objects (e.g. swap all instances of model X for model Y)
- Update ground material across a polygon area
- Patch a WRP without triggering a full regeneration

These tools are also used by GRM Studio's live terrain editor.

---

## Directory Structure

| Directory | Contents |
|-----------|----------|
| `ManMade/` | Layer generators for roads, buildings, fences, railways, etc. |
| `Nature/` | Layer generators for forests, lakes, rocks, scrub, etc. |
| `Assets/` | Asset type definitions (`BuildingDefinition`, `ClusterCollectionDefinition`, etc.) |
| `GameEngine/` | `WrpCompiler`, `PboCompiler`, config file generators |
| `Imagery/` | SatMap/IdMap tile generation and colour correction |
| `TerrainBuilder/` | `TerrainBuilderObject`, CSV serialisation |
| `Edit/` | WRP patch / object-replace tools |
| `Aerial/` | Aerial imagery extension hooks |
| `Demo/` | Demo map generator (synthetic data, no downloads required) |

---

## See Also

- [`docs/architecture/arma3-integration.md`](../docs/architecture/arma3-integration.md) — detailed reference
- [`GameRealisticMap/README.md`](../GameRealisticMap/README.md) — core engine and builder pattern
- [`GameRealisticMap.Arma3.CommandLine/README.md`](../GameRealisticMap.Arma3.CommandLine/README.md) — CLI usage
