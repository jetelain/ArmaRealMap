using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Nature
{
    internal class ForestEdgeBuilder : BasicEdgeBuilder<ForestEdgeData,ForestData>
    {
        public ForestEdgeBuilder(IProgressSystem progress) : base(progress, 25f)
        {

        }

        protected override IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            return base.GetPriority(context);
        }

        protected override ForestEdgeData CreateWrapper(List<TerrainPolygon> polygons)
        {
            return new ForestEdgeData(polygons);
        }
    }
}
