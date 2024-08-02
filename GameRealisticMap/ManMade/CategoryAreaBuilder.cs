using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;
using Pmad.ProgressTracking;

namespace GameRealisticMap.ManMade
{
    internal class CategoryAreaBuilder : IDataBuilder<CategoryAreaData>
    {
        public CategoryAreaData Build(IBuildContext context, IProgressScope scope)
        {
            var osmAreas = context.OsmSource.All.Where(o => o.Tags != null && o.Tags.ContainsKey("landuse")).ToList();
            var areas = new List<CategoryArea>();
            using var report = scope.CreateInteger("CategoryArea", osmAreas.Count);
            foreach (var area in osmAreas)
            {
                var buildingType = GetBuildingType(area.Tags.GetValue("landuse"));
                if (buildingType != null)
                {
                    var polygons = context.OsmSource.Interpret(area)
                        .SelectMany(geometry => TerrainPolygon.FromGeometry(geometry, context.Area.LatLngToTerrainPoint))
                        .SelectMany(g => g.ClippedBy(context.Area.TerrainBounds))
                        .ToList();
                    areas.Add(new CategoryArea(buildingType.Value, polygons));
                }
                report.ReportOneDone();
            }
            return new CategoryAreaData(areas);
        }

        private BuildingTypeId? GetBuildingType(string landuse)
        {
            switch (landuse)
            {
                case "industrial": return BuildingTypeId.Industrial;
                case "commercial": return BuildingTypeId.Commercial;
                case "residential": return BuildingTypeId.Residential;
                case "retail": return BuildingTypeId.Retail;
                case "military": return BuildingTypeId.Military;
                case "farmyard": return BuildingTypeId.Agricultural;
            }
            return null;
        }
    }
}
