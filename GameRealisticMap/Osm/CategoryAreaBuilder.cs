using GameRealisticMap.Buildings;
using GameRealisticMap.Geo;
using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;
using OsmSharp.Complete;

namespace GameRealisticMap.Osm
{
    internal class CategoryAreaBuilder : IDataBuilder<CategoryAreaData>
    {
        private readonly IProgressSystem progress;

        public CategoryAreaBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public CategoryAreaData Build(IBuildContext context)
        {
            var interpret = new DefaultFeatureInterpreter2();
            var osmAreas = context.OsmSource.Stream.Where(o => o.Tags != null && o.Tags.ContainsKey("landuse")).ToList();
            var areas = new List<CategoryArea>();
            using var report = progress.CreateStep("CategoryArea", osmAreas.Count);
            foreach (var area in osmAreas)
            {
                var buildingType = GetBuildingType(area.Tags.GetValue("landuse"));
                if (buildingType != null)
                {
                    var complete = area.CreateComplete(context.OsmSource.Snapshot);
                    var polygons = interpret.Interpret(complete).Features.SelectMany(feature => TerrainPolygon.FromGeometry(feature.Geometry, context.Area.LatLngToTerrainPoint)).ToList();
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
            }
            return null;
        }
    }
}
