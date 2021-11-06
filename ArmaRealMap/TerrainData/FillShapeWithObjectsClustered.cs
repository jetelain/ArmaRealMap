using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using ArmaRealMap.Core.ObjectLibraries;
using ArmaRealMap.Geometries;
using ArmaRealMap.Libraries;

namespace ArmaRealMap.TerrainData
{
    class FillShapeWithObjectsClustered : FillShapeWithObjects<SimpleSpacialIndex<SingleObjetInfos>>
    {
        protected static readonly Vector2 searchArea = new Vector2(50, 50);

        public FillShapeWithObjectsClustered(MapData mapData, ObjectCategory library, ObjectLibraries libraries)
            : base(mapData, library, libraries)
        {
        }

        protected override SingleObjetInfos SelectObjectToInsert(FillArea fillarea, SimpleSpacialIndex<SingleObjetInfos> clusters, TerrainPoint point)
        {
            var potential = clusters.Search(point.Vector - searchArea, point.Vector + searchArea);
            if (potential.Count == 0)
            {
                Trace.TraceWarning("No cluster at '{0}'", point);
                return fillarea.Library.Objects.OrderByDescending(o => o.PlacementProbability).First();
            }
            if (potential.Count == 1)
            {
                return potential[0];
            }
            return potential[fillarea.Random.Next(0, potential.Count)];
        }

        protected override SimpleSpacialIndex<SingleObjetInfos> GenerateAreaSelectData(FillArea fillarea)
        {
            var clusters = new SimpleSpacialIndex<SingleObjetInfos>(
                new Vector2(fillarea.X1, fillarea.Y1),
                new Vector2(fillarea.X2 - fillarea.X1 + 1, fillarea.Y2 - fillarea.Y1 + 1));
            var clusterCount = Math.Max(fillarea.ItemsToAdd, 100);
            foreach (var obj in fillarea.Library.Objects)
            {
                var count = clusterCount * (obj.PlacementProbability ?? 1);
                for (int i = 0; i < count; ++i)
                {
                    var x = fillarea.Random.Next(fillarea.X1, fillarea.X2);
                    var y = fillarea.Random.Next(fillarea.Y1, fillarea.Y2);
                    clusters.Insert(new Vector2(x, y), obj);
                }
            }
            return clusters;
        }
    }
}
