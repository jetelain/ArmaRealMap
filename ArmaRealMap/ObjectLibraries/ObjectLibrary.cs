using System;
using System.Collections.Generic;
using System.Text;

namespace ArmaRealMap.Libraries
{
    public class ObjectLibrary
    {
        public ObjectCategory Category { get; set; }

        public List<SingleObjetInfos> Objects { get; set; }

        public List<CompositionInfos> Compositions { get; set; }

    }
}
