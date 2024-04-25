using System.Globalization;
using System.Text.RegularExpressions;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.IO;
using HugeImages;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.Edit.Imagery
{
    public class ExistingImageryInfos : IArma3MapConfig
    {
        public ExistingImageryInfos(int tileSize, double resolution, float sizeInMeters, string proPrefix)
        {
            TileSize = tileSize;
            Resolution = resolution;
            SizeInMeters = sizeInMeters;
            PboPrefix = proPrefix;
        }

        public int TileSize { get; }

        public double Resolution { get; }

        public float SizeInMeters { get; }

        public string PboPrefix { get; }

        public int TotalSize => (int)Math.Floor(SizeInMeters / Resolution);

        float IArma3MapConfig.FakeSatBlend => throw new NotImplementedException();

        string IArma3MapConfig.WorldName => throw new NotImplementedException();

        public bool UseColorCorrection => throw new NotImplementedException();

        public static ExistingImageryInfos? TryCreate(ProjectDrive projectDrive, string pboPrefix, float sizeInMeters)
        {
            var png = projectDrive.GetFullPath($"{pboPrefix}\\data\\layers\\M_{0:000}_{0:000}_lca.png");
            var rvmat = projectDrive.GetFullPath($"{pboPrefix}\\data\\layers\\P_{0:000}-{0:000}.rvmat");
            if (!File.Exists(png) || !File.Exists(rvmat))
            {
                return null;
            }

            var firstUA = new Regex("aside\\[\\]={([0-9\\.]+),", RegexOptions.CultureInvariant).Match(File.ReadAllText(rvmat));
            if (!firstUA.Success)
            {
                return null;
            }
            var ua = double.Parse(firstUA.Groups[1].Value, CultureInfo.InvariantCulture);

            int tileSize;
            using (var img = Image.Load(png))
            {
                tileSize = img.Width;
            }

            var resolution = 1d / (ua * tileSize);

            return new ExistingImageryInfos(tileSize, resolution, sizeInMeters, pboPrefix);
        }

        public HugeImage<Rgb24> GetIdMap(IGameFileSystem fileSystem, TerrainMaterialLibrary materials)
        {
            var parts = new ImageryTilerHugeImagePartitioner(CreateTiler());
            return new HugeImage<Rgb24>(new IdMapReadStorage(parts, fileSystem, PboPrefix, materials, this), new Size(TotalSize), new HugeImageSettingsBase(), parts, new Rgb24());
        }

        internal ImageryTiler CreateTiler()
        {
            return new ImageryTiler(TileSize, Resolution, SizeInMeters);
        }

        public HugeImage<Rgb24> GetSatMap(IGameFileSystem fileSystem)
        {
            var parts = new ImageryTilerHugeImagePartitioner(CreateTiler());
            return new HugeImage<Rgb24>(new SatMapReadStorage(parts, fileSystem, PboPrefix), new Size(TotalSize), new HugeImageSettingsBase(), parts, new Rgb24());
        }

    }
}
