﻿using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;

namespace GameRealisticMap.Generic.Exporters.ManMade
{
    internal class BuildingsRectangleExporter : PolygonExporterBase
    {
        public override string Name => "BuildingsRectangle";

        protected override IEnumerable<(TerrainPolygon, IDictionary<string, object>?)> GetPolygons(IBuildContext context, IDictionary<string, object>? properties)
        {
            return context.GetData<BuildingsData>().Buildings.Select(l => (l.Box.Polygon, GetProperties(l, properties)));
        }

        internal static IDictionary<string, object>? GetProperties(Building building, IDictionary<string, object>? properties)
        {
            var dict = properties != null ? new Dictionary<string, object>(properties) : new Dictionary<string, object>();
            dict["grm_building"] = building.TypeId.ToString();
            return dict;
        }
    }
}
