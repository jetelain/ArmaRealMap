using ArmaRealMap.ElevationModel;

namespace ArmaRealMap
{
    class ElevationGridBuilder
    {
        internal static void BuildElevationGrid(ConfigSRTM configSRTM, AreaInfos area)
        {
            var elevation = new ElevationGrid(area);
            elevation.LoadFromSRTM(configSRTM);
            elevation.SaveToAsc("elevation2.asc");

            var elevation2 = new ElevationGrid(area);
            elevation2.LoadFromAsc("elevation2.asc");
            elevation2.SaveToAsc("elevation3.asc");
        }
    }
}
