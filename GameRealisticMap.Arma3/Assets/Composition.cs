using System.Numerics;
using System.Text.Json.Serialization;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Geometries;
using MathNet.Numerics;

namespace GameRealisticMap.Arma3.Assets
{
    public class Composition
    {
        public Composition()
            : this(new List<CompositionObject>())
        {

        }

        public Composition(CompositionObject obj)
            : this(new List<CompositionObject>(1) { obj })
        {

        }

        [JsonConstructor]
        public Composition(List<CompositionObject> objects)
        {
            Objects = objects;
        }

        public List<CompositionObject> Objects { get; }

        public Composition Translate(Vector2 translation)
        {
            return Translate(new Vector3(translation.X, 0, translation.Y));
        }

        public Composition Translate(Vector3 translation)
        {
            return Transform(Matrix4x4.CreateTranslation(translation));
        }

        private Composition Transform(Matrix4x4 matrix)
        {
            return new Composition(Objects.Select(o => new CompositionObject(o.Model, o.Transform * matrix)).ToList());
        }

        public IEnumerable<TerrainBuilderObject> ToTerrainBuilderObjects(IModelPosition position)
        {
            var matrix = Matrix4x4.CreateRotationY(MathHelper.ToRadians(-position.Angle));
            if (position.Scale != 1f)
            {
                matrix = matrix * Matrix4x4.CreateScale(position.Scale);
            }
            matrix.M42 = position.RelativeElevation;
            matrix.M41 = position.Center.X;
            matrix.M43 = position.Center.Y;
            matrix.M44 = 1f;
            return Objects.Select(o => o.ToTerrainBuilderObject(matrix, ElevationMode.Relative));
        }

        public IEnumerable<TerrainBuilderObject> ToTerrainBuilderObjects(TerrainPoint center, float absoluteElevarion, float angle, float pitch, float scale = 1)
        {
            var matrix = Matrix4x4.CreateRotationX(MathHelper.ToRadians(pitch)) * Matrix4x4.CreateRotationY(MathHelper.ToRadians(-angle));
            if (scale != 1f)
            {
                matrix = matrix * Matrix4x4.CreateScale(scale);
            }
            matrix.M42 = absoluteElevarion;
            matrix.M41 = center.X;
            matrix.M43 = center.Y;
            matrix.M44 = 1f;
            return Objects.Select(o => o.ToTerrainBuilderObject(matrix, ElevationMode.Absolute));
        }

        public static Composition CreateFrom(IEnumerable<TerrainBuilderObject> objects)
        {
            return new Composition(objects.Select(o => new CompositionObject(o.Model, o.ToWrpTransform())).ToList());
        }
        public static Composition CreateFrom(TerrainBuilderObject o)
        {
            return new Composition(new CompositionObject(o.Model, o.ToWrpTransform()));
        }
        public static Composition CreateFromCsv(IEnumerable<string> terrainBuilderCsv, IModelInfoLibrary library)
        {
            return CreateFrom(terrainBuilderCsv
                .Select(line => new TerrainBuilderObject(ElevationMode.Absolute, line, library)));
        }

        public static Composition CreateFromCsv(string terrainBuilderCsv, IModelInfoLibrary library)
        {
            return CreateFromCsv(terrainBuilderCsv.Split('\n').Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)), library);
        }

        public static Composition CreateSingleFrom(ModelInfo model)
        {
            return CreateFrom(new TerrainBuilderObject(model, new TerrainPoint(Vector2.Zero)));
        }

        public static Composition CreateSingleFrom(ModelInfo model, Vector2 center)
        {
            return CreateFrom(new TerrainBuilderObject(model, new TerrainPoint(center)));
        }

        public static Composition CreateSingleFrom(ModelInfo model, Vector2 center, float elevation)
        {
            return CreateFrom(new TerrainBuilderObject(model, new TerrainPoint(center), elevation));
        }
    }
}
