namespace GameRealisticMap.Reporting
{
    public class ConsoleProgressSystem : ProgressSystemBase, IProgressTask
    {
        public int Total { get; set; }

        public CancellationToken CancellationToken => CancellationToken.None;

        public override IProgressInteger CreateStep(string name, int total)
        {
            return new ConsoleProgressReport(Scope.Prefix + name, total);
        }

        public override IProgressPercent CreateStepPercent(string name)
        {
            return new ConsoleProgressReport(Scope.Prefix + name, 1000);
        }

        public void Dispose()
        {

        }

        public void Failed(Exception e)
        {
            Console.WriteLine(e);
        }

        public void Report(int value)
        {

        }

        public void ReportOneDone()
        {
        }
    }
}
