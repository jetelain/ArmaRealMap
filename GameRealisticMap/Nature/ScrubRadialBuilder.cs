using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Nature
{
    internal class ScrubRadialBuilder : BasicRadialBuilder<ScrubRadialData, ScrubData>
    {
        public ScrubRadialBuilder(IProgressSystem progress) : base(progress, 15f)
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
