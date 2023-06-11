using GameRealisticMap.Geometries;

namespace GameRealisticMap.Arma3
{
    public class Arma3MapConfig : IArma3MapConfig, IImageryOptions
    {
        public Arma3MapConfig(Arma3MapConfigJson arma3MapConfigJson)
        {
            if (!string.IsNullOrEmpty(arma3MapConfigJson.Center))
            {
                TerrainArea = TerrainAreaUTM.CreateFromCenter(arma3MapConfigJson.Center, arma3MapConfigJson.GridCellSize, arma3MapConfigJson.GridSize);
            }
            else
            {
                TerrainArea = TerrainAreaUTM.CreateFromSouthWest(arma3MapConfigJson.SouthWest!, arma3MapConfigJson.GridCellSize, arma3MapConfigJson.GridSize);
            }

            TileSize = arma3MapConfigJson.TileSize;

            Resolution = arma3MapConfigJson.Resolution;

            FakeSatBlend = arma3MapConfigJson.FakeSatBlend;

            if (string.IsNullOrEmpty(arma3MapConfigJson.WorldName))
            {
                WorldName = GetAutomaticWorldName(TerrainArea);
            }
            else
            {
                WorldName = arma3MapConfigJson.WorldName;
            }

            if (string.IsNullOrEmpty(arma3MapConfigJson.PboPrefix))
            {
                PboPrefix = @$"z\arm\addons\{WorldName}";
            }
            else
            {
                PboPrefix = arma3MapConfigJson.PboPrefix;
            }

            AssetConfigFile = arma3MapConfigJson.AssetConfigFile!;

            if (string.IsNullOrEmpty(arma3MapConfigJson.TargetModDirectory))
            {
                TargetModDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GameRealisticMap", "Arma3", "Mods", $"@{WorldName}");
            }
            else
            {
                TargetModDirectory = arma3MapConfigJson.TargetModDirectory;
            }
        }

        public static string GetAutomaticWorldName(ITerrainArea area)
        {
            var coordinate = area.TerrainPointToLatLng(new TerrainPoint(area.SizeInMeters / 2, area.SizeInMeters / 2));
            var mgrs = new CoordinateSharp.Coordinate(coordinate.Y, coordinate.X).MGRS;
            return $"m_{mgrs.LongZone}{mgrs.LatZone.ToLowerInvariant()}{mgrs.Digraph.ToLowerInvariant()}{Math.Truncate(mgrs.Easting) / 100:000}{Math.Truncate(mgrs.Northing/100):000}s{Math.Truncate(area.SizeInMeters / 100)}";
        }

        public ITerrainArea TerrainArea { get; }

        public IImageryOptions Imagery => this;

        public float SizeInMeters => TerrainArea.SizeInMeters;

        public int TileSize { get; }

        public double Resolution { get; }

        public string PboPrefix { get; }

        public float FakeSatBlend { get; }

        public string WorldName { get; }

        public string AssetConfigFile { get; }
        public string TargetModDirectory { get; }
    }
}