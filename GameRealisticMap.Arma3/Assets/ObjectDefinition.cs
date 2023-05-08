using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Arma3.Assets
{
    internal class ObjectDefinition : IWithProbability
    {
        public ObjectDefinition(Composition composition, double probability)
        {
            Composition = composition;
            Probability = probability;
        }

        public Composition Composition { get; }

        public double Probability { get; }
    }
}
