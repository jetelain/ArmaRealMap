using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameRealisticMap.Arma3
{
    internal static class MathHelper
    {
        internal static float ToRadians(float angle)
        {
            if (angle == 0)
            {
                return 0;
            }
            return angle * MathF.PI / 180f;
        }

        internal static float FromRadians(double angle)
        {
            if (angle == 0)
            {
                return 0;
            }
            return (float)(angle * 180.0 / Math.PI);
        }

    }
}
