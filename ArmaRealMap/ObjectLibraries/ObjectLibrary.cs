using System;
using System.Collections.Generic;
using System.Linq;
using ArmaRealMap.Core.ObjectLibraries;

namespace ArmaRealMap.Libraries
{
    public class ObjectLibrary
    {
        private readonly Lazy<List<SingleObjetInfos>> objectsByProbility;

        public ObjectLibrary()
        {
            objectsByProbility = new Lazy<List<SingleObjetInfos>>(() => Objects.Count == 1 ? Objects : Objects.SelectMany(l => Enumerable.Repeat(l, (int)((l.PlacementProbability ?? 1d) * 100))).ToList());
        }

        public ObjectCategory Category { get; set; }

        public List<SingleObjetInfos> Objects { get; set; }

        public List<CompositionInfos> Compositions { get; set; }

        public TerrainRegion? Terrain { get; set; }

        public double? Density { get; set; }
        public double? Probability { get; set; }

        internal SingleObjetInfos GetObject(string name)
        {
            return Objects.First(o => o.Name == name);
        }

        public List<SingleObjetInfos> ObjectsExpendedByProbability => objectsByProbility.Value;

        public SingleObjetInfos GetObject(Random rnd)
        {
            if (Objects.Count == 1)
            {
                return Objects[0];
            }
            return ObjectsExpendedByProbability[rnd.Next(0, ObjectsExpendedByProbability.Count)];
        }
    }
}
