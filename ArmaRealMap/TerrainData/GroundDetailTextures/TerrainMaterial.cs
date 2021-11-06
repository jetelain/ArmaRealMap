using System;
using System.IO;
using System.Text.RegularExpressions;
using ArmaRealMap.Core.ObjectLibraries;
using SixLabors.ImageSharp;

namespace ArmaRealMap.GroundTextureDetails
{
    public class TerrainMaterial
    {
        internal static readonly TerrainMaterial Dirt = new TerrainMaterial(Color.Brown, nameof(Dirt));

        internal static readonly TerrainMaterial Forest = new TerrainMaterial(Color.Green, nameof(Forest));

        internal static readonly TerrainMaterial WetLand = new TerrainMaterial(Color.LightBlue, nameof(WetLand));

        internal static readonly TerrainMaterial GrassShort = new TerrainMaterial(Color.YellowGreen, nameof(GrassShort));

        internal static readonly TerrainMaterial FarmLand = new TerrainMaterial(Color.Yellow, nameof(FarmLand));

        internal static readonly TerrainMaterial Sand = new TerrainMaterial(Color.SandyBrown, nameof(Sand));

        internal static readonly TerrainMaterial Rock = new TerrainMaterial(Color.DarkGray, nameof(Rock));

        internal static readonly TerrainMaterial Concrete = new TerrainMaterial(Color.Gray, nameof(Concrete));

        internal static readonly TerrainMaterial Default = new TerrainMaterial(Color.Black, nameof(Default));

        internal static readonly TerrainMaterial[] All = new[] { Dirt, Forest, WetLand, GrassShort, FarmLand, Sand, Rock, Concrete };

        internal static readonly TerrainMaterial[] AllWithDefault = new[] { Dirt, Forest, WetLand, GrassShort, FarmLand, Sand, Rock, Concrete, Default };

        public TerrainMaterial(Color color, string name)
        {
            DefaultColor = color;
            Name = name;
        }

        public Color GetColor(TerrainRegion region)
        {
            return GetMaterial(region).DefaultColor;
        }

        private TerrainMaterial GetMaterial(TerrainRegion region)
        {
            if (this == Default)
            {
                switch (region)
                {
                    case TerrainRegion.Sahel:
                        return Dirt;
                    case TerrainRegion.CentralEurope:
                        return GrassShort;
                }
            }
            return this;
        }

        private Color DefaultColor { get; }

        private string Name { get; }


        private string GetRvMat(TerrainRegion terrain)
        {
            return File.ReadAllText(Path.Combine(@"P:\z\arm\addons\common\data\gdt", $"arm_{GetMaterial(terrain).Name.ToLowerInvariant()}_{terrain.ToString().ToLowerInvariant()}.rvmat"));
        }

        private static readonly Regex Texture = new Regex(@"texture=""([^""]+)"";", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private string GetTexture(TerrainRegion terrain, int index)
        {
            var matches = Texture.Matches(GetRvMat(terrain));
            return matches[index].Groups[1].Value;
        }

        internal string NoPx(TerrainRegion terrain) => GetTexture(terrain, 0);

        internal string Co(TerrainRegion terrain) => GetTexture(terrain, 1);
    }
}
