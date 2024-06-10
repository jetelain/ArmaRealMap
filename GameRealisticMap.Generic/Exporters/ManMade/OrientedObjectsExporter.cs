using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Objects;

namespace GameRealisticMap.Generic.Exporters.ManMade
{
    internal class OrientedObjectsExporter : PointExporterBase
    {
        public override string Name => "OrientedObjects";

        protected override IEnumerable<(TerrainPoint, IDictionary<string, object>?)> GetPoints(IBuildContext context, IDictionary<string, object>? properties)
        {
            return context.GetData<OrientedObjectData>().Objects.Select(l => (l.Point, GetProperties(properties, l)));
        }

        private IDictionary<string, object>? GetProperties(IDictionary<string, object>? properties, OrientedObject obj)
        {
            var dict = properties != null ? new Dictionary<string, object>(properties) : new Dictionary<string, object>();
            dict["grm_heading"] = obj.Heading;
            dict["grm_object"] = obj.TypeId.ToString();
            return dict;
        }
    }
}
