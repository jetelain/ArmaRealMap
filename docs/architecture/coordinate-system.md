# Coordinate System

## Overview

GRM uses two coordinate systems:

| System | Type | Used for |
|--------|------|---------|
| **WGS-84** lat/lng | `GeoAPI.Geometries.Coordinate` | OSM data input, area definition |
| **Terrain space** (local UTM) | `TerrainPoint` (metres) | All internal geometry, output files |

The conversion between the two systems is owned by `ITerrainArea` and performed via a **UTM projection**.

---

## TerrainPoint

`TerrainPoint` (`GameRealisticMap/Geometries/TerrainPoint.cs`) is the fundamental 2D coordinate type used throughout the codebase.

```
(0, 0) ─────────────────→ X (East, metres)
  │
  │   terrain area
  │
  ↓ Y (North, metres)
```

- **Origin**: south-west corner of the terrain area.
- **X axis**: east (positive = east).
- **Y axis**: north (positive = north).
- **Precision**: ~1 cm accuracy up to 83 km from origin (backed by `float`, scaled ×100 for integer-based Clipper polygon operations).
- **Immutable** value type backed by `System.Numerics.Vector2`.

### Clipper scaling

When passed to Clipper (polygon boolean operations), `TerrainPoint` values are multiplied by `GeometryHelper.ScaleForClipper` (1000) and converted to `IntPoint`. This gives 1 mm precision in integer arithmetic.

---

## ITerrainArea

`ITerrainArea` (`GameRealisticMap/ITerrainArea.cs`) abstracts the geographic area and its coordinate transformations.

```csharp
TerrainPoint LatLngToTerrainPoint(Coordinate latLng);
Coordinate   TerrainPointToLatLng(TerrainPoint point);
TerrainPolygon TerrainBounds { get; }  // always (0,0) to (SizeInMeters, SizeInMeters)
float GridCellSize { get; }            // metres per heightmap cell
int   GridSize     { get; }            // number of cells per axis
float SizeInMeters { get; }            // = GridCellSize × GridSize
```

The terrain is always a **square** grid:

```
SizeInMeters = GridCellSize × GridSize

Example: GridCellSize=5, GridSize=2048 → SizeInMeters=10240 m (≈10 km)
```

---

## TerrainAreaUTM

`TerrainAreaUTM` (`GameRealisticMap/TerrainAreaUTM.cs`) is the standard `ITerrainArea` implementation. It pins the south-west corner to a UTM easting/northing and converts lat/lng via [CoordinateSharp](https://github.com/Tronald/CoordinateSharp).

### Creating a TerrainAreaUTM

```csharp
// From a known south-west corner (lat/lng string "N48°40'00" E6°10'00""):
TerrainAreaUTM.CreateFromSouthWest("48.666667 N, 6.166667 E", gridCellSize: 5f, gridSize: 2048);

// From center point:
TerrainAreaUTM.CreateFromCenter("48.7 N, 6.2 E", gridCellSize: 5f, gridSize: 2048);
```

Internally this stores a `UniversalTransverseMercator` (CoordinateSharp) representing the south-west corner. Conversion applies:
```
TerrainPoint.X = UTM_Easting  - SW_Easting
TerrainPoint.Y = UTM_Northing - SW_Northing
```

For points that cross UTM zone boundaries, a simplified adjustment is applied to keep coordinates monotonic within the terrain area.

---

## Elevation Grid

The elevation/heightmap grid is aligned with terrain space:

```
cell(col, row) covers:
  X: [col × GridCellSize, (col+1) × GridCellSize]
  Y: [row × GridCellSize, (row+1) × GridCellSize]
```

`ElevationGrid` stores `float[GridSize, GridSize]` elevation values (metres above sea level). The grid origin `[0, 0]` corresponds to terrain point `(0, 0)` (south-west corner).

---

## Geometry Types Summary

| Type | Description |
|------|-------------|
| `TerrainPoint` | 2D point in terrain space (metres) |
| `TerrainPath` | Ordered list of `TerrainPoint` forming a polyline |
| `TerrainPathSegment` | Single line segment between two `TerrainPoint` values |
| `TerrainPolygon` | Closed polygon; may have holes. Backed by Clipper for boolean ops |
| `BoundingBox` | Axis-aligned bounding rectangle in terrain space |
| `BoundingCircle` | Bounding circle for fast radius tests |
| `SimpleSpacialIndex<T>` | Grid-based spatial index for fast proximity queries |
| `TerrainSpacialIndex` | Specialised spatial index for `TerrainPolygon` lookup |
| `Envelope` | Tight bounding envelope used during geometry merging |

---

## Coordinate Flow Example

```
OSM node: lat=48.7, lng=6.2
     ↓  ITerrainArea.LatLngToTerrainPoint()
TerrainPoint(X=1452.3, Y=3891.7)   ← local terrain metres
     ↓  various builders work entirely in TerrainPoint space
TerrainPolygon(forest boundary)
     ↓  Arma3LayerGeneratorCatalog
TerrainBuilderObject(X=1452.3, Y=3891.7, model=…)   ← CSV for TerrainBuilder tool
```
