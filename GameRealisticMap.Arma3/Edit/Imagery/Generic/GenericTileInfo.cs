using GameRealisticMap.Arma3.GameEngine;

namespace GameRealisticMap.Arma3.Edit.Imagery.Generic
{
    public class GenericTileInfo
    {
        internal GenericTileInfo(ImageryTile grmTile, IntermediateTileInfo tile)
        {
            this.GrmTile = grmTile;
            this.Mask = tile.Mask;
            this.Sat = tile.Sat;
            this.Textures = tile.Textures;
            this.Normals = tile.Normals;
        }

        public int X => GrmTile.X;

        public int Y => GrmTile.Y;

        public ImageryTile GrmTile { get; }

        public string Sat { get; }

        public string Mask { get; }

        public List<string> Textures { get; }

        public List<string> Normals { get; }

    }
}
