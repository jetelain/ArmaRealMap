using System.Collections.Generic;
using System.Numerics;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;

namespace GameRealisticMap.Arma3.TerrainBuilder
{
    public class ModelPreviewHelper
    {
        private readonly Dictionary<ModelInfo, List<TerrainPolygon>> modelPolygons = new ();
        private readonly Dictionary<ModelInfo, List<TerrainPolygon>> modelVisualPolygons = new();
        private readonly ModelInfoLibrary library;

        public ModelPreviewHelper(ModelInfoLibrary library)
        {
            this.library = library;
        }

        public IEnumerable<Feature> ToGeoJson(IEnumerable<TerrainBuilderObject> objects)
        {
            return objects.SelectMany(o => ToFeatures(GetPolygonsCache(o.Model), o.ToWrpTransform()));
        }

        private IEnumerable<Feature> ToFeatures(List<TerrainPolygon> terrainPolygons, Matrix4x4 matrix)
        {
            return terrainPolygons.Select(p => new Feature(Transform(p, matrix).ToGeoJson(p => p)));
        }

        public IEnumerable<TerrainPolygon> ToPolygons(TerrainBuilderObject obj)
        {
            return ToPolygons(GetPolygonsCache(obj.Model), obj.ToWrpTransform());
        }

        public IEnumerable<TerrainPolygon> ToVisualPolygons(TerrainBuilderObject obj)
        {
            return ToPolygons(GetVisualPolygonsCache(obj.Model), obj.ToWrpTransform());
        }

        private IEnumerable<TerrainPolygon> ToPolygons(List<TerrainPolygon> terrainPolygons, Matrix4x4 matrix)
        {
            return terrainPolygons.Select(p => Transform(p, matrix));
        }

        private TerrainPolygon Transform(TerrainPolygon polygon, Matrix4x4 matrix)
        {
            var x = polygon.Shell.Select(p => Transform(p, matrix)).ToList();
            x.Add(x[0]);
            return new TerrainPolygon(x);
        }

        private TerrainPoint Transform(TerrainPoint point, Matrix4x4 matrix)
        {
            var vector3 = Vector3.Transform(new Vector3(point.X, 0, point.Y), matrix);
            return new TerrainPoint(vector3.X, vector3.Z);
        }

        private List<TerrainPolygon> GetPolygonsCache(ModelInfo model)
        {
            lock (this)
            {
                if (!modelPolygons.TryGetValue(model, out var polygons))
                {
                    modelPolygons.Add(model, polygons = GetPolygons(model));
                }
                return polygons;
            }
        }

        private List<TerrainPolygon> GetVisualPolygonsCache(ModelInfo model)
        {
            lock (this)
            {
                if (!modelVisualPolygons.TryGetValue(model, out var polygons))
                {
                    modelVisualPolygons.Add(model, polygons = GetVisualPolygons(model));
                }
                return polygons;
            }
        }

        private List<TerrainPolygon> GetPolygons(ModelInfo model)
        {
            var list = new List<TerrainPolygon>();
            var p3d = library.ReadODOL(model.Path);
            if (p3d == null)
            {
                return list;
            }
            var geoLod = p3d.Lods.FirstOrDefault(l => l.Resolution == 1E+13f && l.Polygons.Faces.Length > 0)
                ?? p3d.Lods.OrderBy(l => l.Resolution).FirstOrDefault(l => l.Polygons.Faces.Length > 0);
            if (geoLod == null)
            {
                return list;
            }
            foreach (var face in geoLod.Polygons.Faces)
            {
                var points = face.VertexIndices.Select(i => geoLod.Vertices[i]).Select(p => p.Vector3 + p3d.ModelInfo.BoundingCenter.Vector3);
                var x = points.Select(p => new TerrainPoint(p.X, p.Z)).ToList();
                x.Add(x[0]);
                list.Add(new TerrainPolygon(x));
            }
            return TerrainPolygon.MergeAllParallel(list);
        }


        private List<TerrainPolygon> GetVisualPolygons(ModelInfo model)
        {
            var list = new List<TerrainPolygon>();
            var p3d = library.ReadODOL(model.Path);
            if (p3d == null)
            {
                return list;
            }
            var visualLod = p3d.Lods.OrderBy(l => l.Resolution).FirstOrDefault(l => l.Polygons.Faces.Length > 0 && l.Polygons.Faces.Length < 3000 && l.Resolution < 10000) ??
                            p3d.Lods.OrderBy(l => l.Resolution).FirstOrDefault(l => l.Polygons.Faces.Length > 0);
            if (visualLod == null)
            {
                return list;
            }
            foreach (var face in visualLod.Polygons.Faces)
            {
                var points = face.VertexIndices.Select(i => visualLod.Vertices[i]).Select(p => p.Vector3 + p3d.ModelInfo.BoundingCenter.Vector3);
                var x = points.Select(p => new TerrainPoint(p.X, p.Z)).ToList();
                x.Add(x[0]);
                list.Add(new TerrainPolygon(x));
            }
            return TerrainPolygon.MergeAllParallel(list);
        }
    }
}
