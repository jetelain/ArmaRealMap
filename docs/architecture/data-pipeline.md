# Data Pipeline & Builder Pattern

> For a project-level overview see [`GameRealisticMap/README.md`](../../GameRealisticMap/README.md). This document is the in-depth reference for the builder pattern and `BuildContext` internals.

## Core Concept

The entire map generation pipeline is built on a single pattern: **typed data builders**.

- Every type of data (`ForestData`, `RoadsData`, `ElevationData`, …) is produced by exactly one `IDataBuilder<T>` implementation.
- Builders are registered in a `IBuidersCatalog`.
- A `BuildContext` wraps the catalog and provides lazy, cached, concurrent access to all data.
- Builders request their dependencies by calling `context.GetData<TOtherData>()` — the framework resolves and builds dependencies automatically.

---

## Key Interfaces

### `IContext`
Base context. Provides access to built data:

```csharp
T GetData<T>(IProgressScope? parentScope = null) where T : class;
Task<T> GetDataAsync<T>(IProgressScope? parentScope = null) where T : class;
IEnumerable<T> GetOfType<T>() where T : class;
IHugeImageStorage HugeImageStorage { get; }
```

### `IBuildContext : IContext`
Full build context passed to every builder:

```csharp
ITerrainArea Area { get; }         // geographic bounds + coordinate conversion
IOsmDataSource OsmSource { get; }  // parsed OSM data for the area
IMapProcessingOptions Options { get; }  // resolution, thresholds, satellite settings
```

### `IDataBuilder<T>`
One implementation per data type `T`:

```csharp
T Build(IBuildContext context, IProgressScope scope);
```

### `IDataBuilderAsync<T>`
For builders doing I/O (elevation, satellite download). Provides `BuildAsync`; the sync `Build` is auto-implemented as `.Result`.

### `IBuidersCatalog`
Registry that maps `Type → IDataBuilder<T>`:

```csharp
void Register<TData>(IDataBuilder<TData> builder);
IDataBuilder<TData> Get<TData>();
IEnumerable<TResult> VisitAll<TResult>(IDataBuilderVisitor<TResult> visitor);
IEnumerable<T> GetOfType<T>(IContext ctx, Func<Type, bool>? filter = null);
```

---

## BuildContext Implementation

`BuildContext` (`GameRealisticMap/BuildContext.cs`) implements `IBuildContext` with:

- A `Dictionary<Type, Task>` cache (`datas`) — one entry per data type ever requested.
- A `SemaphoreSlim(1,1)` protecting concurrent registrations into the cache dictionary (not the builds themselves).
- Each data type is built **exactly once** on a `Task.Run` thread pool task; subsequent calls return the cached `Task<T>`.

```csharp
// Simplified flow of GetDataAsync<T>:
await semaphoreSlim.WaitAsync();
if (datas.ContainsKey(typeof(T))) return (Task<T>)datas[typeof(T)];
var task = Task.Run(() => catalog.Get<T>().Build(this, scope));
datas[typeof(T)] = task;
semaphoreSlim.Release();
return task;
```

This means builders from different domains can run **concurrently** as long as they don't share dependencies. When builder A calls `GetData<T>()` for a dependency, it suspends until that dependency's task completes — naturally forming a DAG.

The `SetData<T>(T value)` method allows injecting pre-built data (useful for tests and Studio live editing).

---

## BuildersCatalog

`BuildersCatalog` (`GameRealisticMap/BuildersCatalog.cs`) is the default `IBuidersCatalog` implementation. Its constructor registers **all builders** in sequence. The order does not affect correctness (lazy evaluation handles dependencies), but it reflects typical data dependency order for readability.

Registration groups (see [builders-reference.md](builders-reference.md) for the full table):

| Group | Builders |
|-------|---------|
| Base terrain | `OceanBuilder`, `CoastlineBuilder` |
| Remote data | `RawSatelliteImageBuilder`, `RawElevationBuilder` |
| OSM classification | `CategoryAreaBuilder` |
| Man-made | `RoadsBuilder`, `BuildingsBuilder`, `RailwaysBuilder`, `FencesBuilder`, `FarmlandsBuilder`, `OrientedObjectBuilder`, `ProceduralStreetLampsBuilder`, `SidewalksBuilder`, `CitiesBuilder`, `VineyardBuilder`, `OrchardBuilder`, `AirportBuilder`, `AerowaysBuilder`, `AsphaltBuilder`, `CutlinesBuilder` |
| Default urban areas | `DefaultResidentialAreasBuilder`, `DefaultCommercialAreasBuilder`, `DefaultIndustrialAreasBuilder`, `DefaultMilitaryAreasBuilder`, `DefaultRetailAreasBuilder`, `DefaultAgriculturalAreasBuilder` |
| Nature | `ForestBuilder`, `ForestRadialBuilder`, `ForestEdgeBuilder`, `ScrubBuilder`, `ScrubRadialBuilder`, `RocksBuilder`, `LakesBuilder`, `WatercoursesBuilder`, `WatercourseRadialBuilder`, `TreesBuilder`, `TreeRowsBuilder`, `DefaultAreasBuilder` |
| Nature surfaces | `SandSurfacesBuilder`, `MeadowsBuilder`, `GrassBuilder`, `IceSurfaceBuilder`, `ScreeBuilder` |
| Elevation | `ElevationWithLakesBuilder`, `ElevationBuilder`, `ElevationContourBuilder`, `ElevationOutOfBoundsBuilder` |
| Auxiliary | `ConditionEvaluatorBuilder`, `WeatherBuilder` |

---

## Builder Conventions

When implementing a new builder:

1. Create a `*Data` class (plain data container, no logic, usually with `IReadOnlyList<T>` properties).
2. Create a `*Builder : IDataBuilder<*Data>` (or `IDataBuilderAsync<*Data>`) class.
3. Register it in `BuildersCatalog` (or in the game-specific catalog if Arma3-only).
4. Depend on other data via `context.GetData<OtherData>()`.

```csharp
public class ForestData
{
    public IReadOnlyList<TerrainPolygon> Polygons { get; }
    // ...
}

public class ForestBuilder : IDataBuilder<ForestData>
{
    public ForestData Build(IBuildContext context, IProgressScope scope)
    {
        var osm = context.OsmSource;
        // ... parse OSM, build polygons
        return new ForestData(polygons);
    }
}
```

---

## Progress Reporting

Every builder receives an `IProgressScope` (from [Pmad.ProgressTracking](https://github.com/jetelain/ProgressTracking)). Builders should create child scopes for sub-operations:

```csharp
using (var sub = scope.CreateScope("LoadingPolygons"))
{
    // ...
}
```

The `BuildContext` automatically names each builder's scope after the builder's class name (with "Builder" stripped).

---

## IBuildersConfig

`IBuildersConfig` is provided to `BuildersCatalog`'s constructor and supplies domain-specific configuration:
- `Roads` — road type classification config
- `Buildings` — building size/type config
- `RailwayCrossings` — railway crossing assets config

The Arma 3 implementation is `Arma3MapConfig`. A generic no-op implementation exists in `GameRealisticMap.Generic`.

---

## ISourceLocations

`ISourceLocations` provides paths to data caches (SRTM tiles, satellite tiles, weather data). Used by `RawElevationBuilder`, `RawSatelliteImageBuilder`, `WeatherBuilder`.
