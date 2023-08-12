namespace GameRealisticMap.Arma3.GameEngine
{
    public interface IPboCompiler
    {
        Task BinarizeAndCreatePbo(IPboConfig config, IReadOnlyCollection<string> usedModels, IReadOnlyCollection<string> usedRvmat);
    }
}
