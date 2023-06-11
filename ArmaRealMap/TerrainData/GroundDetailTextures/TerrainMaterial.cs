using ArmaRealMap.Core;
using ArmaRealMap.Core.ObjectLibraries;
using SixLabors.ImageSharp;

namespace ArmaRealMap.GroundTextureDetails
{
    public class TerrainMaterial
    {
        public static readonly TerrainMaterial Dirt = new TerrainMaterial(Color.Brown, nameof(Dirt));

        public static readonly TerrainMaterial Forest = new TerrainMaterial(Color.Green, nameof(Forest));

        public static readonly TerrainMaterial WetLand = new TerrainMaterial(Color.LightBlue, nameof(WetLand));

        public static readonly TerrainMaterial GrassShort = new TerrainMaterial(Color.YellowGreen, nameof(GrassShort));

        public static readonly TerrainMaterial FarmLand = new TerrainMaterial(Color.Yellow, nameof(FarmLand));

        public static readonly TerrainMaterial Sand = new TerrainMaterial(Color.SandyBrown, nameof(Sand));

        public static readonly TerrainMaterial Rock = new TerrainMaterial(Color.DarkGray, nameof(Rock));

        public static readonly TerrainMaterial Concrete = new TerrainMaterial(Color.Gray, nameof(Concrete));

        public static readonly TerrainMaterial Default = new TerrainMaterial(Color.Black, nameof(Default));

        public static readonly TerrainMaterial[] All = new[] { Dirt, Forest, WetLand, GrassShort, FarmLand, Sand, Rock, Concrete };

        public static readonly TerrainMaterial[] AllWithDefault = new[] { Dirt, Forest, WetLand, GrassShort, FarmLand, Sand, Rock, Concrete, Default };

        public TerrainMaterial(Color color, string name)
        {
            DefaultColor = color;
            Name = name;
        }

        public Color GetColor(TerrainRegion region)
        {
            return GetMaterial(region).DefaultColor;
        }

        public TerrainMaterial GetMaterial(TerrainRegion region)
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

        public string Name { get; }

    }
}
