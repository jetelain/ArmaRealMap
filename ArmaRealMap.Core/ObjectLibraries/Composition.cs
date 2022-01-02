using System.Collections.Generic;

namespace ArmaRealMap.Core.ObjectLibraries
{
    public class Composition
    {
        public float Width { get; set; }
        public float Depth { get; set; }
        public float Height { get; set; }

        public List<CompositionObject> Objects { get; set; }
    }
}