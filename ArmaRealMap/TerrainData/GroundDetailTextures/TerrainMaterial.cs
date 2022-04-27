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
