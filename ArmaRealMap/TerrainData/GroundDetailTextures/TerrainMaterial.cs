using System;
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

        public TerrainMaterial(Color color, string name)
        {
            DefaultColor = color;
            Name = name;
        }

        public Color GetColor(TerrainRegion region)
        {
            if (this == Default)
            {
                switch(region)
                {
                    case TerrainRegion.Sahel:
                        return Dirt.DefaultColor;
                    case TerrainRegion.CentralEurope:
                        return GrassShort.DefaultColor;
                }
            }
            return DefaultColor;
        }

        public Color DefaultColor { get; }

        public string Name { get; }

        public string RvMatGeneric => $"arm_{Name.ToLowerInvariant()}.rvmat";

        public string RvMat(TerrainRegion terrain) => $"arm_{Name.ToLowerInvariant()}_{terrain.ToString().ToLowerInvariant()}.rvmat";

        public string ClassName(TerrainRegion terrain) => $"arm_{Name.ToLowerInvariant()}_{terrain.ToString().ToLowerInvariant()}";

        // TODO: should read rvmat to get texture names
        internal string NoPx(TerrainRegion terrain) => $"arm_{Name.ToLowerInvariant()}_{terrain.ToString().ToLowerInvariant()}_nopx.paa";
        internal string Co(TerrainRegion terrain) => $"arm_{Name.ToLowerInvariant()}_{terrain.ToString().ToLowerInvariant()}_co.paa";
    }
}
