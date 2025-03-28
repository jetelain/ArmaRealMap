using GameRealisticMap.Arma3.GameEngine;

namespace GameRealisticMap.Arma3.Edit.Imagery
{
    internal interface IImageryPartitioner
    {
        ImageryTile GetPartFromId(int partId);
    }
}
