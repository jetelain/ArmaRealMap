using System.Numerics;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;

namespace GameRealisticMap.Arma3.TerrainBuilder
{
    public class ModelPreviewHelper
    {
        private readonly Dictionary<ModelInfo, List<TerrainPolygon>> modelPolygons = new ();
        private readonly ModelInfoLibrary library;

        public ModelPreviewHelper(ModelInfoLibrary library)
        {
            this.library = library;
        }

        public IEnumerable<Feature> ToGeoJson(IEnumerable<TerrainBuilderObject> objects)
        {
            return objects.SelectMany(o => ToFeatures(GetPolygonsCache(o.Model), o.ToWrpObject(NoGrid.Zero).Transform.Matrix));
        }

        private IEnumerable<Feature> ToFeatures(List<TerrainPolygon> terrainPolygons, Matrix4x4 matrix)
        {
            return terrainPolygons.Select(p => new Feature(Transform(p, matrix).ToGeoJson()));
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
            if (!modelPolygons.TryGetValue(model, out var polygons))
            {
                modelPolygons.Add(model, polygons = GetPolygons(model));
            }
            return polygons;
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
                ?? p3d.Lods.FirstOrDefault(l => l.Resolution == 0 && l.Polygons.Faces.Length > 0);
            if (geoLod == null)
            {
                return list;
            }
            foreach (var face in geoLod.Polygons.Faces)
            {
                var points = face.VertexIndices.Select(i => geoLod.Vertices[i]).Select(p => p.Vector3 /*+ p3d.ModelInfo.BoundingCenter.Vector3*/);
                var x = points.Select(p => new TerrainPoint(p.X, p.Z)).ToList();
                x.Add(x[0]);
                list.Add(new TerrainPolygon(x));
            }
            return TerrainPolygon.MergeAll(list);
        }
    }
}
