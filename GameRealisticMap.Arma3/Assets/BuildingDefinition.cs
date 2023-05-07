using System.Numerics;
using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;

namespace GameRealisticMap.Arma3.Assets
{
    internal class BuildingDefinition
    {
        public BuildingDefinition(BuildingTypeId typeId, Vector2 size, Composition composition)
        {
            TypeId = typeId;
            Size = size;
            Composition = composition;
        }

        public BuildingTypeId TypeId { get; }

        public Vector2 Size { get; }

        public Composition Composition { get; }

        [JsonIgnore]
        public float Surface => Size.X * Size.Y;

        internal bool Fits(BoundingBox box, float minFactor, float maxFactor)
        {
            var fWidth = Size.X / box.Width;
            var fDepth = Size.Y / box.Height;
            if (minFactor <= fWidth &&
                 maxFactor >= fWidth &&
                 minFactor <= fDepth &&
                 maxFactor >= fDepth) // 0° rotated
            {
                return true;
            }
            fWidth = Size.X / box.Height;
            fDepth = Size.Y / box.Width;
            if (minFactor <= fWidth &&
                 maxFactor >= fWidth &&
                 minFactor <= fDepth &&
                 maxFactor >= fDepth) // 90° rotated
            {
                return true;
            }
            return false;
        }

        internal float RotateToFit(BoundingBox box, float minFactor, float maxFactor)
        {
            var fWidth = Size.X / box.Width;
            var fDepth = Size.Y / box.Height;
            if (minFactor <= fWidth &&
                 maxFactor >= fWidth &&
                 minFactor <= fDepth &&
                 maxFactor >= fDepth) // 0° rotated
            {
                return 0.0f;
            }
            fWidth = Size.X / box.Height;
            fDepth = Size.Y / box.Width;
            if (minFactor <= fWidth &&
                 maxFactor >= fWidth &&
                 minFactor <= fDepth &&
                 maxFactor >= fDepth) // 90° rotated
            {
                return 90.0f;
            }
            throw new ArgumentException();
        }
    }
}
