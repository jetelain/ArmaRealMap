using System.Numerics;
using BIS.P3D.ODOL;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Arma3.Assets.Detection
{
    public class ObjectPlacementDetectedInfos
    {
        public ObjectPlacementDetectedInfos(RadiusBasedPlacement generalRadius, RadiusBasedPlacement trunkRadius, RectangleBasedPlacement upperRectangle, RectangleBasedPlacement generalRectangle)
        {
            GeneralRadius = generalRadius;
            TrunkRadius = trunkRadius;
            UpperRectangle = upperRectangle;
            GeneralRectangle = generalRectangle;
        }

        public RadiusBasedPlacement GeneralRadius { get; }

        public RadiusBasedPlacement TrunkRadius { get; }

        public RectangleBasedPlacement UpperRectangle { get; }

        public RectangleBasedPlacement GeneralRectangle { get; }

        public static ObjectPlacementDetectedInfos? CreateFromODOL(ODOL odol)
        {
            var points = GetPoints(odol);
            if (points.Count == 0)
            {
                return null;
            }
            return CreateFromPoints(points);
        }

        public static ObjectPlacementDetectedInfos? CreateFromModel(GameRealisticMap.Arma3.TerrainBuilder.ModelInfo model, ModelInfoLibrary library)
        {
            var odol = library.ReadODOL(model.Path);
            if ( odol != null)
            {
                return CreateFromODOL(odol);
            }
            return null;
        }

        public static ObjectPlacementDetectedInfos? CreateFromComposition(Composition composition, ModelInfoLibrary library)
        {
            var points = new List<Vector3>();
            foreach (var obj in composition.Objects)
            {
                var odol = library.ReadODOL(obj.Model.Path);
                if (odol != null)
                {
                    points.AddRange(GetPoints(odol).Select(p => Vector3.Transform(p, obj.Transform)));
                }
            }
            if (points.Count == 0)
            {
                return null;
            }
            return CreateFromPoints(points);
        }

        public static ObjectPlacementDetectedInfos CreateFromPoints(List<Vector3> points)
        {
            var projectedPoints = points.Select(p => new Vector2(p.X, p.Z)).Distinct().ToList();

            return new ObjectPlacementDetectedInfos(
                CreateRadiusBased(projectedPoints, projectedPoints),
                CreateRadiusBased(projectedPoints, GetTrunk(points, projectedPoints)),
                CreateUpperRectangleBased(points, projectedPoints),
                CreateRectangleBased(projectedPoints));
        }


        private static List<Vector3> GetPoints(ODOL odol)
        {
            var geoLod = odol.Lods.FirstOrDefault(l => l.Resolution == 1E+13f && l.Polygons.Faces.Length > 0);
            if (geoLod != null)
            {
                return GetPointsInComponent(odol.ModelInfo.BoundingCenter.Vector3, geoLod);
            }
            var anyLod = odol.Lods.OrderBy(l => l.Resolution).FirstOrDefault(l => l.Polygons.Faces.Length > 0);
            if (anyLod != null)
            {
                return GetPointsInFaces(odol.ModelInfo.BoundingCenter.Vector3, anyLod);
            }
            return new List<Vector3>();
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

        private static List<Vector3> GetPointsInFaces(Vector3 boundingCenter, LOD lod)
        {
            return
                lod.Polygons.Faces

                // Get face vertices
                .SelectMany(f => f.VertexIndices).Distinct()
                .Select(i => lod.Vertices[i])

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

        private static RectangleBasedPlacement CreateUpperRectangleBased(List<Vector3> points, List<Vector2> projectedPoints)
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

        private static RectangleBasedPlacement CreateRectangleBased(List<Vector2> projectedPoints)
        {
            var min = new Vector2(projectedPoints.Min(p => p.X), projectedPoints.Min(p => p.Y));
            var max = new Vector2(projectedPoints.Max(p => p.X), projectedPoints.Max(p => p.Y));
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
