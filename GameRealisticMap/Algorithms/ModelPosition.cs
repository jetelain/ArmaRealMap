using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Algorithms
{
    public class ModelPosition : IModelPosition
    {
        public ModelPosition(TerrainPoint center, float angle, float relativeElevation = 0, float scale = 1)
        {
            Angle = angle;
            Center = center;
            RelativeElevation = relativeElevation;
            Scale = scale;
        }

        public float Angle { get; }

        public TerrainPoint Center { get; }

        public float RelativeElevation { get; }

        public float Scale { get; }
    }
}
