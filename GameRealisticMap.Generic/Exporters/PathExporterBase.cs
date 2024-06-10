using System.Text;
using System.Text.Json;
using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;
namespace GameRealisticMap.Generic.Exporters
{
    internal abstract class PathExporterBase : ShapeExporterBase
    {
        protected abstract IEnumerable<(TerrainPath, IDictionary<string, object>?)> GetPaths(IBuildContext context, IDictionary<string, object>? properties);

        public override FeatureCollection GetGeoJsonFeatureCollection(IBuildContext context, IDictionary<string, object>? properties)
        {
            return new FeatureCollection(GetPaths(context, properties).Select(pair => new Feature(pair.Item1.ToGeoJson(p => p), pair.Item2)).ToList());
        }

        public override List<NetTopologySuite.Features.Feature> GetShapeFeatures(IBuildContext context, IDictionary<string, object>? properties)
        {
            return GetPaths(context, properties).Select(pair => new NetTopologySuite.Features.Feature(pair.Item1.AsLineString, ToAttributesTable(pair.Item2))).ToList();
        }

    }
}
