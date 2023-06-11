using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Nature.Forests
{
    internal class ForestRadialBuilder : BasicRadialBuilder<ForestRadialData, ForestData>
    {
        public ForestRadialBuilder(IProgressSystem progress) : base(progress, ForestRadialData.Width)
        {

        }

        protected override IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            return base.GetPriority(context);
        }

        protected override ForestRadialData CreateWrapper(List<TerrainPolygon> polygons)
        {
            return new ForestRadialData(polygons);
        }
    }
}
