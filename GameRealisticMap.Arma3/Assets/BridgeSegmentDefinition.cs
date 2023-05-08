using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameRealisticMap.Arma3.Assets
{
    public class BridgeSegmentDefinition
    {
        public BridgeSegmentDefinition(Composition composition, float size)
        {
            Composition = composition;
            Size = size;
        }

        public Composition Composition { get; }

        public float Size { get; }
    }
}
