using GameRealisticMap.ElevationModel;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Nature.Ocean;
using GameRealisticMap.Nature.Scrubs;

namespace GameRealisticMap.Studio.Shared
{
    public class PreviewMapData
    {
        public PreviewMapData(IContext context)
        {
            Ocean = context.GetData<OceanData>();
            ElevationWithLakes = context.GetData<ElevationWithLakesData>();
            Scrub = context.GetData<ScrubData>();
            Forest = context.GetData<ForestData>();
            Buildings = context.GetData<BuildingsData>();
            Roads = context.GetData<RoadsData>();
        }

        public OceanData Ocean { get; }

        public ElevationWithLakesData ElevationWithLakes { get; }

        public ScrubData Scrub { get; }

        public ForestData Forest { get; }

        public BuildingsData Buildings { get; }

        public RoadsData Roads { get;}
    }
}
