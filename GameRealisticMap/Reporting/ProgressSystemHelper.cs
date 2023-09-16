using GameRealisticMap.Geometries;
using System.Collections.Concurrent;

namespace GameRealisticMap.Reporting
{
    public static class ProgressSystemHelper
    {
        public static IEnumerable<T> ProgressStep<T>(this IEnumerable<T> input, IProgressSystem progress, string name)
        {
            return ProgressStep(input.ToList(), progress, name);
        }

        public static IEnumerable<T> ProgressStep<T>(this List<T> input, IProgressSystem progress, string name)
        {
            return Progress(input, progress.CreateStep(name, input.Count));
        }

        public static IEnumerable<T> ProgressStep<T>(this IReadOnlyCollection<T> input, IProgressSystem progress, string name)
        {
            return Progress(input, progress.CreateStep(name, input.Count));
        }

        private static IEnumerable<T> Progress<T>(IEnumerable<T> input, IProgressInteger report)
        {
            using (report)
            {
                foreach (var item in input)
                {
                    yield return item;
                    report.ReportOneDone();
                }
            }
        }

        public static IEnumerable<T> Progress<T>(this IEnumerable<T> input, IProgressTask report)
        {
            return Progress(input, report, report.CancellationToken);
        }

        public static IEnumerable<T> Progress<T>(this IEnumerable<T> input, IProgressInteger report, CancellationToken cancellationToken)
        {
            foreach (var item in input)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    yield break;
                }
                yield return item;
                report.ReportOneDone();
            }
        }

        public static IReadOnlyCollection<TTarget> SelectManyParallel<TSource,TTarget>(this IReadOnlyCollection<TSource> source, IProgressSystem progress, string stepName, Func<TSource,IEnumerable<TTarget>> transform)
        {
            var result = new ConcurrentQueue<TTarget>();
            using (var report = progress.CreateStep(stepName, source.Count))
            {
                Parallel.ForEach(source, sourceItem =>
                {
                    foreach (var resultItem in transform(sourceItem))
                    {
                        result.Enqueue(resultItem);
                    }
                    report.ReportOneDone();
                });
            }
            return result;
        }

        public static IReadOnlyCollection<TTarget> SelectParallel<TSource, TTarget>(this IReadOnlyCollection<TSource> source, IProgressSystem progress, string stepName, Func<TSource, TTarget> transform)
        {
            var result = new ConcurrentQueue<TTarget>();
            using (var report = progress.CreateStep(stepName, source.Count))
            {
                Parallel.ForEach(source, sourceItem =>
                {
                    result.Enqueue(transform(sourceItem));
                    report.ReportOneDone();
                });
            }
            return result;
        }
    }
}
