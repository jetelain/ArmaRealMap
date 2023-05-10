﻿using System.Numerics;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Algorithms.Following
{
    public static class FollowPathWithObjects
    {
        public static void PlaceOnPathRightAngle<TModel>(IReadOnlyCollection<ISegmentsDefinition<TModel>> lib, List<PlacedModel<TModel>> layer, IEnumerable<TerrainPoint> path)
        {
            if (lib.Count != 0)
            {
                PlaceOnPathRightAngle(lib.GetRandom(path.First()), layer, path);
            }
        }

        public static void PlaceOnPathRightAngle<TModel>(ISegmentsDefinition<TModel> lib, List<PlacedModel<TModel>> layer, IEnumerable<TerrainPoint> path)
        {
            var width = lib.Straights.Max(t => t.Size);
            var maxObj = lib.Straights.First(t => t.Size == width);

            var follow = new FollowPath(path);
            follow.KeepRightAngles = true;

            while (follow.Move(width))
            {
                var delta = Vector2.Normalize(follow.Current.Vector - follow.Previous.Vector);
                var angle = (180f + (MathF.Atan2(delta.Y, delta.X) * 180 / MathF.PI)) % 360f;

                var wantedSize = (follow.Previous.Vector - follow.Current.Vector).Length();
                if (Math.Abs(wantedSize - width) < 0.2f)
                {
                    // Almost fit 
                    var center = new TerrainPoint(Vector2.Lerp(follow.Previous.Vector, follow.Current.Vector, 0.5f));
                    layer.Add(new PlacedModel<TModel>(maxObj.Model, center, angle));
                }
                else
                {
                    // Does not fit
                    var best = lib.Straights.OrderBy(o => o.Size).First(o => o.Size >= wantedSize);
                    if (follow.IsLast || follow.IsAfterRightAngle)
                    {
                        // Adjust to Current (last in segment)
                        var center = follow.Current - (delta * best.Size / 2);
                        layer.Add(new PlacedModel<TModel>(best.Model, center, angle));
                    }
                    else
                    {
                        // Adjust to Previous (first in segment)
                        var center = follow.Previous + (delta * best.Size / 2);
                        layer.Add(new PlacedModel<TModel>(best.Model, center, angle));
                    }
                }
            }
        }

    }
}