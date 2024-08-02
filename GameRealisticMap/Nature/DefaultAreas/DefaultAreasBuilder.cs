using GameRealisticMap.Geometries;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Nature.DefaultAreas
{
    internal class DefaultAreasBuilder : IDataBuilder<DefaultAreasData>
    {
        public DefaultAreasData Build(IBuildContext context, IProgressScope scope)
        {
            var allData = context.GetOfType<INonDefaultArea>().ToList();
            var allPolygons = allData.WithProgress(scope, "Polygons")
                .SelectMany(l => l.Polygons)
                .ToList();
            using var report = scope.CreateSingle("SubstractAll");
            var polygons = context.Area.TerrainBounds.SubstractAllSplitted(allPolygons).ToList();
            return new DefaultAreasData(polygons);
        }
    }
}
