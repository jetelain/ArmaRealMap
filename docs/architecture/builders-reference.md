# Builders Reference

> For the builder pattern internals see [data-pipeline.md](data-pipeline.md). For a project overview see [`GameRealisticMap/README.md`](../../GameRealisticMap/README.md).

Complete catalog of all data builders registered in `BuildersCatalog`. Each row shows the builder class, the data type it produces, its primary data source, and a short description.

See [data-pipeline.md](data-pipeline.md) for how builders are resolved and cached.

---

## Base Terrain

| Builder | Output Type | Source | Description |
|---------|-------------|--------|-------------|
| `OceanBuilder` | `OceanData` | OSM | Ocean/sea polygons derived from coastline ways |
| `CoastlineBuilder` | `CoastlineData` | OSM | Raw coastline lines; used to build `OceanData` and mask other features |

---

## Remote Data Downloads

| Builder | Output Type | Source | Description |
|---------|-------------|--------|-------------|
| `RawElevationBuilder` | `RawElevationData` | NASA SRTM | Downloads SRTM 30 m tiles, assembles into an `ElevationGrid` covering the terrain area |
| `RawSatelliteImageBuilder` | `RawSatelliteImageData` | Satellite image | Downloads and assembles satellite imagery as a `HugeImage<Rgba32>` |
| `WeatherBuilder` | `WeatherData` | Local config / cache | Wind direction and atmospheric statistics for the terrain's geographic location |

---

## OSM Classification

| Builder | Output Type | Source | Description |
|---------|-------------|--------|-------------|
| `CategoryAreaBuilder` | `CategoryAreaData` | OSM | Classifies land-use polygons into categories: residential, commercial, industrial, military, retail, agricultural |

---

## Man-Made Features

| Builder | Output Type | Source | Description |
|---------|-------------|--------|-------------|
| `RoadsBuilder` | `RoadsData` | OSM | Extracts and classifies road ways by type (motorway→track). Resolves bridges, embankments and tunnels. Merges connected segments. |
| `BuildingsBuilder` | `BuildingsData` | OSM | Multi-pass algorithm: extract bounds → merge small buildings → add node buildings → remove collisions → crop to roads → classify by type |
| `RailwaysBuilder` | `RailwaysData` | OSM | Railway lines and crossings |
| `FencesBuilder` | `FencesData` | OSM | Fences, walls, hedgerows as linear features |
| `FarmlandsBuilder` | `FarmlandsData` | OSM | Farmland and agricultural polygon areas |
| `OrientedObjectBuilder` | `OrientedObjectData` | OSM | Point objects with a fixed orientation (power poles, water towers, wind turbines, etc.) |
| `ProceduralStreetLampsBuilder` | `ProceduralStreetLampsData` | `RoadsData` | Generates street lamp positions along roads procedurally |
| `SidewalksBuilder` | `SidewalksData` | `RoadsData` | Generates sidewalk paths along roads with footpaths |
| `CitiesBuilder` | `CitiesData` | OSM | Named settlements (place=city/town/village/hamlet) as point features |
| `VineyardBuilder` | `VineyardData` | OSM | Vineyard polygon areas (landuse=vineyard) |
| `OrchardBuilder` | `OrchardData` | OSM | Orchard polygon areas (landuse=orchard) |
| `AirportBuilder` | `AirportData` | OSM | Airport/aerodrome boundary polygons |
| `AerowaysBuilder` | `AerowaysData` | OSM | Runways, taxiways, helipads as linear/polygon features |
| `AsphaltBuilder` | `AsphaltData` | OSM | Asphalt/paved surface areas (parking lots, plazas) |
| `CutlinesBuilder` | `CutlinesData` | OSM | Forest cutlines (power line corridors, firebreaks) |

---

## Default Urban Area Fill

These builders produce fill polygons for land-use areas that have no specific OSM tagging. They reference `CategoryAreaData` and `RoadsData` to clip and fill uncovered areas.

| Builder | Output Type | Category |
|---------|-------------|----------|
| `DefaultResidentialAreasBuilder` | `DefaultResidentialAreasData` | residential |
| `DefaultCommercialAreasBuilder` | `DefaultCommercialAreasData` | commercial |
| `DefaultIndustrialAreasBuilder` | `DefaultIndustrialAreasData` | industrial |
| `DefaultMilitaryAreasBuilder` | `DefaultMilitaryAreasData` | military |
| `DefaultRetailAreasBuilder` | `DefaultRetailAreasData` | retail |
| `DefaultAgriculturalAreasBuilder` | `DefaultAgriculturalAreasData` | agricultural |

---

## Nature Features

| Builder | Output Type | Source | Description |
|---------|-------------|--------|-------------|
| `ForestBuilder` | `ForestData` | OSM | Forest/woodland polygon areas (landuse=forest, natural=wood). Higher priority than scrub. |
| `ForestRadialBuilder` | `ForestRadialData` | `ForestData` | Radial inset areas of forests used for density-based interior tree placement |
| `ForestEdgeBuilder` | `ForestEdgeData` | `ForestData` | Forest edge/fringe polygons for edge-specific vegetation |
| `ScrubBuilder` | `ScrubData` | OSM | Scrubland/bushland polygons (natural=scrub) |
| `ScrubRadialBuilder` | `ScrubRadialData` | `ScrubData` | Radial inset of scrub areas for interior density placement |
| `RocksBuilder` | `RocksData` | OSM | Rocky outcrop areas (natural=rock, natural=bare_rock) |
| `ScreeBuilder` | `ScreeData` | OSM | Scree/talus slope areas (natural=scree) |
| `LakesBuilder` | `LakesData` | OSM | Lake and pond water bodies |
| `WatercoursesBuilder` | `WatercoursesData` | OSM | Rivers, streams, canals as linear features |
| `WatercourseRadialBuilder` | `WatercourseRadialData` | `WatercoursesData` | Buffered watercourse polygons for riparian vegetation |
| `TreesBuilder` | `TreesData` | OSM | Individual tree nodes (natural=tree) |
| `TreeRowsBuilder` | `TreeRowsData` | OSM | Rows of trees (natural=tree_row as OSM ways) |
| `DefaultAreasBuilder` | `DefaultAreasData` | All feature data | Fallback surface for terrain not covered by any other feature |

