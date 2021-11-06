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

        private static readonly Regex TextLine = new Regex(@"\[([a-zA-Z/0-9\.\-]+),""([^""]+)"",""([^""]+)"",\[\[([0-9\.\-]+),([0-9\.\-]+),([0-9\.\-]+)\],\[([0-9\.\-]+),([0-9\.\-]+),([0-9\.\-]+)\],([0-9\.\-]+)\]\]", RegexOptions.Compiled);

        internal void LoadFromXData()
        {
            //Load("Base game", "a.txt", TerrainRegion.Mediterranean, AssetCategory.Structure);
            //Load("Livonia", "c.txt", TerrainRegion.CentralEurope, AssetCategory.Structure);
            //Load("Tanoa", "b2.txt", TerrainRegion.Tropical, AssetCategory.Structure);
            //Load("Base game", "b1.txt", TerrainRegion.Mediterranean, AssetCategory.Structure);
            Load("Base game", "d1.txt", TerrainRegion.Unknown, AssetCategory.Structure);
            Load("Tanoa", "d2.txt", TerrainRegion.Unknown, AssetCategory.Structure);
            Load("Base game", "d3.txt", TerrainRegion.Unknown, AssetCategory.Structure);
            Load("Livonia", "d4.txt", TerrainRegion.Unknown, AssetCategory.Structure);
            /*
            var changes = 0;
            foreach(var img in AssetPreviews.Where(a => a.Width == 1920))
            {
                var format = Image.DetectFormat(img.Data);
                if (format is SixLabors.ImageSharp.Formats.Png.PngFormat)
                {
                    using (var thumb = Image.Load(img.Data))
                    {
                        using (var stream = new MemoryStream())
                        {
                            thumb.SaveAsJpeg(stream);
                            img.Data = stream.ToArray();
                        }
                        Update(img);
                        changes++;
                        if (changes % 10 == 9)
                        {
                            SaveChanges();
                        }
                    }
                }
            }
            SaveChanges();
            */
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
                if (line.Contains("_ruins_", StringComparison.OrdinalIgnoreCase) || line.Contains("_damaged_", StringComparison.OrdinalIgnoreCase)) continue;
                
                var match = TextLine.Match(line);
                if (match.Success)
                {
                    var model = match.Groups[3].Value;

                    byte[] thumbData, jpegData;
                    GetPreviews(Path.Combine("xdata", match.Groups[1].Value + ".png"), out thumbData, out jpegData);

                    var existing = Assets.FirstOrDefault(a => a.ModelPath == model);
                    if (existing == null)
                    {
                        var minX = float.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
                        var minY = float.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture);
                        var minZ = float.Parse(match.Groups[6].Value, CultureInfo.InvariantCulture);
                        var maxX = float.Parse(match.Groups[7].Value, CultureInfo.InvariantCulture);
                        var maxY = float.Parse(match.Groups[8].Value, CultureInfo.InvariantCulture);
                        var maxZ = float.Parse(match.Groups[9].Value, CultureInfo.InvariantCulture);
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
                                new AssetPreview() { Data = jpegData, Width = 1920, Height = 1080 },
                                new AssetPreview() { Data = thumbData, Width = 480, Height = 270 }
                            }
                        };
                        Assets.Add(asset);
                    }
                    else
                    {
                        UpdatePreview(jpegData, 1920, existing);
                        UpdatePreview(thumbData, 480, existing);
                    }
                }
            }
            SaveChanges();
        }

        private void UpdatePreview(byte[] data, int width, Asset existing)
        {
            var preview = AssetPreviews.First(a => a.AssetID == existing.AssetID && a.Width == width);
            preview.Data = data;
            Update(preview);
        }

        private static void GetPreviews(string shot, out byte[] thumbData, out byte[] jpegData)
        {
            using (var thumb = Image.Load(shot))
            {
                using (var stream = new MemoryStream())
                {
                    thumb.SaveAsJpeg(stream);
                    jpegData = stream.ToArray();
                }
                thumb.Mutate(i => i.Resize(480, 270));
                using (var stream = new MemoryStream())
                {
                    thumb.SaveAsPng(stream);
                    thumbData = stream.ToArray();
                }
            }
        }

        private AssetCategory GuessCategory(string value, AssetCategory def)
        {
            if (value.Contains("Houses", StringComparison.OrdinalIgnoreCase))
            {
                return AssetCategory.Building;
            }
            if (value.Contains("structures_", StringComparison.OrdinalIgnoreCase))
            {
                return AssetCategory.Structure;
            }
            if (value.Contains("rocks_", StringComparison.OrdinalIgnoreCase))
            {
                return AssetCategory.Rock;
            }
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
            return def;
        }
    }
}
