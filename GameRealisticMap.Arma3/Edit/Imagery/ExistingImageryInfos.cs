using System.Globalization;
using System.Text.RegularExpressions;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.IO;
using Pmad.HugeImages;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.Edit.Imagery
{
    public class ExistingImageryInfos : IArma3MapConfig
    {
        public ExistingImageryInfos(int tileSize, double resolution, float sizeInMeters, string proPrefix, int idMapMultiplier = 1)
        {
            TileSize = tileSize;
            Resolution = resolution;
            SizeInMeters = sizeInMeters;
            PboPrefix = proPrefix;
            IdMapMultiplier = idMapMultiplier;
        }

        public int TileSize { get; }

        public double Resolution { get; }

        public float SizeInMeters { get; }

        public string PboPrefix { get; }

        public int TotalSize => (int)Math.Floor(SizeInMeters / Resolution);

        float IArma3MapConfig.FakeSatBlend => throw new NotImplementedException();

        string IArma3MapConfig.WorldName => throw new NotImplementedException();

        public bool UseColorCorrection => throw new NotImplementedException();

        public int IdMapMultiplier { get; set; }

        public static ExistingImageryInfos? TryCreate(IProjectDrive projectDrive, string pboPrefix, float sizeInMeters)
        {
            var idmap00 = projectDrive.GetFullPath($"{pboPrefix}\\data\\layers\\M_{0:000}_{0:000}_lca.png");
            var satmap00 = projectDrive.GetFullPath($"{pboPrefix}\\data\\layers\\S_{0:000}_{0:000}_lco.png");
            var rvmat = projectDrive.GetFullPath($"{pboPrefix}\\data\\layers\\P_{0:000}-{0:000}.rvmat");
            if (!File.Exists(idmap00) || !File.Exists(satmap00) || !File.Exists(rvmat))
            {
                return null;
            }

            var firstUA = new Regex("aside\\[\\]={([0-9\\.]+),", RegexOptions.CultureInvariant).Match(File.ReadAllText(rvmat));
            if (!firstUA.Success)
            {
                return null;
            }
            var ua = double.Parse(firstUA.Groups[1].Value, CultureInfo.InvariantCulture);

            var satMapTileSize = Image.Identify(satmap00).Width;
            var satMapResolution = 1d / (ua * satMapTileSize);

            var idMapTileSize = Image.Identify(idmap00).Width;
            var idMapMultiplier = idMapTileSize / satMapTileSize;

            return new ExistingImageryInfos(satMapTileSize, satMapResolution, sizeInMeters, pboPrefix, idMapMultiplier);
        }

        public HugeImage<Rgb24> GetIdMap(IGameFileSystem fileSystem, TerrainMaterialLibrary materials)
        {
            var parts = new ImageryTilerHugeImagePartitioner(CreateTiler(), IdMapMultiplier);
            return new HugeImage<Rgb24>(new IdMapReadStorage(parts, fileSystem, PboPrefix, materials, this), new Size(TotalSize * IdMapMultiplier), new HugeImageSettingsBase(), parts, new Rgb24());
        }

        internal ImageryTiler CreateTiler()
        {
            return new ImageryTiler(TileSize, Resolution, SizeInMeters, IdMapMultiplier);
        }

        public HugeImage<Rgb24> GetSatMap(IGameFileSystem fileSystem)
        {
            var parts = new ImageryTilerHugeImagePartitioner(CreateTiler(), 1);
            return new HugeImage<Rgb24>(new SatMapReadStorage(parts, fileSystem, PboPrefix), new Size(TotalSize), new HugeImageSettingsBase(), parts, new Rgb24());
        }

    }
}
