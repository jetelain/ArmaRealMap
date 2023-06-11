namespace GameRealisticMap.Reporting
{
    public interface IProgressTask : IProgressSystem, IProgressInteger
    {
        int Total { get; set; }

        CancellationToken CancellationToken { get; }

        void Failed(Exception e);
    }
}
