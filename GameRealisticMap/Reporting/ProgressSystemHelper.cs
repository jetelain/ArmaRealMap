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
            using (var report = progress.CreateStep(name, input.Count))
            {
                return Progress(input, report);
            }
        }

        public static IEnumerable<T> Progress<T>(this IEnumerable<T> input, IProgressInteger report)
        {
            foreach (var item in input)
            {
                yield return item;
                report.ReportOneDone();
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
    }
}
