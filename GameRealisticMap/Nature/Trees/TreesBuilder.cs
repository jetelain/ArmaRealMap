using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Roads;
using OsmSharp.Geo;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Nature.Trees
{
    internal class TreesBuilder : IDataBuilder<TreesData>
    {
        public TreesData Build(IBuildContext context, IProgressScope scope)
        {
            var keepWay = new TerrainSpacialIndex<ITerrainGeo>(context.Area);
            keepWay.AddRange(context.GetData<RoadsData>().Roads.SelectMany(b => b.Polygons));
            keepWay.AddRange(context.GetData<BuildingsData>().Buildings.Select(p => p.Box.Polygon));

            var points = context.OsmSource.Nodes
                .Where(n => n.Tags != null && n.Tags.GetValue("natural") == "tree")
                .Select(t => context.Area.LatLngToTerrainPoint(t.GetCoordinate()))
                .WithProgress(scope, "Trees")
                .Where(p => context.Area.IsInside(p))
                .Select(p => GeometryHelper.KeepAway(p, keepWay, 2))
                .ToList();

            return new TreesData(points);
        }
    }
}
