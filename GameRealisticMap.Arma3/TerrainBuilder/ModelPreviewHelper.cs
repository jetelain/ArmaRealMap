using System.Numerics;
using BIS.P3D.ODOL;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Arma3.TerrainBuilder
{
    public class ModelPreviewHelper
    {
        private readonly Dictionary<ModelInfo, List<TerrainPolygon>> cacheGeoYAxis = new ();
        private readonly Dictionary<ModelInfo, List<TerrainPolygon>> cacheVisualYAxis = new();
        private readonly Dictionary<ModelInfo, List<List<Vector3>>> cacheGeo = new();
        private readonly Dictionary<ModelInfo, List<List<Vector3>>> cacheVisual = new();
        private readonly ModelInfoLibrary library;

        public ModelPreviewHelper(ModelInfoLibrary library)
        {
            this.library = library;
        }

        public IEnumerable<TerrainPolygon> ToGeoAxisY(TerrainBuilderObject obj)
        {
            // Use an approximation to cache intermediate render (assume that Pitch and Roll are small)
            return ToPolygons(GetOrCreate(cacheGeoYAxis, obj.Model, YProjection.Instance, GetGeoLod), obj.ToWrpTransform(), YProjection.Instance);
        }

        public IEnumerable<TerrainPolygon> ToVisualAxisY(TerrainBuilderObject obj)
        {
            // Use an approximation to cache intermediate render (assume that Pitch and Roll are small)
            return ToPolygons(GetOrCreate(cacheVisualYAxis, obj.Model, YProjection.Instance, GetVisualLod), obj.ToWrpTransform(), YProjection.Instance);
        }

        public IEnumerable<TerrainPolygon> ToGeoAxisZ(TerrainBuilderObject obj)
        {
            return ToPolygons(GetOrCreate(cacheGeo, obj.Model, GetGeoLod), obj.ToWrpTransform(), ZProjection.Instance);
        }

        public IEnumerable<TerrainPolygon> ToVisualAxisZ(TerrainBuilderObject obj)
        {
            return ToPolygons(GetOrCreate(cacheVisual, obj.Model, GetVisualLod), obj.ToWrpTransform(), ZProjection.Instance);
        }
        public IEnumerable<TerrainPolygon> ToGeoAxisX(TerrainBuilderObject obj)
        {
            return ToPolygons(GetOrCreate(cacheGeo, obj.Model, GetGeoLod), obj.ToWrpTransform(), XProjection.Instance);
        }

        public IEnumerable<TerrainPolygon> ToVisualAxisX(TerrainBuilderObject obj)
        {
            return ToPolygons(GetOrCreate(cacheVisual, obj.Model, GetVisualLod), obj.ToWrpTransform(), XProjection.Instance);
        }
        private IEnumerable<TerrainPolygon> ToPolygons(List<TerrainPolygon> terrainPolygons, Matrix4x4 matrix, I3dProjection projection)
        {
            return terrainPolygons.Select(p => Transform(p, matrix, projection));
        }

        private IEnumerable<TerrainPolygon> ToPolygons(List<List<Vector3>> faces, Matrix4x4 matrix, I3dProjection projection)
        {
            return TerrainPolygon.MergeAllParallel(faces.Select(p => Transform(p, matrix, projection)).ToList());
        }

        private TerrainPolygon Transform(List<Vector3> polygon, Matrix4x4 matrix, I3dProjection projection)
        {
            var x = polygon.Select(p => projection.Project(Vector3.Transform(p, matrix))).ToList();
            x.Add(x[0]);
            return new TerrainPolygon(x);
        }

        private TerrainPolygon Transform(TerrainPolygon polygon, Matrix4x4 matrix, I3dProjection projection)
        {
            var x = polygon.Shell.Select(p => Transform(p, matrix, projection)).ToList();
            x.Add(x[0]);
            return new TerrainPolygon(x);
        }

        private TerrainPoint Transform(TerrainPoint point, Matrix4x4 matrix, I3dProjection projection)
        {
            return projection.Project(Vector3.Transform(projection.Unproject(point), matrix));
        }

        private List<TerrainPolygon> GetOrCreate(Dictionary<ModelInfo, List<TerrainPolygon>> cache, ModelInfo model, I3dProjection projection, Func<ModelInfo,LOD?> getLod)
        {
            lock (this)
            {
                if (!cache.TryGetValue(model, out var polygons))
                {
                    cache.Add(model, polygons = GeneratePolygon(GetFaces(getLod(model)), projection.Project));
                }
                return polygons;
            }
        }

        private List<List<Vector3>> GetOrCreate(Dictionary<ModelInfo, List<List<Vector3>>> cache, ModelInfo model, Func<ModelInfo, LOD?> getLod)
        {
            lock (this)
            {
                if (!cache.TryGetValue(model, out var faces))
                {
                    cache.Add(model, faces = GetFaces(getLod(model)));
                }
                return faces;
            }
        }

        private LOD? GetGeoLod(ModelInfo model)
        {
            var p3d = library.ReadODOL(model.Path);
            if (p3d == null)
            {
                return null;
            }
            return p3d.Lods.FirstOrDefault(l => l.Resolution == 1E+13f && l.Polygons.Faces.Length > 0)
                ?? p3d.Lods.OrderBy(l => l.Resolution).FirstOrDefault(l => l.Polygons.Faces.Length > 0);
        }

        private LOD? GetVisualLod(ModelInfo model)
        {
            var p3d = library.ReadODOL(model.Path);
            if (p3d == null)
            {
                return null;
            }
            return p3d.Lods.OrderBy(l => l.Resolution).FirstOrDefault(l => l.Polygons.Faces.Length > 0 && l.Polygons.Faces.Length < 3000 && l.Resolution < 10000) ??
                            p3d.Lods.OrderBy(l => l.Resolution).FirstOrDefault(l => l.Polygons.Faces.Length > 0);
        }

        private List<TerrainPolygon> GeneratePolygon(List<List<Vector3>> faces, Func<Vector3,TerrainPoint> project)
        {
            var list = new List<TerrainPolygon>();
            foreach (var points in faces)
            {
                var x = points.Select(project).ToList();
                x.Add(x[0]);
                list.Add(new TerrainPolygon(x));
            }
            return TerrainPolygon.MergeAllParallel(list);
        }

        private List<List<Vector3>> GetFaces(LOD? geoLod)
        {
            if (geoLod == null)
            {
                return new List<List<Vector3>>();
            }
            var result = new List<List<Vector3>>();
            foreach (var face in geoLod.Polygons.Faces)
            {
                result.Add(face.VertexIndices.Select(i => geoLod.Vertices[i]).Select(p => p.Vector3).ToList());
            }
            return result;
        }
    }
}
