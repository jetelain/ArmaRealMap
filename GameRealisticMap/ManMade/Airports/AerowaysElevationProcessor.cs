using System.Numerics;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;
using Pmad.ProgressTracking;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.ManMade.Airports
{
    internal sealed class AerowaysElevationProcessor : IElevationProcessorStage1
    {
        public void ProcessStage1(ElevationGrid grid, IContext context, IProgressScope scope)
        {
            var aeroways = context.GetData<AerowaysData>();

            foreach (var airport in aeroways.InsideAirports.WithProgress(scope, "Aeroways"))
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

            var polygons = TerrainPolygon.MergeAll(airport.Aeroways.SelectMany(a => a.ToPolygons(10f)).ToList());

            mutate.Image.Mutate(p =>
            {
                var brush = CreateBrush(grid, mutate, p1, p2, 0.75f);
                foreach (var poly in TerrainPolygon.MergeAll(polygons.SelectMany(p => p.Offset(20f)).ToList()))
                {
                    PolygonDrawHelper.DrawPolygon(p, poly, brush, mutate.ToPixels);
                }
                p.BoxBlur((int)(10f / grid.CellSize.X));

                brush = CreateBrush(grid, mutate, p1, p2, 0.85f);
                foreach (var poly in TerrainPolygon.MergeAll(polygons.SelectMany(p => p.Offset(10f)).ToList()))
                {
                    PolygonDrawHelper.DrawPolygon(p, poly, brush, mutate.ToPixels);
                }
                p.BoxBlur((int)(10f / grid.CellSize.X));

                brush = CreateBrush(grid, mutate, p1, p2, 1f);
                foreach (var poly in polygons)
                {
                    PolygonDrawHelper.DrawPolygon(p, poly, brush, mutate.ToPixels);
                }
                p.BoxBlur((int)(10f / grid.CellSize.X));

                foreach (var poly in polygons)
                {
                    PolygonDrawHelper.DrawPolygon(p, poly, brush, mutate.ToPixels);
                }
            });

            mutate.Apply();
        }

        private static LinearGradientBrush CreateBrush(ElevationGrid grid, ElevationGridArea mutate, TerrainPoint p1, TerrainPoint p2, float alpha)
        {
            return new LinearGradientBrush(
                mutate.ToPixel(p1),
                mutate.ToPixel(p2),
                GradientRepetitionMode.None,
                new ColorStop(0f, mutate.ElevationToColor(grid.ElevationAround(p1)).WithAlpha(alpha)),
                new ColorStop(1f, mutate.ElevationToColor(grid.ElevationAround(p2)).WithAlpha(alpha)));
        }
    }
}
