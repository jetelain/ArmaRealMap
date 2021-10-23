using System.Collections.Generic;
using ArmaRealMap.Geometries;
using ArmaRealMap.Libraries;

namespace ArmaRealMap.TerrainData
{
    class FillShapeWithObjectsBasic : FillShapeWithObjects<List<SingleObjetInfos>>
    {
        public FillShapeWithObjectsBasic(MapData mapData, ObjectCategory library, ObjectLibraries libraries)
            : base(mapData, library, libraries)
        {
        }

        protected override SingleObjetInfos SelectObjectToInsert(FillArea fillarea, List<SingleObjetInfos> clusters, TerrainPoint point)
        {
            if (clusters.Count == 1)
            {
                return clusters[0];
            }
            return clusters[fillarea.Random.Next(0, clusters.Count)];
        }

        protected override List<SingleObjetInfos> GenerateAreaSelectData(FillArea fillarea)
        {
            var clusters = new List<SingleObjetInfos>();
            foreach (var obj in fillarea.Library.Objects)
            {
                var count = 100 * (obj.PlacementProbability ?? 1);
                for (int i = 0; i < count; ++i)
                {
                    clusters.Add(obj);
                }
            }
            return clusters;
        }
    }
}
