using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ArmaRealMapWebSite.Entities.Assets
{
    public class AssetsContext : DbContext
    {
        public AssetsContext(DbContextOptions<AssetsContext> options)
            : base(options)
        {
        }

        public DbSet<Asset> Assets { get; set; }
        public DbSet<AssetPreview> AssetPreviews { get; set; }
        public DbSet<GameMod> GameMods { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Asset>().ToTable(nameof(Asset));
            modelBuilder.Entity<AssetPreview>().ToTable(nameof(AssetPreview));
            modelBuilder.Entity<GameMod>().ToTable(nameof(GameMod));

        }

        //[391,"NONE","Jbad_Veg\plants\clutter\jbad_papaver_05.p3d",[[-0.141097,-0.101991,-0.410922],[0.141097,0.106773,0.410923],0.442398]]

        private static readonly Regex TextLine = new Regex(@"\[([0-9\.\-]+),""([^""]+)"",""([^""]+)"",\[\[([0-9\.\-]+),([0-9\.\-]+),([0-9\.\-]+)\],\[([0-9\.\-]+),([0-9\.\-]+),([0-9\.\-]+)\],([0-9\.\-]+)\]\]", RegexOptions.Compiled);

        internal void LoadFromXData()
        {
            if (Assets.Count() == 0)
            {
                Load(new GameMod() { Name = "Base game" }, "veg_vanilla.txt", TerrainRegion.Unknown);
                Load(new GameMod() { Name = "Livonia" }, "veg_enoch.txt", TerrainRegion.CentralEurope);
                Load(new GameMod() { Name = "Tanoa" }, "veg_tanoa.txt", TerrainRegion.Unknown);
                Load(new GameMod() { Name = "CUP" }, "veg_cup.txt", TerrainRegion.Unknown);
                Load(new GameMod() { Name = "JBAD" }, "veg_jbad.txt", TerrainRegion.Unknown);
            }
        }

        private void Load(GameMod gameMod, string file, TerrainRegion region)
        {
            GameMods.Add(gameMod);
            SaveChanges();

            foreach (var line in File.ReadAllLines(Path.Combine("xdata", file)))
            {
                var match = TextLine.Match(line);
                if (match.Success)
                {
                    var minX = float.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
                    var minY = float.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture);
                    var minZ = float.Parse(match.Groups[6].Value, CultureInfo.InvariantCulture);
                    var maxX = float.Parse(match.Groups[7].Value, CultureInfo.InvariantCulture);
                    var maxY = float.Parse(match.Groups[8].Value, CultureInfo.InvariantCulture);
                    var maxZ = float.Parse(match.Groups[9].Value, CultureInfo.InvariantCulture);
                    var shot = Path.Combine("xdata", "veg", match.Groups[1].Value + ".png");
                    byte[] thumbData;
                    using (var thumb = Image.Load(shot))
                    {
                        thumb.Mutate(i => i.Resize(480, 270));
                        using (var stream = new MemoryStream())
                        {
                            thumb.SaveAsPng(stream);
                            thumbData = stream.ToArray();
                        }
                    }

                    var asset = new Asset()
                    {
                        GameMod = gameMod,
                        ClassName = match.Groups[2].Value,
                        ModelPath = match.Groups[3].Value,
                        MinX = minX,
                        MinY = minY,
                        MinZ = minZ,
                        MaxX = maxX,
                        MaxY = maxY,
                        MaxZ = maxZ,
                        BoundingSphereDiameter = float.Parse(match.Groups[10].Value, CultureInfo.InvariantCulture),
                        Width = maxX - minX,
                        Depth = maxY - minY,
                        Height = maxZ - minZ,
                        CX = 0,
                        CY = 0,
                        CZ = 0,
                        TerrainRegions = region,
                        AssetCategory = GuessCategory(match.Groups[3].Value),
                        Name = Path.GetFileNameWithoutExtension(match.Groups[3].Value),
                        Previews = new List<AssetPreview>()
                        {
                            new AssetPreview() { Data = File.ReadAllBytes(shot), Width = 1920, Height = 1080 },
                            new AssetPreview() { Data = thumbData, Width = 480, Height = 270 }
                        }
                    };
                    Assets.Add(asset);
                }
            }
            SaveChanges();
        }

        private AssetCategory GuessCategory(string value)
        {
            if (value.Contains("clutter", StringComparison.OrdinalIgnoreCase))
            {
                return AssetCategory.Clutter;
            }
            if (value.Contains("bush", StringComparison.OrdinalIgnoreCase) || value.Contains("shrub", StringComparison.OrdinalIgnoreCase))
            {
                return AssetCategory.Bush;
            }
            if (value.Contains("tree", StringComparison.OrdinalIgnoreCase))
            {
                return AssetCategory.Tree;
            }
            return AssetCategory.Plant;
        }
    }
}
