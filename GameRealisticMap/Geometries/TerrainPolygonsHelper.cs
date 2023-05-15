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


        internal static ITerrainEnvelope GetEnvelope(this IReadOnlyCollection<ITerrainEnvelope> merged)
        {
            if (merged.Count == 0)
            {
                return Envelope.None;
            }
            if (merged.Count == 1)
            {
                return merged.First();
            }
            return new Envelope(
                new TerrainPoint(merged.Min(m => m.MinPoint.X), merged.Min(m => m.MinPoint.Y)),
                new TerrainPoint(merged.Max(m => m.MaxPoint.X), merged.Max(m => m.MaxPoint.Y)));
        }
    }
}
