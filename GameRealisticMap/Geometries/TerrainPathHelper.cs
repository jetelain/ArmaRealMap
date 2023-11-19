namespace GameRealisticMap.Geometries
{
    public static class TerrainPathHelper
    {
        public static List<TerrainPoint> AutoMergeNotOriented(IList<TerrainPoint> path1, IList<TerrainPoint> path2)
        {
            var f1 = path1[0];
            var l1 = path1[path1.Count - 1];

            var f2 = path2[0];
            var l2 = path2[path2.Count - 1];

            // Our goal : ---1-->---2-->

            var hypothesis =
                new[]
                {
                    // ---1--> ---2-->
                    new { A = l1, B = f2, IsPath1Reversed = false, IsPath2Reversed = false },
                    // ---1--> <--2---
                    new { A = l1, B = l2, IsPath1Reversed = false, IsPath2Reversed = true },
                    // <--1--- <--2---
                    new { A = l2, B = f1, IsPath1Reversed = true, IsPath2Reversed = true },
                    // <--1--- ---2-->
                    new { A = f2, B = f1, IsPath1Reversed = true, IsPath2Reversed = false },
                };

            var best = hypothesis.OrderBy(h => (h.A.Vector - h.B.Vector).Length()).First();
            var result = new List<TerrainPoint>();
            var skip = best.A.Equals(best.B) ? 1 : 0;
            result.AddRange(best.IsPath1Reversed ? path1.Reverse() : path1);
            result.AddRange((best.IsPath2Reversed ? path2.Reverse() : path2).Skip(skip));
            return result;
        }

    }
}
