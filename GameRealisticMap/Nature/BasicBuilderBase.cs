using GameRealisticMap.Buildings;
using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;
using GameRealisticMap.Roads;
using GameRealisticMap.Water;
using OsmSharp.Tags;

namespace GameRealisticMap.Nature
{
    internal abstract class BasicBuilderBase<T> : IDataBuilder<T>
        where T : class, IBasicTerrainData
    {

        private readonly IProgressSystem progress;
        private readonly string name;

        public BasicBuilderBase(IProgressSystem progress)
        {
            this.progress = progress;
            this.name = GetType().Name.Replace("Builder", "");
        }

        protected abstract bool IsTargeted(TagsCollectionBase tags);

        protected abstract T CreateWrapper(List<TerrainPolygon> polygons);

        protected virtual IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            var roads = context.GetData<RoadsData>();
            var water = context.GetData<WaterData>();
            var buildings = context.GetData<BuildingsData>();

            return buildings.Buildings.Select(b => b.Box.Polygon)
                .Concat(roads.Roads.Where(r => r.RoadType != RoadTypeId.Trail).SelectMany(r => r.Polygons))
                .Concat(water.LakesPolygons);
        }

        public T Build(IBuildContext context)
        {
            var priority = GetPriority(context).ToList();

            var polygons = context.OsmSource.All
                .Where(s => s.Tags != null && IsTargeted(s.Tags))

                .ProgressStep(progress, name + ".Interpret")
                .SelectMany(s => context.OsmSource.Interpret(s))
                .SelectMany(s => TerrainPolygon.FromGeometry(s, context.Area.LatLngToTerrainPoint))

                .ProgressStep(progress, name + ".Crop")
                .SelectMany(poly => poly.ClippedBy(context.Area.TerrainBounds))

                .RemoveOverlaps(progress, name + ".Overlaps")

                .ProgressStep(progress, name + ".Priority")
                .SelectMany(l => l.SubstractAll(priority))
                .ToList();

            return CreateWrapper(polygons);
        }

    }
}
