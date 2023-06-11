using System.Numerics;
using GameRealisticMap.ElevationModel;

namespace GameRealisticMap.Arma3.Test
{
    internal class Arma3MapConfigMock : IArma3MapConfig, IElevationGridConfig
    {
        internal static Arma3MapConfigMock Gossi = new Arma3MapConfigMock() {
            CellSize = new Vector2(10),
            Resolution = 2,
            Size = 8192,
            PboPrefix = "prefix",
            TileSize = 1024
        };
        internal static Arma3MapConfigMock Taunus = new Arma3MapConfigMock()
        {
            CellSize = new Vector2(5),
            Resolution = 2,
            Size = 4096,
            PboPrefix = "prefix",
            TileSize = 512
        };
        internal static Arma3MapConfigMock Belfort = new Arma3MapConfigMock()
        {
            CellSize = new Vector2(5),
            Resolution = 1,
            Size = 4096,
            PboPrefix = "prefix",
            TileSize = 1024
        };

        public float SizeInMeters => Size * CellSize.X;

        public int TileSize { get; set; }

        public double Resolution { get; set; }

        public string PboPrefix { get; set; }

        public float FakeSatBlend { get; set; }

        public string WorldName { get; set; }

        public int Size { get; set; }

        public Vector2 CellSize { get; set; }

    }
}
