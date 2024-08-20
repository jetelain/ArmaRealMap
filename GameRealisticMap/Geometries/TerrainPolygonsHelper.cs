using System.Collections.Concurrent;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Geometries
{
    internal static class TerrainPolygonsHelper
    {
        internal static IReadOnlyCollection<TerrainPolygon> SubstractAll(this IEnumerable<TerrainPolygon> input, IProgressScope progress, string stepName, IReadOnlyCollection<TerrainPolygon> others)
        {
            return SubstractAll(input.ToList(), progress, stepName, others);
        }

        internal static IReadOnlyCollection<TerrainPolygon> SubstractAll(this List<TerrainPolygon> list, IProgressScope progress, string stepName, IReadOnlyCollection<TerrainPolygon> others)
        {
#if PARALLEL
            var result = new ConcurrentQueue<TerrainPolygon>();
            using (var report = progress.CreateInteger(stepName+ " (Parallel)", list.Count))
            {
                Parallel.ForEach(list, polygon =>
                {
                    foreach(var resultPolygon in polygon.SubstractAllSplitted(others))
                    {
                        result.Enqueue(resultPolygon);
                    }
                    report.ReportOneDone();
                });
            }
            return result;
#else
            return
                list.ProgressStep(progress, stepName)
                .SelectMany(l => l.SubstractAll(others))
                .ToList();
#endif
        }


        internal static IEnumerable<TerrainPolygon> SubstractAllSplitted(this TerrainPolygon subject, IEnumerable<TerrainPolygon> others)
        {
            if (subject.EnveloppeArea < 1_000_000)
            {
                return subject.SubstractAll(others);
            }
            var quad = subject.SplitQuad().SelectMany(s => subject.ClippedByEnveloppe(s));
            var result = new ConcurrentQueue<TerrainPolygon>();
            Parallel.ForEach(quad, polygon =>
            {
                foreach (var resultPolygon in polygon.SubstractAllSplitted(others))
                {
                    result.Enqueue(resultPolygon);
                }
            });
            return result;
        }



        internal static List<TerrainPolygon> RemoveOverlaps(this IEnumerable<TerrainPolygon> input, IProgressScope progress, string stepName)
        {
            var list = input.ToList();
            using (var report = progress.CreateInteger(stepName, list.Count))
            {
                return RemoveOverlaps(report, list);
            }
        }

        internal static List<TerrainPolygon> RemoveOverlaps(IProgressInteger report, List<TerrainPolygon> input)
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
