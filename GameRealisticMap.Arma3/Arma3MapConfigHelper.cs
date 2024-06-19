using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Geometries;
using SixLabors.ImageSharp;

namespace GameRealisticMap.Arma3
{
    public static class Arma3MapConfigHelper
    {
        public static Size GetSatMapSize(this IArma3MapConfig config)
        {
            return new Size((int)Math.Ceiling(config.SizeInMeters / config.Resolution));
        }

        public static Size GetIdMapSize(this IArma3MapConfig config)
        {
            return new Size((int)Math.Ceiling(config.SizeInMeters * config.IdMapMultiplier / config.Resolution));
        }

        public static IEnumerable<PointF> TerrainToSatMapPixel(this IArma3MapConfig config, IEnumerable<TerrainPoint> points)
        {
            return points.Select(point => new PointF(
                (float)(point.X / config.Resolution), 
                (float)((config.SizeInMeters - point.Y) / config.Resolution)));
        }

        public static IEnumerable<PointF> TerrainToIdMapPixel(this IArma3MapConfig config, IEnumerable<TerrainPoint> points)
        {
            return points.Select(point => new PointF(
                (float)(point.X * config.IdMapMultiplier / config.Resolution), 
                (float)((config.SizeInMeters - point.Y) * config.IdMapMultiplier / config.Resolution)));
        }
    }
}
