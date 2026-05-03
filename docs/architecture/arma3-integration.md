# Arma 3 Integration

## Overview

`GameRealisticMap.Arma3` converts generic terrain data (`*Data` objects from `GameRealisticMap`) into Arma 3 map packages ready to be loaded as a mod. The output is:
- A `.wrp` terrain file (heightmap + object placement)
- A `.pbo` mod package (binarized configs, imagery, object CSVs)
- Satellite/material texture tiles (`.paa`)
- TerrainBuilder-compatible CSV files for manual editing

The entry point for a full generation is `Arma3MapGenerator`.

---

## Configuration: Arma3MapConfig

`Arma3MapConfig` implements both `IMapProcessingOptions` and `IPboConfig`. Key properties:

| Property | Description |
|----------|-------------|
| `TerrainArea` | `ITerrainArea` (UTM-based geographic bounds) |
| `WorldName` | Arma 3 world identifier (e.g. `mymap`) |
| `PboPrefix` | PBO prefix (e.g. `z\arm\addons\mymap`) |
| `TileSize` | Imagery tile size in pixels (auto-detected, typically 1024) |
| `Resolution` | Metres per pixel for imagery (default 1.0) |
| `FakeSatBlend` | 0 = pure material colors, 1 = real satellite, 0–1 = blend |
| `AssetConfigFile` | Path to the region asset configuration JSON file |
| `TargetModDirectory` | Output directory for the compiled mod |

---

## Layer Generation

`Arma3LayerGeneratorCatalog` holds a list of `ITerrainBuilderLayerGenerator` implementations. Each generator:
1. Receives the `IArma3RegionAssets` (asset library for the region).
2. Calls `context.GetData<TData>()` to retrieve the relevant feature data.
3. Returns a list of `TerrainBuilderObject` instances (object placement records).

See [builders-reference.md](builders-reference.md#arma-3-layer-generators) for the full list of generators.

### ITerrainBuilderLayerGenerator

```csharp
public interface ITerrainBuilderLayerGenerator
{
    IEnumerable<TerrainBuilderObject> Generate(IArma3RegionAssets assets, IContext context);
}
```

---

## TerrainBuilderObject

`TerrainBuilderObject` (in `GameRealisticMap.Arma3/TerrainBuilder/`) represents a single placed object in Arma's TerrainBuilder CSV format:

```
"P3D_path"; X; Y; Yaw; Pitch; Roll; Scale; Mode
```

| Field | Description |
|-------|-------------|
| Model path | P3D model path relative to P:\ drive |
| X, Y | Position in terrain space (metres) |
| Yaw/Pitch/Roll | Rotation (degrees) |
| Scale | Object scale factor (usually 1.0) |
| Mode | Elevation mode: `Relative` (above terrain) or `Absolute` |

---

## Asset System

`IArma3RegionAssets` provides all region-specific 3D models and materials. A region is a geographic or biome configuration (e.g. Central Europe, Mediterranean).

### Asset Types

| Type | Description |
|------|-------------|
| `BuildingDefinition` | One or more P3D models for a building category + size class |
| `ClusterCollectionDefinition` | Sets of models for density clustering (e.g. dense forest objects) |
| `BasicCollectionDefinition` | Simple model sets for fill placement |
| `TerrainMaterialLibrary` | Maps material names to texture file paths and material IDs |
| `FenceDefinition` | Model + spacing for fence/wall generation |
| `RowDefinition` | Model + spacing for vineyard/orchard row generation |
| `RailwaysDefinition` | Rail track model + crossing signal models |

### Composition

`Composition` combines multiple `TerrainBuilderObject` records into a logical group (e.g. a building with door + windows as sub-objects). Used by `BuildingGenerator` to place multi-component assemblies.

---

## Imagery Pipeline

Three imagery modes are available, configured via `FakeSatBlend`:

### 1. IdMap (Material ID Map)
An image where each pixel encodes the terrain material at that location (colour = material ID). Used by Arma's terrain engine to select surface type per cell.

### 2. SatMap (Satellite Texture)
- **Real satellite** (`FakeSatBlend = 1.0`): Uses `RawSatelliteImageData` (Sentinel-2 tiles).
- **Fake satellite** (`FakeSatBlend = 0.0`): `FakeSatRender` generates a procedural satellite-like texture from the material colours.
- **Blended** (`0 < FakeSatBlend < 1`): Linear blend of both.

Roads are drawn on top of the satellite texture at full opacity regardless of blend mode.

### 3. Color correction
A final colour-corrected pass adjusts brightness/contrast for the Arma 3 engine's expectations.

---

## WRP Generation

`WrpCompiler` (in `GameRealisticMap.Arma3/GameEngine/`) generates the binary `.wrp` terrain file:

1. Writes the heightmap grid from `ElevationData`.
2. Writes the material/texture cell grid (from IdMap).
3. Writes all `TerrainBuilderObject` entries (roads, trees, buildings, etc.).

The WRP format is handled by `bis-file-formats/BIS.WRP`.

---

## PBO Packaging

`PboCompiler` (in `GameRealisticMap.Arma3/GameEngine/`) binarizes and packs the mod:

1. Generates Arma 3 config files (`config.cpp`, `cfgWorlds`, material configs).
2. Binarizes models and textures.
3. Packs everything into a `.pbo` archive.

PBO format is handled by `bis-file-formats/BIS.PBO`.

---

## Edit Operations

`GameRealisticMap.Arma3/Edit/` contains tools for post-generation WRP editing:
- Mass-replace objects (update all trees of type X to type Y).
- Material updates (change ground material across a polygon).
- WRP patching without full regeneration.

---

## Demo Map Generator

`Arma3DemoMapGenerator` generates a small test map using synthetic data (no OSM/SRTM download required). Useful for testing asset configurations and generation pipeline changes without a real geographic area.
