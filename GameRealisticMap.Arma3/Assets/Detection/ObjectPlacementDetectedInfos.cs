using System.Numerics;
using BIS.P3D.ODOL;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Arma3.Assets.Detection
{
    public class ObjectPlacementDetectedInfos
    {
        public ObjectPlacementDetectedInfos(RadiusBasedPlacement generalRadius, RadiusBasedPlacement trunkRadius, RectangleBasedPlacement rectangle)
        {
            GeneralRadius = generalRadius;
            TrunkRadius = trunkRadius;
            Rectangle = rectangle;
        }

        public RadiusBasedPlacement GeneralRadius { get; }

        public RadiusBasedPlacement TrunkRadius { get; }

        public RectangleBasedPlacement Rectangle { get; }

        public static ObjectPlacementDetectedInfos? CreateFromODOL(ODOL odol)
        {
            var geoLod = odol.Lods.FirstOrDefault(l => l.Resolution == 1E+13f);
            if (geoLod == null)
            {
                return null;
            }

            var points = GetPointsInComponent(odol.ModelInfo.BoundingCenter.Vector3, geoLod);

            var projectedPoints = points.Select(p => new Vector2(p.X, p.Z)).Distinct().ToList();

            return new ObjectPlacementDetectedInfos(
                CreateRadiusBased(projectedPoints, projectedPoints),
                CreateRadiusBased(projectedPoints, GetTrunk(points, projectedPoints)),
                CreateRectangleBased(points, projectedPoints));
        }

        private static List<Vector3> GetPointsInComponent(Vector3 boundingCenter, LOD geoLod)
        {
            return
                // Get faces of components
                geoLod.NamedSelections
                .Where(n => n.Name.StartsWith("component", StringComparison.OrdinalIgnoreCase))
                .SelectMany(s => s.SelectedFaces).Distinct()
                .Select(i => geoLod.Polygons.Faces[i])

                // Get face vertices
                .SelectMany(f => f.VertexIndices).Distinct()
                .Select(i => geoLod.Vertices[i])

                // Return vertices corrected by BoundingCenter
                .Select(v => v.Vector3 + boundingCenter)
                .ToList();
        }


        private static List<Vector2> GetTrunk(List<Vector3> points, List<Vector2> projectedPoints)
        {
            var trunk = points.Where(p => p.Y >= -0.05 && p.Y <= 0.75).Select(p => new Vector2(p.X, p.Z)).Distinct().ToList();
            if (trunk.Count == 0)
            {
                return projectedPoints;
            }
            return trunk;
        }

        private static RectangleBasedPlacement CreateRectangleBased(List<Vector3> points, List<Vector2> projectedPoints)
        {
            var scanPoints = projectedPoints;
            var maxY = points.Max(p => p.Y);
            if (maxY > 1.5f)
            {
                var upperPart = points.Where(p => p.Y >= 1).Select(p => new Vector2(p.X, p.Z)).Distinct().ToList();
                if (upperPart.Count > 0)
                {
                    scanPoints = upperPart;
                }
            }
            var min = new Vector2(scanPoints.Min(p => p.X), scanPoints.Min(p => p.Y));
            var max = new Vector2(scanPoints.Max(p => p.X), scanPoints.Max(p => p.Y));
            return new RectangleBasedPlacement((min + max) / 2, max - min);
        }

        private static RadiusBasedPlacement CreateRadiusBased(List<Vector2> projectedPoints, List<Vector2> scannedPoints)
        {
            var cicleTrunk = Circle.CreateFrom(scannedPoints);
            var centerTrunk = cicleTrunk.Center;
            var radiusTrunk = MathF.Sqrt(projectedPoints.Max(p => (p - centerTrunk).LengthSquared()));
            return new RadiusBasedPlacement(centerTrunk, radiusTrunk);
        }
    }
}
