using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Nature
{
    internal class ScrubEdgeBuilder : BasicEdgeBuilder<ScrubEdgeData, ScrubData>
    {
        public ScrubEdgeBuilder(IProgressSystem progress) : base(progress, 15f)
        {

        }

        protected override IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            var forestEdge = context.GetData<ForestEdgeData>();

            return base.GetPriority(context)
                .Concat(forestEdge.Polygons);
        }

        protected override ScrubEdgeData CreateWrapper(List<TerrainPolygon> polygons)
        {
            return new ScrubEdgeData(polygons);
        }
    }
}
