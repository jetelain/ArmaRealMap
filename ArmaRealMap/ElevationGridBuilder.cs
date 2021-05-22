using System.IO;
using ArmaRealMap.ElevationModel;

namespace ArmaRealMap
{
    class ElevationGridBuilder
    {
        internal static ElevationGrid BuildElevationGrid(ConfigSRTM configSRTM, MapInfos area, string targetFile)
        {
            var elevation = new ElevationGrid(area);
            elevation.LoadFromSRTM(configSRTM);
            elevation.SaveToAsc(targetFile);
            elevation.SavePreview(Path.ChangeExtension(targetFile,".bmp"));
            return elevation;
        }

        internal static ElevationGrid LoadOrGenerateElevationGrid(Config config, MapInfos area)
        {
            var rawElevation = Path.Combine(config.Target?.Terrain ?? string.Empty, "elevation-raw.asc");
            if (!File.Exists(rawElevation))
            {
                return BuildElevationGrid(config.SRTM, area, rawElevation);
            }
            var elevation = new ElevationGrid(area);
            elevation.LoadFromAsc(rawElevation);
            //elevation.SavePreview(Path.ChangeExtension(rawElevation, ".bmp"));
            return elevation;
        }
    }
}
