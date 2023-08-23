using System.Diagnostics;

namespace GameRealisticMap.Reporting
{
    public class ConsoleProgressSystem : ProgressSystemBase, IProgressTask
    {
        private readonly Stopwatch sw;

        public ConsoleProgressSystem()
        {
            this.sw = Stopwatch.StartNew();
        }

        public int Total { get; set; }

        public CancellationToken CancellationToken => CancellationToken.None;

        public double MemoryPeakGB { get; internal set; }

        public override IProgressInteger CreateStep(string name, int total)
        {
            return new ConsoleProgressReport(Scope.Prefix + name, total, this);
        }

        public override IProgressPercent CreateStepPercent(string name)
        {
            return new ConsoleProgressReport(Scope.Prefix + name, 1000, this);
        }

        public void Dispose()
        {

        }

        public void DisplayReport()
        {
            var process = Process.GetCurrentProcess();

            Console.WriteLine($"Elapsed: {sw.Elapsed}, Memory peak: {MemoryPeakGB:0.0} G, Total CPU: {process.TotalProcessorTime} ({process.TotalProcessorTime.TotalMilliseconds/sw.Elapsed.TotalMilliseconds:0.00} average)");
#if DEBUG
            Console.WriteLine($"Note: This is a DEBUG binary, performance could be better with a RELEASE binary");
#endif
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
