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

        private static readonly Regex TextLine = new Regex(@"\[([0-9\.\-]+),""([^""]+)"",""([^""]+)"",\[\[([0-9\.\-]+),([0-9\.\-]+),([0-9\.\-]+)\],\[([0-9\.\-]+),([0-9\.\-]+),([0-9\.\-]+)\],([0-9\.\-]+)\]\]", RegexOptions.Compiled);

        internal void LoadFromXData()
        {
            Load("Base game", "veg_vanilla.txt", TerrainRegion.Unknown, AssetCategory.Plant);
            Load("Livonia", "veg_enoch.txt", TerrainRegion.CentralEurope, AssetCategory.Plant);
            Load("Tanoa", "veg_tanoa.txt", TerrainRegion.Unknown, AssetCategory.Plant);
            Load("CUP", "veg_cup.txt", TerrainRegion.Unknown, AssetCategory.Plant);
            Load("JBAD", "veg_jbad.txt", TerrainRegion.Unknown, AssetCategory.Plant);
            Load("JBAD", "rock_jbad.txt", TerrainRegion.Unknown, AssetCategory.Rock);
            Load("JBAD", "struc_jbad.txt", TerrainRegion.Unknown, AssetCategory.Structure);
            Load("JBAD", "build_jbad.txt", TerrainRegion.Unknown, AssetCategory.Building);
        }

        private void Load(string gameModName, string file, TerrainRegion region, AssetCategory def)
        {
            var name = Path.Combine("xdata", file);
            if (!File.Exists(name))
            {
                return;
            }

            var gameMod = GameMods.FirstOrDefault(n => n.Name == gameModName);
            if (gameMod == null)
            {
                gameMod = new GameMod() { Name = gameModName };
                GameMods.Add(gameMod);
                SaveChanges();
            }

            foreach (var line in File.ReadAllLines(name))
            {
                var match = TextLine.Match(line);
                if (match.Success)
                {
                    var model = match.Groups[3].Value;
                    if (!Assets.Any(a => a.ModelPath == model))
                    {
                        var minX = float.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
                        var minY = float.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture);
                        var minZ = float.Parse(match.Groups[6].Value, CultureInfo.InvariantCulture);
                        var maxX = float.Parse(match.Groups[7].Value, CultureInfo.InvariantCulture);
                        var maxY = float.Parse(match.Groups[8].Value, CultureInfo.InvariantCulture);
                        var maxZ = float.Parse(match.Groups[9].Value, CultureInfo.InvariantCulture);
                        var shot = Path.Combine("xdata", match.Groups[1].Value + ".png");
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
                            ModelPath = model,
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
                            AssetCategory = GuessCategory(match.Groups[3].Value, def),
                            Name = Path.GetFileNameWithoutExtension(match.Groups[3].Value.Replace('\\', Path.DirectorySeparatorChar)),
                            Previews = new List<AssetPreview>()
                            {
                                new AssetPreview() { Data = File.ReadAllBytes(shot), Width = 1920, Height = 1080 },
                                new AssetPreview() { Data = thumbData, Width = 480, Height = 270 }
                            }
                        };
                        Assets.Add(asset);
                    }
                }
            }
            SaveChanges();
        }

        private AssetCategory GuessCategory(string value, AssetCategory def)
        {
            if (def == AssetCategory.Plant)
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
            }
            return def;
        }
    }
}
