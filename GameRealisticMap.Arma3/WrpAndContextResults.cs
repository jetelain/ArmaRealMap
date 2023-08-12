using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.ManMade.Places;

namespace GameRealisticMap.Arma3
{
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