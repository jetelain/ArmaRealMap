using System.Numerics;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Algorithms.Following
{
    public static class FollowPathWithObjects
    {
        public static void PlaceOnPathRightAngle<TModel>(IReadOnlyCollection<ISegmentsDefinition<TModel>> lib, List<PlacedModel<TModel>> layer, IReadOnlyList<TerrainPoint> path)
        {
            if (lib.Count != 0 && path.Count > 1)
            {
                var random = RandomHelper.CreateRandom(path.First());

                PlaceOnPathRightAngle(random, lib.GetRandom(random), layer, path);
            }
        }

        public static void PlaceOnPathRightAngle<TModel>(Random random, ISegmentsDefinition<TModel> lib, List<PlacedModel<TModel>> layer, IReadOnlyList<TerrainPoint> path)
        {
            var follow = new FollowPath(path);

            follow.KeepRightAngles = true;

            var straights = lib.Straights;

            var maxSize = straights.Max(t => t.Size);
            var wantedObjects = lib.UseAnySize ? straights.ToList() : straights.Where(t => t.Size == maxSize).ToList();
            var wantedObject = wantedObjects.GetRandomWithProportion(random) ?? wantedObjects.First();
            var previousDeltaAngle = 0d;
            var isFirst = true;

            var isLoop = path[0].Equals(path[path.Count - 1]);

            while (follow.Move(wantedObject.Size))
            {
                var delta = Vector2.Normalize(follow.Current.Vector - follow.Previous.Vector);
                var deltaAngle = MathF.Atan2(delta.Y, delta.X) * 180 / MathF.PI;
                var angle = (180f + deltaAngle) % 360f; // FIXME: Should be North-South (and not West-East)

                if (isFirst)
                {
                    if (lib.Ends.Count > 0 && !isLoop)
                    {
                        layer.Add(new PlacedModel<TModel>(lib.Ends.GetRandom(random).Model, follow.Previous, (float)deltaAngle));
                    }
                    isFirst = false;
                }

                ProcessCorners(lib, layer, follow, random, previousDeltaAngle, deltaAngle);

                var wantedSize = (follow.Previous.Vector - follow.Current.Vector).Length();
                if (Math.Abs(wantedSize - wantedObject.Size) < 0.2f)
                {
                    // Almost fit 
                    var center = new TerrainPoint(Vector2.Lerp(follow.Previous.Vector, follow.Current.Vector, 0.5f));
                    layer.Add(new PlacedModel<TModel>(wantedObject.Model, center, angle));
                }
                else
                {
                    // Does not fit
                    var bestCandidate = straights.OrderBy(o => o.Size).First(o => o.Size >= wantedSize);
                    var best = straights.Where(t => t.Size == bestCandidate.Size).GetRandomWithProportion(random) ?? bestCandidate;
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

                previousDeltaAngle = deltaAngle;

                if (!follow.IsLast && wantedObjects.Count > 1)
                {
                    wantedObject = wantedObjects.GetRandomWithProportion(random) ?? wantedObjects.First();
                }
            }

            if (isLoop)
            {
                var delta = Vector2.Normalize(path[1].Vector - path[0].Vector);
                var deltaAngle = MathF.Atan2(delta.Y, delta.X) * 180 / MathF.PI;
                AddCorner(lib, layer, path[0], random, previousDeltaAngle, deltaAngle);
            }
            else if (lib.Ends.Count > 0)
            {
                layer.Add(new PlacedModel<TModel>(lib.Ends.GetRandom(random).Model, follow.Current, (float)previousDeltaAngle));
            }
        }

        private static void ProcessCorners<TModel>(ISegmentsDefinition<TModel> lib, List<PlacedModel<TModel>> layer, FollowPath follow, Random random, double previousDeltaAngle, float deltaAngle)
        {
            if (!follow.IsLast && !follow.IsFirst)
            {
                AddCorner(lib, layer, follow.Previous, random, previousDeltaAngle, deltaAngle);
            }
        }

        private static void AddCorner<TModel>(ISegmentsDefinition<TModel> lib, List<PlacedModel<TModel>> layer, TerrainPoint point, Random random, double previousDeltaAngle, float deltaAngle)
        {
            var cornerAngle = deltaAngle - previousDeltaAngle;
            if (cornerAngle > 45 && lib.RightCorners.Count > 0)
            {
                layer.Add(new PlacedModel<TModel>(lib.RightCorners.GetRandom(random).Model, point, (float)previousDeltaAngle));
            }
            else if (cornerAngle < -45 && lib.LeftCorners.Count > 0)
            {
                layer.Add(new PlacedModel<TModel>(lib.LeftCorners.GetRandom(random).Model, point, (float)previousDeltaAngle));
            }
        }

        public static void PlaceOnPath<TModel>(IEnumerable<IStraightSegmentDefinition<TModel>> straights, List<PlacedModel<TModel>> layer, IEnumerable<TerrainPoint> path)
        {
            PlaceOnPath(straights,layer, new FollowPath(path));
        }

        public static void PlaceOnPath<TModel>(IEnumerable<IStraightSegmentDefinition<TModel>> straights, List<PlacedModel<TModel>> layer, FollowPath follow)
        {
            var width = straights.Max(t => t.Size);
            var maxObj = straights.First(t => t.Size == width);

            while (follow.Move(width))
            {
                var delta = Vector2.Normalize(follow.Current.Vector - follow.Previous.Vector);
                var angle = (90f+(MathF.Atan2(delta.Y, delta.X) * 180 / MathF.PI)) % 360f;

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
                    var best = straights.OrderBy(o => o.Size).First(o => o.Size >= wantedSize);
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

        public static void PlaceObjectsOnPath<TModel>(Random random, IReadOnlyCollection<IItemDefinition<TModel>> items, List<PlacedModel<TModel>> layer, IReadOnlyCollection<TerrainPoint> path)
        {
            PlaceObjectsOnPath(random, items, layer, new FollowPath(path));
        }

        public static void PlaceObjectsOnPath<TModel>(Random random, IReadOnlyCollection<IItemDefinition<TModel>> items, List<PlacedModel<TModel>> layer, FollowPath follow)
        {
            do
            {
                var obj = items.GetRandom(random);
                var scale = obj.GetScale(random);
                var radiusScaled = obj.Radius * scale;
                if (follow.Move(radiusScaled) && follow.Move(radiusScaled))
                {
                    layer.Add(new PlacedModel<TModel>(
                        obj.Model, 
                        follow.Previous,
                        angle: random.GetAngle(), 
                        relativeElevation: obj.GetElevation(random), 
                        scale: scale));
                }
            } while (!follow.IsLast);
        }
    }
}
