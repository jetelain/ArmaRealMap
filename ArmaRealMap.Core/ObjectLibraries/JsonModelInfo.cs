using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmaRealMap.Core.ObjectLibraries
{
    public class JsonModelInfo
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public float? BoundingCenterX { get; set; }
        public float? BoundingCenterY { get; set; }
        public float? BoundingCenterZ { get; set; }
    }
}
