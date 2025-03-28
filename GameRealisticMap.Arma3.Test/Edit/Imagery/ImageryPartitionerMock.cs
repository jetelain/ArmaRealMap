using GameRealisticMap.Arma3.Edit.Imagery;
using GameRealisticMap.Arma3.GameEngine;

namespace GameRealisticMap.Arma3.Test.Edit.Imagery
{
    internal class ImageryPartitionerMock : IImageryPartitioner
    {
        public ImageryTile GetPartFromId(int partId)
        {
            if (partId == 1)
            {
                return new ImageryTile(0, 0, 0, 0, 0, 0, 0);
            }
            throw new ArgumentOutOfRangeException();
        }
    }
}