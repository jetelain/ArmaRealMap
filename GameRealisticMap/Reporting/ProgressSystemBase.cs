namespace GameRealisticMap.Reporting
{
    public abstract class ProgressSystemBase : IProgressSystem
    {
        internal ProgressScope Scope { get; set; }

        public ProgressSystemBase()
        {
            Scope = new ProgressScope(null, this, string.Empty);
        }

        public IDisposable CreateScope(string name)
        {
            return Scope = new ProgressScope(Scope, this, name);
        }

        public string Prefix => Scope.Prefix;

        public abstract IProgressInteger CreateStep(string name, int total);

        public abstract IProgressPercent CreateStepPercent(string name);
    }
}