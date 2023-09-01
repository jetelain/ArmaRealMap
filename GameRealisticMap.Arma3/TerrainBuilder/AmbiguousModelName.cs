namespace GameRealisticMap.Arma3.TerrainBuilder
{
    public sealed class AmbiguousModelName : ApplicationException
    {
        public AmbiguousModelName(string name, IReadOnlyCollection<string> candidates)
            : base($"Name '{name}' matches multiples files : '{string.Join("', '", candidates)}'")
        {
            Name = name;
            Candidates = candidates;
        }

        public string Name { get; }

        public IReadOnlyCollection<string> Candidates { get; }
    }
}