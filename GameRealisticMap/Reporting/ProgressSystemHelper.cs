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
                foreach (var item in input)
                {
                    yield return item;
                    report.ReportOneDone();
                }
            }
        }

    }
}
