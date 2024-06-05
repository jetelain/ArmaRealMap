using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;

namespace GameRealisticMap.Generic.Exporters
{
    internal abstract class PolygonExporterBase : ShapeExporterBase
    {

        protected abstract IEnumerable<(TerrainPolygon, IDictionary<string, object>?)> GetPolygons(IBuildContext context, IDictionary<string, object>? properties);

        public override FeatureCollection GetGeoJsonFeatureCollection(IBuildContext context, IDictionary<string, object>? properties)
        {
            return new FeatureCollection(GetPolygons(context, properties).Select(pair => new Feature(pair.Item1.ToGeoJson(p => p), pair.Item2)).ToList());
        }

        public override List<NetTopologySuite.Features.Feature> GetShapeFeatures(IBuildContext context, IDictionary<string, object>? properties)
        {
            return GetPolygons(context, properties).Select(pair => new NetTopologySuite.Features.Feature(pair.Item1.AsPolygon, ToAttributesTable(pair.Item2))).ToList();
        }
    }
}
