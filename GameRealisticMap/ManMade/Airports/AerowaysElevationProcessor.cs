using System.Numerics;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.ManMade.Airports
{
    internal sealed class AerowaysElevationProcessor : IElevationProcessorStage1
    {
        private readonly IProgressSystem progress;

        public AerowaysElevationProcessor(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public void ProcessStage1(ElevationGrid grid, IContext context)
        {
            var aeroways = context.GetData<AerowaysData>();

            foreach (var airport in aeroways.InsideAirports.ProgressStep(progress, "Aeroways"))
            {
                SmoothAerowaysWithinAirport(airport, grid);
            }
        }

        private void SmoothAerowaysWithinAirport(AirportAeroways airport, ElevationGrid grid)
        {
            var mainRunway = airport.Aeroways.Where(a => a.Type == AerowayTypeId.Runway).OrderByDescending(a => a.Segment.Length).FirstOrDefault();
            if (mainRunway == null)
            {
                return;
            }

            var inside = grid.GetElevationInside(airport.Polygon).ToList();
            if (inside.Count == 0)
            {
                return;
            }

            using var mutate = grid.PrepareToMutate(airport.Polygon.MinPoint - grid.CellSize, airport.Polygon.MaxPoint + grid.CellSize, inside.Min() - 10, inside.Max() + 10);
            var center = airport.Polygon.Centroid;

            var somepath = new TerrainPath(center + mainRunway.OverallVector * grid.SizeInMeters, center - mainRunway.OverallVector * grid.SizeInMeters)
                .ClippedBy(airport.Polygon)
                .ToList();

            var p1 = somepath.First().FirstPoint;
            var p2 = somepath.Last().LastPoint;
            var vi = Vector2.Normalize(p2.Vector - p1.Vector) * 25;

            p1 += vi;
            p2 -= vi;

            var brush = new LinearGradientBrush(
                mutate.ToPixel(p1),
                mutate.ToPixel(p2),
                GradientRepetitionMode.None,
                new ColorStop(0f, mutate.ElevationToColor(grid.ElevationAround(p1))),
                new ColorStop(1f, mutate.ElevationToColor(grid.ElevationAround(p2))));

            foreach (var aeroway in airport.Aeroways)
            {
                mutate.Image.Mutate(p =>
                {
                    foreach (var poly in aeroway.Segment.ToTerrainPolygonButt(aeroway.Width + 10))
                    {
                        PolygonDrawHelper.DrawPolygon(p, poly, brush, mutate.ToPixels);
                    }
                });
            }

            var clone = mutate.Image.Clone();
            mutate.Image.Mutate(p =>
            {
                p.BoxBlur(2);
                p.DrawImage(clone, 1);
            });
            mutate.Apply();
        }
    }
}
