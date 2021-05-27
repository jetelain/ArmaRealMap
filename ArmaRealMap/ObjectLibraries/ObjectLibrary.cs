using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArmaRealMap.Libraries
{
    public class ObjectLibrary
    {
        public ObjectCategory Category { get; set; }

        public List<SingleObjetInfos> Objects { get; set; }

        public List<CompositionInfos> Compositions { get; set; }

        internal SingleObjetInfos GetObject(string name)
        {
            return Objects.First(o => o.Name == name);
        }
    }
}
