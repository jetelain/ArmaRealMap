namespace GameRealisticMap.Reporting
{
    internal class ConsoleProgressScope : IDisposable
    {
        private readonly ConsoleProgressSystem owner;

        public ConsoleProgressScope(ConsoleProgressScope? parent, ConsoleProgressSystem owner, string name)
        {
            Parent = parent;
            this.owner = owner;
            Name = name;
        }

        public ConsoleProgressScope? Parent { get; }

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