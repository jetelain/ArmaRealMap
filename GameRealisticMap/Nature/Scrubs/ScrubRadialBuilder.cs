using GameRealisticMap.Geometries;
using GameRealisticMap.Nature.Forests;

namespace GameRealisticMap.Nature.Scrubs
{
    internal class ScrubRadialBuilder : BasicRadialBuilder<ScrubRadialData, ScrubData>
    {
        public ScrubRadialBuilder() : base(ScrubRadialData.Width)
        {

        }

        protected override IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            var forestEdge = context.GetData<ForestRadialData>();

            return base.GetPriority(context)
                .Concat(forestEdge.Polygons);
        }

        protected override ScrubRadialData CreateWrapper(List<TerrainPolygon> polygons)
        {
            return new ScrubRadialData(polygons);
        }
    }
}
