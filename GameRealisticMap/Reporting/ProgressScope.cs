namespace GameRealisticMap.Reporting
{
    internal class ProgressScope : IDisposable
    {
        private readonly ProgressSystemBase owner;

        public ProgressScope(ProgressScope? parent, ProgressSystemBase owner, string name)
        {
            Parent = parent;
            this.owner = owner;
            Name = name;
        }

        public ProgressScope? Parent { get; }

        public string Name { get; }

        public string Prefix
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                {
                    return string.Empty;
                }
                return /*Parent?.Prefix +*/ Name + ".";
            }
        }

        public void Dispose()
        {
            if (Parent != null)
            {
                owner.Scope = Parent;
            }
        }
    }
}