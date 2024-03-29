﻿using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Geometries;
using SixLabors.ImageSharp;

namespace GameRealisticMap.Arma3
{
    public static class Arma3MapConfigHelper
    {
        public static Size GetImagerySize(this IArma3MapConfig config)
        {
            return new Size((int)Math.Ceiling(config.SizeInMeters / config.Resolution));
        }

        public static IEnumerable<PointF> TerrainToPixel(this IArma3MapConfig config, IEnumerable<TerrainPoint> points)
        {
            return points.Select(point => new PointF((float)(point.X / config.Resolution), (float)((config.SizeInMeters - point.Y) / config.Resolution)));
        }
    }
}
