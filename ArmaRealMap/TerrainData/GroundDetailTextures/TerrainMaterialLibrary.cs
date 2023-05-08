using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using ArmaRealMap.Core;
using ArmaRealMap.Core.ObjectLibraries;
using ArmaRealMap.Core.TerrainMaterials;
using ArmaRealMap.GroundTextureDetails;
using BIS.PAA;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ArmaRealMap.TerrainData.GroundDetailTextures
{
    public class TerrainMaterialLibrary
    {
        private static readonly Regex Texture = new Regex(@"texture=""([^""]+)"";", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            Converters ={
                new JsonStringEnumConverter()
            },
            WriteIndented = true
        };

        private readonly List<TerrainMaterialInfo> infos = new List<TerrainMaterialInfo>();

        private static string GetRvMat(TerrainMaterial material, TerrainRegion terrain)
        {
            return File.ReadAllText(Path.Combine(@"P:\z\arm\addons\common\data\gdt", $"arm_{material.Name.ToLowerInvariant()}_{terrain.ToString().ToLowerInvariant()}.rvmat"));
        }

        private static TerrainMaterialInfo GetInfos(TerrainMaterial material, TerrainRegion terrain)
        {
            var infos = new TerrainMaterialInfo();
            infos.Terrain = terrain;
            infos.Id = Enum.Parse<TerrainMaterialId>(material.Name);

            var matches = Texture.Matches(GetRvMat(material, terrain));
            infos.NormalTexture = matches[0].Groups[1].Value;
            infos.ColorTexture = matches[1].Groups[1].Value;

            var texture = Path.Combine("P:", infos.ColorTexture);
            using (var paaStream = File.OpenRead(texture))
            {
                var paa = new PAA(paaStream);
                var map = paa.Mipmaps.First(m => m.Width == 8);
                var pixels = PAA.GetARGB32PixelData(paa, paaStream, map);
                using (var mem = new MemoryStream())
                {
                    Image.LoadPixelData<Bgra32>(pixels, map.Width, map.Height).SaveAsPng(mem);
                    infos.FakeSatPngImage = mem.ToArray();
                }
            }
            return infos;
        }

        public void LoadFromProjectDrive()
        {
            foreach(var mat in TerrainMaterial.All)
            {
                infos.Add(GetInfos(mat, TerrainRegion.CentralEurope));
                infos.Add(GetInfos(mat, TerrainRegion.Sahel));
            }
        }

        public void LoadFromFile(string filename, TerrainRegion? region = null)
        {
            if (!File.Exists(filename) && Directory.Exists("P:"))
            {
                Console.WriteLine($"'{filename}' is missing, material informations loaded from raw files on P:");
                LoadFromProjectDrive();
                return;
            }

            infos.AddRange(JsonSerializer.Deserialize<TerrainMaterialInfo[]>(File.ReadAllText(filename), options));
            if (region != null) // Prefilter to speedup searchs
            {
                infos.RemoveAll(e => e.Terrain != null && e.Terrain != region.Value && e.Terrain != TerrainRegion.Unknown);
            }
        }

        internal void Save(string filename)
        {
            File.WriteAllText(filename, JsonSerializer.Serialize(infos, options));
        }

        public TerrainMaterialInfo GetInfo(TerrainMaterial material, TerrainRegion region)
        {
            var id = Enum.Parse<TerrainMaterialId>(material.GetMaterial(region).Name);
            var entry = infos.FirstOrDefault(i => i.Id == id && i.Terrain == region) ??
                infos.FirstOrDefault(i => i.Id == id && (i.Terrain == null || i.Terrain == TerrainRegion.Unknown));
            if (entry == null)
            {
                throw new ApplicationException($"Material is missing for '{id}' in '{region}'.");
            }
            return entry;
        }
    }
}
