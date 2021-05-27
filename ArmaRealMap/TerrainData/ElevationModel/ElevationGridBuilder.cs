using System.IO;
using ArmaRealMap.ElevationModel;

namespace ArmaRealMap
{
    class ElevationGridBuilder
    {
        internal static ElevationGrid LoadOrGenerateElevationGrid(MapData data)
        {
            var elevation = new ElevationGrid(data.MapInfos);
            var cacheFile = data.Config.Target.GetCache("elevation-raw.asc");
            if (!File.Exists(cacheFile))
            {
                elevation.LoadFromSRTM(data.Config.SRTM);
                elevation.SaveToAsc(cacheFile);
                elevation.SavePreview(data.Config.Target.GetDebug("elevation-raw.bmp"));
            }
            else
            {
                elevation.LoadFromAsc(cacheFile);
            }
            return elevation;
        }
    }
}
