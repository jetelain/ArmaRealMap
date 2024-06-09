using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;

namespace GameRealisticMap.Generic.Exporters
{
    internal abstract class PointExporterBase : ShapeExporterBase
    {
        protected abstract IEnumerable<(TerrainPoint, IDictionary<string, object>?)> GetPoints(IBuildContext context, IDictionary<string, object>? properties);

        public override FeatureCollection GetGeoJsonFeatureCollection(IBuildContext context, IDictionary<string, object>? properties)
        {
            return new FeatureCollection(GetPoints(context, properties).Select(pair => new Feature(new GeoJSON.Text.Geometry.Point(pair.Item1), pair.Item2)).ToList());
        }

        public override List<NetTopologySuite.Features.Feature> GetShapeFeatures(IBuildContext context, IDictionary<string, object>? properties)
        {
            return GetPoints(context, properties).Select(pair => new NetTopologySuite.Features.Feature(new NetTopologySuite.Geometries.Point(pair.Item1.X, pair.Item1.Y), ToAttributesTable(pair.Item2))).ToList();
        }
    }
}
