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

        internal static readonly TerrainMaterial[] All = new[] { Dirt, Forest, WetLand, GrassShort, FarmLand, Sand, Rock, Concrete };

        public TerrainMaterial(Color color, string name)
        {
            Color = color;
            Name = name;
        }

        public Color Color { get; }

        public string Name { get; }

        public string RvMat => $"arm_{Name.ToLowerInvariant()}.rvmat";

        public string  ClassName => $"arm_{Name.ToLowerInvariant()}";
    }
}
