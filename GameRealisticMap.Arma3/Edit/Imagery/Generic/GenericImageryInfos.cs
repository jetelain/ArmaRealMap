using System.Globalization;
using BIS.PAA;
using BIS.WRP;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.IO;
using Pmad.HugeImages;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.Edit.Imagery.Generic
{
    public class GenericImageryInfos : IArma3MapConfig, IImageryInfos
    {
        public GenericImageryInfos(List<GenericTileInfo> otherTileInfos, int tileSize, double resolution, float sizeInMeters, string pboPrefix, int idMapMultiplier)
        {
            OtherTileInfos = otherTileInfos;
            TileSize = tileSize;
            Resolution = resolution;
            SizeInMeters = sizeInMeters;
            IdMapMultiplier = idMapMultiplier;
            PboPrefix = pboPrefix;
        }

        public List<GenericTileInfo> OtherTileInfos { get; }

        public int TileSize { get; }

        public double Resolution { get; }

        public float SizeInMeters { get; }

        public int IdMapMultiplier { get; }

        public int TotalSize => (int)Math.Floor(SizeInMeters / Resolution);

        public string PboPrefix { get; }

        public float FakeSatBlend => throw new NotImplementedException();

        public string WorldName => throw new NotImplementedException();

        public bool UseColorCorrection => throw new NotImplementedException();

        public IEnumerable<GroundDetailTexture> GetGroundDetailTextures()
        {
            return OtherTileInfos.SelectMany(t => Enumerable.Range(0, t.Textures.Count).Select(i => new GroundDetailTexture(t.Textures[i], t.Normals[i]))).Distinct();
        }

        public static async Task<GenericImageryInfos?> TryCreate(IGameFileSystem fileSystem, EditableWrp wrp, string pboPrefix)
        {
            var rvmatCache = new Dictionary<string, TerrainRvMatInfos>();

            var list = new List<IntermediateTileInfo>();
            for (int x = 0; x < wrp.LandRangeX; x++)
            {
                for (int y = 0; y < wrp.LandRangeY; y++)
                {
                    var materialIndex = wrp.MaterialIndex[x + y * wrp.LandRangeX];
                    var materialName = wrp.MatNames[materialIndex];
                    var infos = await GetMaterialInfo(fileSystem, rvmatCache, materialName);
                    if (infos != null)
                    {
                        infos.CellIndexes.Add(new Point(x, y));
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            var tiles = new List<IntermediateTileInfo>();
            foreach (var tileGroup in rvmatCache.Values.GroupBy(v => new { v.Mask, v.Sat }))
            {
                tiles.Add(new IntermediateTileInfo()
                {
                    Mask = tileGroup.Key.Mask,
                    Sat = tileGroup.Key.Sat,
                    UA = tileGroup.First().UA,
                    UB = tileGroup.First().UB,
                    VB = tileGroup.First().VB,
                    CellIndexes = Rectangle.FromLTRB(
                        tileGroup.Min(v => v.CellIndexes.Min(p => p.X)),
                        tileGroup.Min(v => v.CellIndexes.Min(p => p.Y)),
                        tileGroup.Max(v => v.CellIndexes.Max(p => p.X)),
                        tileGroup.Max(v => v.CellIndexes.Max(p => p.Y))),
                    Textures = Merge(tileGroup.Select(v => v.Textures)),
                    Normals = Merge(tileGroup.Select(v => v.Normals)),
                });
            }

            var mask = GetPaa(tiles[0].Mask, fileSystem);

            var sat = GetPaa(tiles[0].Sat, fileSystem);

            if (mask == null || sat == null)
            {
                return null;
            }

            var tileSize = sat.Width;
            var idMapMultiplier = mask.Width / sat.Width;
            var resolution = 1d / (tiles[0].UA * tileSize);
            var landgridCellCount = tiles.Select(t => t.CellIndexes.Width).Max() + 1;
            var tileSizeMeters = landgridCellCount * wrp.CellSize;
            var step = (int)Math.Round(tileSizeMeters / resolution);
            var halfOverlap = (tileSize - step) / 2;

            var fullImageSize = new Size((int)Math.Round(wrp.CellSize * wrp.LandRangeX / resolution), (int)Math.Round(wrp.CellSize * wrp.LandRangeY / resolution));

            var top = fullImageSize.Height + tileSize - halfOverlap - step;

            var mappedTiles = new List<GenericTileInfo>();

            foreach (var tile in tiles)
            {
                var x = Math.Round((halfOverlap - tile.UB * tileSize) / step);
                var y = Math.Round((top - tile.VB * tileSize) / step);

                var grmTile = new ImageryTile((int)x, (int)y, step, halfOverlap, tileSize, top, tile.UA);
                if (Math.Abs(grmTile.UB - tile.UB) > 0.0001 || Math.Abs(grmTile.VB - tile.VB) > 0.0001)
                {
                    // Results mismatch, not compatible
                    return null;
                }

                // It's compatible !
                mappedTiles.Add(new GenericTileInfo(grmTile, tile));
            }

            return new GenericImageryInfos(
                mappedTiles.OrderBy(i => i.X).ThenBy(i => i.Y).ToList(),
                tileSize,
                resolution,
                sizeInMeters: wrp.LandRangeX * wrp.CellSize,
                pboPrefix,
                idMapMultiplier);
        }

        private static PAA? GetPaa(string path, IGameFileSystem fileSystem)
        {
            using var stream = fileSystem.OpenFileIfExists(path);
            if (stream == null)
            {
                return null;
            }
            return new PAA(stream);
        }

        private static async Task<TerrainRvMatInfos?> GetMaterialInfo(IGameFileSystem fileSystem, Dictionary<string, TerrainRvMatInfos> rvmatCache, string materialName)
        {
            if (!rvmatCache.TryGetValue(materialName, out var infos))
            {
                using var content = fileSystem.OpenFileIfExists(materialName);
                if (content == null)
                {
                    return null;
                }

                var contextAsText = await GameConfigHelper.GetText(content);
                if (TerrainRvMatInfos.ShaderMatch.Matches(contextAsText).Count == 0)
                {
                    return null;
                }
                var textures = IdMapHelper.TextureMatch.Matches(contextAsText).Select(m => m.Groups[1].Value).ToList();
                var normals = IdMapHelper.NormalMatch.Matches(contextAsText).Select(m => m.Groups[1].Value).ToList();
                var sat = TerrainRvMatInfos.SatMatch.Matches(contextAsText).Select(m => m.Groups[1].Value).ToList();
                var mask = TerrainRvMatInfos.MaskMatch.Matches(contextAsText).Select(m => m.Groups[1].Value).ToList();
                var transform = TerrainRvMatInfos.UvTransformMatch.Matches(contextAsText).FirstOrDefault();

                if (sat.Count == 1 && mask.Count == 1 && textures.Count > 0 && normals.Count > 0 && transform != null)
                {
                    rvmatCache.Add(materialName, infos = new TerrainRvMatInfos()
                    {
                        Sat = sat[0],
                        Mask = mask[0],
                        Textures = textures,
                        Normals = normals,
                        UA = float.Parse(transform.Groups["UA"].Value, CultureInfo.InvariantCulture),
                        UB = float.Parse(transform.Groups["UB"].Value, CultureInfo.InvariantCulture),
                        VB = float.Parse(transform.Groups["VB"].Value, CultureInfo.InvariantCulture)
                    });
                }
            }
            return infos;
        }

        private static List<string> Merge(IEnumerable<List<string>> enumerable)
        {
            var first = enumerable.First();

            foreach (var item in enumerable.Skip(1))
            {
                for (int i = 0; i < first.Count; i++)
                {
                    if (string.IsNullOrEmpty(first[i]) && !string.IsNullOrEmpty(item[i]))
                    {
                        first[i] = item[i];
                    }
                }
            }
            return first;
        }


        public HugeImage<Rgb24> GetIdMap(IGameFileSystem fileSystem, TerrainMaterialLibrary materials)
        {
            return GetIdMap<Rgb24>(fileSystem, materials);
        }

        public HugeImage<Rgb24> GetSatMap(IGameFileSystem fileSystem)
        {
            return GetSatMap<Rgb24>(fileSystem);
        }

        public HugeImage<TPixel> GetIdMap<TPixel>(IGameFileSystem fileSystem, TerrainMaterialLibrary materials) where TPixel : unmanaged, IPixel<TPixel>
        {
            var parts = new ImageryTilerHugeImagePartitioner(OtherTileInfos.Select(o => o.GrmTile).ToList(), IdMapMultiplier);
            return new HugeImage<TPixel>(new GenericIdMapReadStorage(this, fileSystem, materials), new Size(TotalSize * IdMapMultiplier), new HugeImageSettingsBase(), parts, new TPixel());
        }

        public HugeImage<TPixel> GetSatMap<TPixel>(IGameFileSystem fileSystem) where TPixel : unmanaged, IPixel<TPixel>
        {
            var parts = new ImageryTilerHugeImagePartitioner(OtherTileInfos.Select(o => o.GrmTile).ToList(), 1);
            return new HugeImage<TPixel>(new GenericSatMapReadStorage(this, fileSystem), new Size(TotalSize), new HugeImageSettingsBase(), parts, new TPixel());
        }
    }
}
