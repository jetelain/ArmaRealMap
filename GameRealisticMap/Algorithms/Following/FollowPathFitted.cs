using System.Diagnostics;
using System.Numerics;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Algorithms.Following
{
    internal class FollowPathFitted<TModel>
    {
        private readonly IReadOnlyCollection<IStraightSegmentProportionDefinition<TModel>> baselineStraights;
        private readonly ISegmentsDefinition<TModel> lib;

        public FollowPathFitted(ISegmentsDefinition<TModel> lib)
        {
            this.lib = lib;
            if (lib.UseAnySize)
            {
                baselineStraights = lib.Straights;
            }
            else
            {
                var maxSize = lib.Straights.Max(t => t.Size);
                baselineStraights = lib.Straights.Where(t => t.Size == maxSize).ToList();
            }
        }

        public void PlaceOnPath(Random random, List<PlacedModel<TModel>> layer, IReadOnlyList<TerrainPoint> path)
        {
            var isClosed = path[0].Equals(path[path.Count - 1]);
            var segments = TerrainPathSegment.FromPath(path);
            if (segments.Count == 1)
            {
                UniqueSegment(random, layer, segments[0]);
            }
            else
            {
                var isFirst = true; // First if not closed
                foreach (var segment in segments)
                {
                    Segment(random, layer, segment, isFirst && !isClosed, !segment.HasNext && !isClosed);
                    isFirst = false;
                }
            }
        }

        private void AddCorner(List<PlacedModel<TModel>> layer, TerrainPoint point, Random random, float previousDeltaAngle, float cornerAngle)
        {
            if (cornerAngle > 45 && lib.RightCorners.Count > 0)
            {
                layer.Add(new PlacedModel<TModel>(lib.RightCorners.GetRandom(random).Model, point, previousDeltaAngle));
            }
            else if (cornerAngle < -45 && lib.LeftCorners.Count > 0)
            {
                layer.Add(new PlacedModel<TModel>(lib.LeftCorners.GetRandom(random).Model, point, previousDeltaAngle));
            }
        }

        private void Segment(Random random, List<PlacedModel<TModel>> layer, TerrainPathSegment segment, bool isFirstSegment, bool isLastSegment)
        {
            // May choose average on some criterias ?
            // May give up a part of length on some criterias ?
            PlaceEndsAjusted(random, layer, segment, ChooseModels(random, segment, 0.8f), isFirstSegment, isLastSegment);
        }

        private void UniqueSegment(Random random, List<PlacedModel<TModel>> layer, TerrainPathSegment terrainPathSegment)
        {
            if (terrainPathSegment.IsClosed)
            {
                var models = ChooseModels(random, terrainPathSegment);
                if (models.Count >= 3)
                {
                    PlaceAverage(random, layer, terrainPathSegment, models, false, false);
                }
                else
                {
                    // Give up: can't approximate a closed polygon with 1 or 2 objects
                }
            }
            else
            {
                Segment(random, layer, terrainPathSegment, false, false);
            }
        }

        internal void PlaceAverage(Random random, List<PlacedModel<TModel>> layer, TerrainPathSegment terrainPathSegment, IReadOnlyList<IStraightSegmentDefinition<TModel>> models, bool isFirstSegment, bool isLastSegment)
        {
            var modelLengths = models.Sum(m => m.Size);
            var factor = modelLengths / terrainPathSegment.Length;
            var follow = new FollowPath(terrainPathSegment.Points);
            var previousDeltaAngle = 0f;

            foreach (var model in models)
            {
                var modelSize = model.Size * factor;
                if (!follow.Move(modelSize))
                {
                    Debugger.Break();
                }
                previousDeltaAngle = PlaceSingleObject(random, layer, isFirstSegment, follow, model);
                isFirstSegment = false;
            }

            FinishSegment(random, layer, terrainPathSegment, isLastSegment, previousDeltaAngle);
        }

        internal void PlaceEndsAjusted(Random random, List<PlacedModel<TModel>> layer, TerrainPathSegment terrainPathSegment, IReadOnlyList<IStraightSegmentDefinition<TModel>> models, bool isFirstSegment, bool isLastSegment)
        {
            if (models.Count == 1)
            {
                // Cannot adjust anything, average is the only solution
                PlaceAverage(random, layer, terrainPathSegment, models, isFirstSegment, isLastSegment);
                return;
            }

            var firstModel = models[0];
            var lastModel = models[models.Count - 1];
            
            var follow = new FollowPath(terrainPathSegment.Points);
            follow.Move(firstModel.Size);
            PlaceSingleObject(random, layer, isFirstSegment, follow, firstModel);

            if (models.Count > 2)
            {
                var middleSize = models.Sum(m => m.Size) - firstModel.Size - lastModel.Size;
                follow = new FollowPath(terrainPathSegment.Points);
                follow.Move((terrainPathSegment.Length - middleSize) * firstModel.Size / (firstModel.Size + lastModel.Size));
                foreach (var model in models.Skip(1).Take(models.Count - 2))
                {
                    if (!follow.Move(model.Size))
                    {
                        Debugger.Break();
                    }
                    PlaceSingleObject(random, layer, false, follow, model);
                }
            }

            follow = new FollowPath(terrainPathSegment.Points);
            follow.Move(terrainPathSegment.Length - lastModel.Size);
            follow.Move(lastModel.Size);
            var previousDeltaAngle = PlaceSingleObject(random, layer, false, follow, lastModel);

            FinishSegment(random, layer, terrainPathSegment, isLastSegment, previousDeltaAngle);
        }

        private float PlaceSingleObject(Random random, List<PlacedModel<TModel>> layer, bool isFirst, FollowPath follow, IStraightSegmentDefinition<TModel> model)
        {
            var delta = Vector2.Normalize(follow.Current.Vector - follow.Previous.Vector);
            var deltaAngle = MathF.Atan2(delta.Y, delta.X) * 180 / MathF.PI;
            var angle = (180f + deltaAngle) % 360f; // FIXME: Should be North-South (and not West-East)
            if (isFirst && lib.Ends.Count > 0)
            {
                layer.Add(new PlacedModel<TModel>(lib.Ends.GetRandom(random).Model, follow.Previous, (float)deltaAngle));
            }
            var center = new TerrainPoint(Vector2.Lerp(follow.Previous.Vector, follow.Current.Vector, 0.5f));
            layer.Add(new PlacedModel<TModel>(model.Model, center, angle));
            return deltaAngle;
        }

        private void FinishSegment(Random random, List<PlacedModel<TModel>> layer, TerrainPathSegment terrainPathSegment, bool isLastSegment, float previousDeltaAngle)
        {
            var lastPoint = terrainPathSegment.Points[terrainPathSegment.Points.Count - 1];
            if (isLastSegment)
            {
                if (lib.Ends.Count > 0)
                {
                    layer.Add(new PlacedModel<TModel>(lib.Ends.GetRandom(random).Model, lastPoint, previousDeltaAngle));
                }
            }
            else
            {
                AddCorner(layer, lastPoint, random, previousDeltaAngle, terrainPathSegment.AngleWithNext);
            }
        }

        private List<IStraightSegmentDefinition<TModel>> ChooseModels(Random random, TerrainPathSegment terrainPathSegment, float factor = 1)
        {
            var remainLength = terrainPathSegment.Length;
            var models = new List<IStraightSegmentDefinition<TModel>>();
            do
            {
                var wantedObject = baselineStraights.GetRandomWithProportion(random) ?? baselineStraights.First();
                if (remainLength < wantedObject.Size)
                {
                    // Object is too large, try to find a smaller one
                    wantedObject = lib.Straights.OrderBy(o => o.Size).First(o => o.Size >= remainLength * factor);
                    wantedObject = lib.Straights.Where(t => t.Size == wantedObject.Size).GetRandomWithProportion(random) ?? wantedObject;

                    // May we give up if the size difference is too important ?
                }
                models.Add(wantedObject);
                remainLength -= wantedObject.Size;
            } while (remainLength > 0);
            return models;
        }


    }
}
