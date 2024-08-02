using Pmad.ProgressTracking;
using Xunit.Abstractions;

namespace GameRealisticMap.Test
{
    public sealed class TestProgressSystem : IProgressBase, IProgressInteger, IProgressLong, IProgressPercent, IProgressScope
    {
        public CancellationToken CancellationToken => CancellationToken.None;

        private ITestOutputHelper output;

        public TestProgressSystem(ITestOutputHelper output)
        {
            this.output = output;
        }

        public IProgressInteger CreateInteger(string name, int total)
        {
            output.WriteLine($"{name} ({total})");
            return this;
        }

        public IProgressLong CreateLong(string name, long total)
        {
            output.WriteLine($"{name} ({total})");
            return this;
        }

        public IProgressPercent CreatePercent(string name)
        {
            output.WriteLine($"{name}");
            return this;
        }

        public IProgressScope CreateScope(string name, int estimatedChildrenCount = 0)
        {
            output.WriteLine($"{name}");
            return this;
        }

        public IProgressBase CreateSingle(string name)
        {
            output.WriteLine($"{name}");
            return this;
        }

        public void Dispose()
        {

        }

        public void Report(string value)
        {

        }

        public void Report(int value)
        {

        }

        public void Report(long value)
        {

        }

        public void Report(double value)
        {

        }

        public void ReportOneDone()
        {

        }

        public void WriteLine(string message)
        {
            output.WriteLine("  " + message);
        }
    }
}