using System.Collections.Generic;
using System.Linq;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Railways;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Nature.Ocean;
using GameRealisticMap.Nature.Scrubs;
using GameRealisticMap.Nature.Watercourses;

namespace GameRealisticMap.Studio.Shared
{
    public class PreviewMapData
    {
        public PreviewMapData(IContext context)
            : this(context, Enumerable.Empty<PreviewAdditionalLayer>())
        {

        }

        public PreviewMapData(IContext context, IEnumerable<PreviewAdditionalLayer> layers)
        {
            // Enforced layers
            Ocean = context.GetData<OceanData>();
            ElevationWithLakes = context.GetData<ElevationWithLakesData>();
            Scrub = context.GetData<ScrubData>();
            Forest = context.GetData<ForestData>();
            Buildings = context.GetData<BuildingsData>();
            Roads = context.GetData<RoadsData>();
            Railways = context.GetData<RailwaysData>();
            Watercourses = context.GetData<WatercoursesData>();
            Additional = layers.ToList();
        }

        public OceanData Ocean { get; }

        public ElevationWithLakesData ElevationWithLakes { get; }

        public ScrubData Scrub { get; }

        public ForestData Forest { get; }

        public BuildingsData Buildings { get; }

        public RoadsData Roads { get; }

        public RailwaysData Railways { get; }

        public WatercoursesData Watercourses { get; }

        public List<PreviewAdditionalLayer> Additional { get; }
    }
}
