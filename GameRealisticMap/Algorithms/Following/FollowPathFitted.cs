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

        private const float MinimalSegmentLength = 0.25f; // Ignore anything small than 25cm

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

        private void AddCorner(List<PlacedModel<TModel>> layer, TerrainPoint point, Random random, float prevObjectAngle, float cornerAngle)
        {
            if (cornerAngle > 45 && lib.RightCorners.Count > 0)
            {
                layer.Add(new PlacedModel<TModel>(lib.RightCorners.GetRandom(random).Model, point, prevObjectAngle));
            }
            else if (cornerAngle < -45 && lib.LeftCorners.Count > 0)
            {
                layer.Add(new PlacedModel<TModel>(lib.LeftCorners.GetRandom(random).Model, point, prevObjectAngle));
            }
        }

        private void UniqueSegment(Random random, List<PlacedModel<TModel>> layer, TerrainPathSegment terrainPathSegment)
        {
            if (terrainPathSegment.IsClosed)
            {
                var models = ChooseModels(random, terrainPathSegment.Length, false, 0.9f);
                if (models.Count > 2)
                {
                    PlaceAverage(random, layer, terrainPathSegment, models, false, false, models.Sum(m => m.Size));
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

        private void PlaceAverage(Random random, List<PlacedModel<TModel>> layer, TerrainPathSegment terrainPathSegment, IReadOnlyList<IStraightSegmentDefinition<TModel>> models, bool isFirstSegment, bool isLastSegment, float modelLengths)
        {
            var follow = new FollowPath(terrainPathSegment.Points);
            var factor = terrainPathSegment.Length / modelLengths;
            var prevObjectAngle = PlaceObjects(random, layer, models, isFirstSegment, factor, follow);
            FinishSegment(random, layer, terrainPathSegment, isLastSegment, prevObjectAngle);
        }

        private void PlaceCentered(Random random, List<PlacedModel<TModel>> layer, TerrainPathSegment terrainPathSegment, IReadOnlyList<IStraightSegmentDefinition<TModel>> models, bool isFirstSegment, bool isLastSegment, float modelLengths)
        {
            var follow = new FollowPath(terrainPathSegment.Points);
            follow.Move((terrainPathSegment.Length - modelLengths) / 2);
            var prevObjectAngle = PlaceObjects(random, layer, models, isFirstSegment, 1, follow);
            FinishSegment(random, layer, terrainPathSegment, isLastSegment, prevObjectAngle);
        }

        private float PlaceObjects(Random random, List<PlacedModel<TModel>> layer, IEnumerable<IStraightSegmentDefinition<TModel>> models, bool isFirstSegment, float factor, FollowPath follow)
        {
            var prevObjectAngle = 0f;
            foreach (var model in models)
            {
                var modelSize = model.Size * factor;
                if (!follow.Move(modelSize))
                {
                    Debugger.Break();
                }
                prevObjectAngle = PlaceSingleObject(random, layer, isFirstSegment, follow, model);
                isFirstSegment = false;
            }
            return prevObjectAngle;
        }

        internal void Segment(Random random, List<PlacedModel<TModel>> layer, TerrainPathSegment terrainPathSegment, bool isFirstSegment, bool isLastSegment)
        {
            var models = ChooseModels(random, terrainPathSegment.Length, isFirstSegment || isLastSegment, 0.8f);
            if (models.Count == 0)
            {
                return; // Nothing to do
            }
            var modelLengths = models.Sum(m => m.Size);
            if (modelLengths >= terrainPathSegment.Length)
            {
                if (models.Count < 2)
                {
                    // Cannot adjust anything, average is the only solution
                    PlaceAverage(random, layer, terrainPathSegment, models, isFirstSegment, isLastSegment, modelLengths);
                }
                else
                {
                    PlaceEndsFitted(random, layer, terrainPathSegment, models, isFirstSegment, isLastSegment, modelLengths);
                }
            }
            else
            {
                PlaceCentered(random, layer, terrainPathSegment, models, isFirstSegment, isLastSegment, modelLengths);
            }
        }

        private void PlaceEndsFitted(Random random, List<PlacedModel<TModel>> layer, TerrainPathSegment terrainPathSegment, IReadOnlyList<IStraightSegmentDefinition<TModel>> models, bool isFirstSegment, bool isLastSegment, float modelLengths)
        {
            var firstModel = models[0];
            var lastModel = models[models.Count - 1];

            PlaceFirstFitted(random, layer, terrainPathSegment, isFirstSegment, firstModel);

            if (models.Count > 2)
            {
                var middleSize = modelLengths - firstModel.Size - lastModel.Size;
                var followMid = new FollowPath(terrainPathSegment.Points);
                followMid.Move((terrainPathSegment.Length - middleSize) * firstModel.Size / (firstModel.Size + lastModel.Size));
                PlaceObjects(random, layer, models.Skip(1).Take(models.Count - 2), false, 1, followMid);
            }

            var lastAngle = PlaceLastFitted(random, layer, terrainPathSegment, lastModel);

            FinishSegment(random, layer, terrainPathSegment, isLastSegment, lastAngle);
        }

        private float PlaceLastFitted(Random random, List<PlacedModel<TModel>> layer, TerrainPathSegment terrainPathSegment, IStraightSegmentDefinition<TModel> lastModel)
        {
            var followEnd = new FollowPath(terrainPathSegment.Points);
            followEnd.Move(terrainPathSegment.Length - lastModel.Size);
            followEnd.Move(lastModel.Size);
            return PlaceSingleObject(random, layer, false, followEnd, lastModel);
        }

        private void PlaceFirstFitted(Random random, List<PlacedModel<TModel>> layer, TerrainPathSegment terrainPathSegment, bool isFirstSegment, IStraightSegmentDefinition<TModel> firstModel)
        {
            var followSt = new FollowPath(terrainPathSegment.Points);
            followSt.Move(firstModel.Size);
            PlaceSingleObject(random, layer, isFirstSegment, followSt, firstModel);
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

        private void FinishSegment(Random random, List<PlacedModel<TModel>> layer, TerrainPathSegment terrainPathSegment, bool isLastSegment, float angle)
        {
            var lastPoint = terrainPathSegment.Points[terrainPathSegment.Points.Count - 1];
            if (isLastSegment)
            {
                if (lib.Ends.Count > 0)
                {
                    layer.Add(new PlacedModel<TModel>(lib.Ends.GetRandom(random).Model, lastPoint, angle));
                }
            }
            else
            {
                AddCorner(layer, lastPoint, random, angle, terrainPathSegment.AngleWithNext);
            }
        }

        internal List<IStraightSegmentDefinition<TModel>> ChooseModels(Random random, float pathLength, bool canGiveUp, float toleranceFactor)
        {
            var remainLength = pathLength;
            var models = new List<IStraightSegmentDefinition<TModel>>(); 
            if (pathLength < MinimalSegmentLength)
            {
                return models;
            }
            do
            {
                var wantedObject = baselineStraights.GetRandomWithProportion(random) ?? baselineStraights.First();
                if (remainLength < wantedObject.Size)
                {
                    // Object is too large, try to find a smaller one
                    wantedObject = lib.Straights.OrderBy(o => o.Size).First(o => o.Size >= remainLength * toleranceFactor);
                    wantedObject = lib.Straights.Where(t => t.Size == wantedObject.Size).GetRandomWithProportion(random) ?? wantedObject;

                    if ((canGiveUp && ((wantedObject.Size - remainLength) / pathLength) > 0.25) || remainLength < MinimalSegmentLength) // Not sure about this
                    {
                        // Give up: Would overflow more than 25% of the total path
                        return models;
                    }
                }
                models.Add(wantedObject);
                remainLength -= wantedObject.Size;
            } while (remainLength > 0);
            return models;
        }


    }
}
