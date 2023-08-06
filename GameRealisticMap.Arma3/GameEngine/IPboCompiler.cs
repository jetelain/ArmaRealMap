namespace GameRealisticMap.Arma3.GameEngine
{
    public interface IPboCompiler
    {
        Task BinarizeAndCreatePbo(Arma3MapConfig config, IReadOnlyCollection<string> usedModels, IReadOnlyCollection<string> usedRvmat);
    }
}
