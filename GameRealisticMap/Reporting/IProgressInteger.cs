namespace GameRealisticMap.Reporting
{
    public interface IProgressInteger : IProgress<int>, IDisposable
    {
        void ReportOneDone();
    }
}