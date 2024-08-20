using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Roads;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Nature.Forests
{
    internal class ForestEdgeBuilder : IDataBuilder<ForestEdgeData>
    {
        public ForestEdgeData Build(IBuildContext context, IProgressScope scope)
        {
            var roads = context.GetData<RoadsData>().Roads;
            var forests = context.GetData<ForestData>().Polygons;

            // Trails are ignored by BasicBuilderBase, but prevents forest edge effect
            // Buildings are not surrounded with bushed
            var priority = roads
                .Where(r => r.RoadType == RoadTypeId.Trail)
                .SelectMany(r => r.Path.ToTerrainPolygon(r.Width + 1))
                .Concat(context.GetData<BuildingsData>().Buildings.SelectMany(b => b.Box.Polygon.Offset(2.5f)))
                .ToList();

            // Ignore reallys smalls "forests", as it might have been used to map some isolated trees
            forests = forests.Where(f => f.Area > 200).ToList();

            var edges = 
                forests.WithProgress(scope, "Edges")
                    .SelectMany(f => f.InnerCrown(ForestEdgeData.Width)) // 2m width offset

                    .SubstractAll(scope, "Priority", priority)

                    .ToList();

            return new ForestEdgeData(edges, forests);
        }
    }
}
