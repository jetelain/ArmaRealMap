using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Geometries
{
    internal static class TerrainPolygonsHelper
    {
        internal static IEnumerable<TerrainPolygon> RemoveOverlaps(this IEnumerable<TerrainPolygon> input, IProgressSystem progress, string stepName)
        {
            var list = input.ToList();
            using (var report = progress.CreateStep(stepName, list.Count))
            {
                return RemoveOverlaps(report, list);
            }
        }

        internal static IEnumerable<TerrainPolygon> RemoveOverlaps(IProgressInteger report, List<TerrainPolygon> input)
        {
            foreach (var item in input.ToList())
            {
                if (input.Contains(item))
                {
                    var contains = input.Where(f => f != item && item.ContainsOrSimilar(f)).ToList();
                    if (contains.Count > 0)
                    {
                        foreach (var toremove in contains)
                        {
                            input.Remove(toremove);
                        }
                    }
                }
                report.ReportOneDone();
            }
            return input;
        }
    }
}
