using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Nature.DefaultAreas
{
    internal class DefaultAreasBuilder : IDataBuilder<DefaultAreasData>
    {
        private readonly IProgressSystem progress;

        public DefaultAreasBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public DefaultAreasData Build(IBuildContext context)
        {
            var allData = context.GetOfType<INonDefaultArea>().ToList();
            var allPolygons = allData.ProgressStep(progress, "Polygons")
                .SelectMany(l => l.Polygons)
                .ToList();
            using var report = progress.CreateStep("SubstractAll", 1);
            var polygons = context.Area.TerrainBounds.SubstractAllSplitted(allPolygons).ToList();
            return new DefaultAreasData(polygons);
        }
    }
}
