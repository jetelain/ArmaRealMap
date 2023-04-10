namespace GameRealisticMap.Reporting
{
    public class ConsoleProgressSystem : IProgressSystem
    {
        internal ConsoleProgressScope Scope { get; set; }

        public ConsoleProgressSystem()
        {
            Scope = new ConsoleProgressScope(null, this, string.Empty);
        }

        public IDisposable CreateScope(string name)
        {
            return Scope = new ConsoleProgressScope(Scope, this, name);
        }

        public IProgressInteger CreateStep(string name, int total)
        {
            return new ConsoleProgressReport(Scope.Prefix + name, total);
        }

        public IProgressPercent CreateStepPercent(string name)
        {
            return new ConsoleProgressReport(Scope.Prefix + name, 1000);
        }
    }
}
