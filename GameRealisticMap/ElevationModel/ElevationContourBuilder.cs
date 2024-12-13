using GameRealisticMap.Geometries;
using Pmad.Cartography.Contours;
using Pmad.ProgressTracking;

namespace GameRealisticMap.ElevationModel
{
    internal class ElevationContourBuilder : IDataBuilder<ElevationContourData>
    {
        public ElevationContourData Build(IBuildContext context, IProgressScope scope)
        {
            var elevation = context.GetData<ElevationData>().Elevation;

            var contour = new ContourGraph();
            using (var report = scope.CreatePercent("Contours"))
            {
                contour.Add(elevation.ToDataCell(), new ContourLevelGenerator(-50, 5), false, report);
            }

            var paths = contour.Lines.WithProgress(scope, "Lines").Select(l => new TerrainPath(
                TerrainPath.Simplify(l.Points.Select(p => new TerrainPoint((float)p.Longitude, (float)p.Latitude)).ToList()))).ToList();

            var slices = CreateSlices(context.Area.TerrainBounds).ToList();

            paths = slices.SelectParallelManyWithProgress(scope, "Slices", s => paths.SelectMany(p => p.ClippedByEnveloppe(s))).ToList();

            return new ElevationContourData(paths);
        }

        private IEnumerable<ITerrainEnvelope> CreateSlices(ITerrainEnvelope envelope)
        {
            if ( envelope.MaxPoint.X - envelope.MinPoint.X > 2000 )
            {
                return envelope.SplitQuad().SelectMany(CreateSlices);
            }
            return new[] { envelope };
        }
    }
}