---

## Nature Surfaces (Ground Cover)

| Builder | Output Type | Source | Description |
|---------|-------------|--------|-------------|
| `SandSurfacesBuilder` | `SandSurfacesData` | OSM | Sandy areas (natural=sand, natural=beach) |
| `MeadowsBuilder` | `MeadowsData` | OSM | Meadow areas (landuse=meadow, natural=grassland) |
| `GrassBuilder` | `GrassData` | OSM | Short grass/lawn areas (landuse=grass, leisure=park) |
| `IceSurfaceBuilder` | `IceSurfaceData` | OSM | Glacier/ice areas (natural=glacier) |

---

## Elevation

| Builder | Output Type | Source | Description |
|---------|-------------|--------|-------------|
| `ElevationWithLakesBuilder` | `ElevationWithLakesData` | `RawElevationData` + `LakesData` | Flattens lake areas to a consistent water level |
| `ElevationBuilder` | `ElevationData` | `ElevationWithLakesData` + `RoadsData` + `RailwaysData` + `WatercoursesData` | Applies road/railway embankment and bridge constraints, ensures watercourses flow downhill |
| `ElevationContourBuilder` | `ElevationContourData` | `ElevationData` | Generates contour line polygons for map overview rendering |
| `ElevationOutOfBoundsBuilder` | `ElevationOutOfBoundsData` | `RawElevationData` | Computes min/max elevation for terrain outside the main grid (used for terrain edge blending) |

---

## Auxiliary

| Builder | Output Type | Description |
|---------|-------------|-------------|
| `ConditionEvaluatorBuilder` | `ConditionEvaluatorData` | Compiles and caches all tag-filter conditions defined in the asset configuration |

---

## Arma 3 Layer Generators

`Arma3LayerGeneratorCatalog` (`GameRealisticMap.Arma3/Arma3LayerGeneratorCatalog.cs`) registers the generators that convert `*Data` objects into `TerrainBuilderObject` lists (object placement CSV for Arma's TerrainBuilder tool).

### Man-Made Generators

| Generator | Consumes | Description |
|-----------|----------|-------------|
| `BuildingGenerator` | `BuildingsData` | Places 3D building models matching footprint dimensions from asset library |
| `OrientedObjectsGenerator` | `OrientedObjectData` | Places single oriented objects (poles, towers, etc.) |
| `BridgeGenerator` | `RoadsData` | Places bridge deck models over bridge road segments |
| `FenceGenerator` | `FencesData` | Places fence/wall models along fence lines |
| `RailwayGenerator` | `RailwaysData` | Places rail track models and crossing signals |
| `VineyardsGenerator` | `VineyardData` | Vineyard row objects |
| `OrchardGenerator` | `OrchardData` | Orchard tree objects |
| `SidewalksGenerator` | `SidewalksData` | Sidewalk surface materials |
| `DefaultAgriculturalAreasGenerator` | `DefaultAgriculturalAreasData` | Fill material for agricultural areas |
| `DefaultCommercialAreasGenerator` | `DefaultCommercialAreasData` | Fill material for commercial areas |
| `DefaultIndustrialAreasGenerator` | `DefaultIndustrialAreasData` | Fill material for industrial areas |
| `DefaultMilitaryAreasGenerator` | `DefaultMilitaryAreasData` | Fill material for military areas |
| `DefaultResidentialAreasGenerator` | `DefaultResidentialAreasData` | Fill material for residential areas |
| `DefaultRetailAreasGenerator` | `DefaultRetailAreasData` | Fill material for retail areas |

### Nature Generators

| Generator | Consumes | Description |
|-----------|----------|-------------|
| `ForestGenerator` | `ForestData` | Density-based tree cluster placement inside forests |
| `ForestRadialGenerator` | `ForestRadialData` | Tree placement for forest interior zones |
| `ForestEdgeGenerator` | `ForestEdgeData` | Edge-specific vegetation at forest boundaries |
| `LakeSurfaceGenerator` | `LakesData` | Water surface material for lakes |
| `RocksGenerator` | `RocksData` | Rock model placement in rocky areas |
| `ScreeGenerator` | `ScreeData` | Scree rock model placement |
| `ScrubGenerator` | `ScrubData` | Shrub/bush model placement in scrub areas |
| `ScrubRadialGenerator` | `ScrubRadialData` | Interior shrub placement |
| `TreesGenerator` | `TreesData` | Individual tree model placement |
| `TreeRowsGenerator` | `TreeRowsData` | Tree-row model placement |
| `WatercourseGenerator` | `WatercoursesData` | Watercourse bank materials |
| `WatercourseRadialGenerator` | `WatercourseRadialData` | Riparian vegetation placement |
| `DefaultAreasGenerator` | `DefaultAreasData` | Default ground cover for unclassified areas |
