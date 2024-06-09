using GameRealisticMap.Reporting;
using Xunit.Abstractions;

namespace GameRealisticMap.Test
{
    internal class TestProgressSystem : NoProgressSystem
    {
        private ITestOutputHelper output;

        public TestProgressSystem(ITestOutputHelper output)
        {
            this.output = output;
        }

        public override IProgressInteger CreateStep(string name, int total)
        {
            output.WriteLine($"{Prefix}{name} ({total})");
            return base.CreateStep(name, total);
        }

        public override IProgressPercent CreateStepPercent(string name)
        {
            output.WriteLine($"{Prefix}{name}");
            return base.CreateStepPercent(name);
        }

        public override void WriteLine(string message)
        {
            output.WriteLine("  " + message);
        }
    }
}