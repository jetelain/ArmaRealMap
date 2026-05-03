using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.ManMade.Places;

namespace GameRealisticMap.Arma3
{
    /// <summary>
    /// Holds the results of a WRP generation pass: the compiled WRP data and the
    /// <see cref="BuildContext"/> that produced it (allowing access to all built feature data).
    /// </summary>
    public class WrpAndContextResults 
    {
        public WrpAndContextResults(Arma3MapConfig a3config, IContext context, IReadOnlyCollection<string> models, IReadOnlyCollection<string> usedRvmat)
        {
            UsedRvmat = usedRvmat;
            Config = a3config;
            Context = context;
            UsedModels = models;
        }

        public Arma3MapConfig Config { get; }

        public IContext Context { get; }

        public IReadOnlyCollection<string> UsedModels { get; }

        public IReadOnlyCollection<string> UsedRvmat { get; }

        public string FreindlyName => GameConfigGenerator.GetFreindlyName(Config, Context.GetData<CitiesData>());
    }
}