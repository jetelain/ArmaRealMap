using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameRealisticMap.Geometries
{
    [Flags]
    public enum BoxSide
    {
        None = 0,

        Top = 1,

        Right = 2,

        Bottom = 4,

        Left = 8
    }
}
