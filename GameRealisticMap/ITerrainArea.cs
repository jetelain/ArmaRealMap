using GameRealisticMap.Geometries;
using GeoAPI.Geometries;

namespace GameRealisticMap
{
    /// <summary>
    /// Defines the geographic bounds of the terrain being generated and provides coordinate conversion
    /// between WGS-84 latitude/longitude and local terrain-space coordinates (<see cref="TerrainPoint"/>).
    /// The default implementation is <see cref="TerrainAreaUTM"/>, which uses a UTM projection.
    /// </summary>
    public interface ITerrainArea
    {
        /// <summary>
        /// Converts a WGS-84 coordinate (latitude/longitude) to a local terrain point
        /// (X/Y in metres from the south-west corner of the terrain area).
        /// </summary>
        /// <param name="latLng">WGS-84 coordinate. <c>latLng.X</c> = longitude, <c>latLng.Y</c> = latitude.</param>
        /// <returns>The corresponding local terrain point in metres.</returns>
        TerrainPoint LatLngToTerrainPoint(Coordinate latLng);

        /// <summary>
        /// Converts a local terrain point back to a WGS-84 latitude/longitude coordinate.
        /// </summary>
        /// <param name="point">Local terrain point in metres from the south-west origin.</param>
        /// <returns>The corresponding WGS-84 coordinate.</returns>
        Coordinate TerrainPointToLatLng(TerrainPoint point);

        /// <summary>
        /// The bounding polygon of the terrain area in local terrain coordinates.
        /// Always a rectangle from <c>(0, 0)</c> to <c>(SizeInMeters, SizeInMeters)</c>.
        /// </summary>
        TerrainPolygon TerrainBounds { get; }

        /// <summary>
        /// The size of each heightmap grid cell in metres (i.e. terrain heightmap resolution).
        /// Typical values: 5, 10, or 20 metres per cell.
        /// </summary>
        float GridCellSize { get; }

        /// <summary>
        /// The number of grid cells along each axis. The terrain is always square.
        /// Total size in metres = <see cref="GridCellSize"/> × <see cref="GridSize"/>.
        /// </summary>
        int GridSize { get; }

        /// <summary>
        /// Total side length of the terrain in metres.
        /// Equals <see cref="GridCellSize"/> × <see cref="GridSize"/>.
        /// </summary>
        float SizeInMeters { get; }
    }
}
