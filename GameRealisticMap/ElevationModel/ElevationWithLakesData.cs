using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.ElevationModel
{
    /// <summary>
    /// Elevation data with lakes below their computed water surface level.
    /// Intermediate result between <see cref="RawElevationData"/> and the fully-constrained <see cref="ElevationData"/>.
    /// </summary>
    public class ElevationWithLakesData : IGeoJsonData
    {

        public ElevationWithLakesData(ElevationGrid elevation, List<LakeWithElevation> lakes)
        {
            Elevation = elevation;
            Lakes = lakes;
        }

        public ElevationGrid Elevation { get; }

        public List<LakeWithElevation> Lakes { get; }

        public IEnumerable<Feature> ToGeoJson(Func<TerrainPoint, IPosition> project)
        {
            var properties = new Dictionary<string, object>();
            properties.Add("type", "realLake");
            return Lakes.Select(l => new Feature(l.TerrainPolygon.ToGeoJson(project), properties));
        }
    }
}
