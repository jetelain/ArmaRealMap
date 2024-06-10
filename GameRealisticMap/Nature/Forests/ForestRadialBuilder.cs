using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Cutlines;
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
            var cutlines = context.GetData<CutlinesData>();

            return base.GetPriority(context)
                .Concat(cutlines.Polygons);
        }

        protected override ForestRadialData CreateWrapper(List<TerrainPolygon> polygons)
        {
            return new ForestRadialData(polygons);
        }
    }
}
