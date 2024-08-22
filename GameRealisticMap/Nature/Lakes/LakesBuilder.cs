using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade;
using GameRealisticMap.ManMade.Railways;
using GameRealisticMap.ManMade.Roads;
using OsmSharp.Tags;

namespace GameRealisticMap.Nature.Lakes
{
    internal class LakesBuilder : BasicBuilderBase<LakesData>
    {
        protected override LakesData CreateWrapper(List<TerrainPolygon> polygons)
        {
            return new LakesData(polygons);
        }

        protected override TerrainPolygon GetClipArea(IBuildContext context)
        {
            return base.GetClipArea(context).Offset(-25).First(); 
            // Having lakes on edge tends to crash Mikero's MakePbo / tends binarize to produce corrupted WRP files
            // It's not yet clear which tools is getting things wrong
            // It's linked to the map=river of pond models
        }

        protected override bool IsTargeted(TagsCollectionBase tags)
        {
            var water = tags.GetValue("water");
            if (!string.IsNullOrEmpty(water))
            {
                return water == "lake" || water == "pond";
            }
            return tags.GetValue("natural") == "water";
        }

        protected override IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            var embankmentMargin = 2.5f * context.Area.GridCellSize;

            return context.GetData<RoadsData>()
                .Roads.Cast<IWay>().Concat(context.GetData<RailwaysData>().Railways)
                .Where(r => r.SpecialSegment == WaySpecialSegment.Embankment)
                .SelectMany(s => s.Path.ToTerrainPolygon(s.Width + embankmentMargin))
                .ToList();
        }

        public override IEnumerable<IDataDependency> Dependencies => [
            new DataDependency<RoadsData>()
            ];
    }
}
