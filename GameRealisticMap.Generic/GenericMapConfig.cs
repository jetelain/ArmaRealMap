using GameRealisticMap.Configuration;
using GameRealisticMap.Generic.Profiles;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Generic
{
    public sealed class GenericMapConfig : IMapProcessingOptions
    {
        public GenericMapConfig(GenericMapConfigJson genericMapConfigJson)
        {
            if (!string.IsNullOrEmpty(genericMapConfigJson.Center))
            {
                TerrainArea = TerrainAreaUTM.CreateFromCenter(genericMapConfigJson.Center, genericMapConfigJson.GridCellSize, genericMapConfigJson.GridSize);
            }
            else
            {
                TerrainArea = TerrainAreaUTM.CreateFromSouthWest(genericMapConfigJson.SouthWest!, genericMapConfigJson.GridCellSize, genericMapConfigJson.GridSize);
            }

            Resolution = genericMapConfigJson.Resolution;

            ExportProfileFile = genericMapConfigJson.ExportProfileFile ?? ExportProfile.Default;

            if (string.IsNullOrEmpty(genericMapConfigJson.TargetDirectory))
            {
                TargetDirectory = GetAutomaticTargetDirectory(TerrainArea);
            }
            else
            {
                TargetDirectory = genericMapConfigJson.TargetDirectory;
            }

            PrivateServiceRoadThreshold = genericMapConfigJson.PrivateServiceRoadThreshold ?? MapProcessingOptions.Default.PrivateServiceRoadThreshold;

            Satellite = genericMapConfigJson.Satellite ?? new SatelliteImageOptions();

            IsPersisted = genericMapConfigJson.IsPersisted;
        }

        public TerrainAreaUTM TerrainArea { get; }

        public double Resolution { get; }

        public string ExportProfileFile { get; }

        public string TargetDirectory { get; }

        public float PrivateServiceRoadThreshold { get; }

        public ISatelliteImageOptions Satellite { get; }

        public bool IsPersisted { get; } = false;

        public static string GetAutomaticName(ITerrainArea area)
        {
            var coordinate = area.TerrainPointToLatLng(new TerrainPoint(area.SizeInMeters / 2, area.SizeInMeters / 2));
            var mgrs = new CoordinateSharp.Coordinate(coordinate.Y, coordinate.X).MGRS;
            return $"{mgrs.LongZone}{mgrs.LatZone.ToLowerInvariant()}{mgrs.Digraph.ToLowerInvariant()}{Math.Truncate(mgrs.Easting) / 100:000}{Math.Truncate(mgrs.Northing / 100):000}s{Math.Truncate(area.SizeInMeters / 100)}";
        }

        public static string GetAutomaticTargetDirectory(ITerrainArea area)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GameRealisticMap", "Generic", GetAutomaticName(area));
        }
    }
}