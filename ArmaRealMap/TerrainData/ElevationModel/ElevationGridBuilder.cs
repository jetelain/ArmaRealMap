using System;
using System.IO;
using ArmaRealMap.ElevationModel;
using ArmaRealMap.Geometries;

namespace ArmaRealMap
{
    class ElevationGridBuilder
    {
        internal static ElevationGrid LoadOrGenerateElevationGrid(MapData data)
        {
            var elevation = new ElevationGrid(data.MapInfos);
            var cacheFile = data.Config.Target.GetCache("elevation-raw.bin");
            if (!File.Exists(cacheFile))
            {
                elevation.LoadFromSRTM(data.Config.SRTM);
                elevation.SaveToBin(cacheFile);
            }
            else
            {
                elevation.LoadFromBin(cacheFile);
            }
            /*
            var x = new[]{
                elevation.ElevationAt( new TerrainPoint(0,0        )),
                elevation.ElevationAt( new TerrainPoint(5,0        )),
                elevation.ElevationAt( new TerrainPoint(10,0       )),
                elevation.ElevationAt( new TerrainPoint(0,5        )),
                elevation.ElevationAt( new TerrainPoint(5,5        )),
                elevation.ElevationAt( new TerrainPoint(10,5       )),
                elevation.ElevationAt( new TerrainPoint(0,10       )),
                elevation.ElevationAt( new TerrainPoint(5,10       )),
                elevation.ElevationAt( new TerrainPoint(10,10      )),
                elevation.ElevationAt( new TerrainPoint(81910,81910)),
                elevation.ElevationAt( new TerrainPoint(81915,81910)),
                elevation.ElevationAt( new TerrainPoint(81920,81910)),
                elevation.ElevationAt( new TerrainPoint(81910,81915)),
                elevation.ElevationAt( new TerrainPoint(81915,81915)),
                elevation.ElevationAt( new TerrainPoint(81920,81915)),
                elevation.ElevationAt( new TerrainPoint(81910,81920)),
                elevation.ElevationAt( new TerrainPoint(81915,81920)),
                elevation.ElevationAt( new TerrainPoint(81920,81920))
            };
            Console.WriteLine(string.Join(";", x));
            */
            return elevation;
        }
    }
}
