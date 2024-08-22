using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Cutlines;

namespace GameRealisticMap.Nature.Forests
{
    internal class ForestRadialBuilder : BasicRadialBuilder<ForestRadialData, ForestData>
    {
        public ForestRadialBuilder()
            : base(ForestRadialData.Width)
        {

        }

        protected override IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            var cutlines = context.GetData<CutlinesData>();

            return base.GetPriority(context)
                .Concat(cutlines.Polygons);
        }
        public override IEnumerable<IDataDependency> Dependencies => base.Dependencies.Concat([
            new DataDependency<CutlinesData>()
        ]);

        protected override ForestRadialData CreateWrapper(List<TerrainPolygon> polygons)
        {
            return new ForestRadialData(polygons);
        }
    }
}
