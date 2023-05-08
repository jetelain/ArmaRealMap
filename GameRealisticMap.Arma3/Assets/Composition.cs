using System.Numerics;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Arma3.Assets
{
    public class Composition
    {
        internal Composition(List<CompositionObject> objects)
        {
            Objects = objects;
        }

        internal List<CompositionObject> Objects { get; }

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
            return new Composition(objects.Select(o => new CompositionObject(o.Model, o.ToWrpObject(NoGrid.Zero).Transform.Matrix)).ToList());
        }

        public static Composition CreateFromCsv(IEnumerable<string> terrainBuilderCsv, IModelInfoLibrary library, float elevationOffset)
        {
            return CreateFrom(terrainBuilderCsv
                .Select(line => new TerrainBuilderObject(ElevationMode.Absolute, line, library))
                .Select(line => new TerrainBuilderObject(line.Model, line.Point, line.Elevation - elevationOffset, ElevationMode.Absolute, line.Yaw, line.Pitch, line.Roll, line.Scale)));
        }

        public static Composition CreateFromCsv(string terrainBuilderCsv, IModelInfoLibrary library, float elevationOffset)
        {
            return CreateFromCsv(terrainBuilderCsv.Split('\n').Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)), library, elevationOffset);
        }
    }
}
