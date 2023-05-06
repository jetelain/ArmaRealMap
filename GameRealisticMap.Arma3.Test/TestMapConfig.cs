using GameRealisticMap.Arma3.GameEngine;

namespace GameRealisticMap.Arma3.Test
{
    internal class TestMapConfig : IArma3MapConfig
    {
        public int GridSize => 1024;

        public float GridCellSize => 4;

        public int TileSize => 1024;

        public double Resolution => 1;

        public string PboPrefix => "z\\arm\\addons\\arm_testworld";

        public double TextureSizeInMeters => 4;

        public float SizeInMeters => GridSize * GridCellSize;

        public float FakeSatBlend => 0.5f;
    }
}
