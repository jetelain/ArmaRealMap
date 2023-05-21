﻿using System.Collections.Generic;
using System.Linq;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal static class DefinitionHelper
    {
        internal static double GetNewItemProbility(IReadOnlyList<IWithEditableProbability> list)
        {
            if (list.Count == 0)
            {
                return 1;
            }
            return list.Min(o => o.Probability);
        }

        internal static void SameProbabilities(IReadOnlyList<IWithEditableProbability> list)
        {
            foreach (var item in list)
            {
                item.Probability = 1 / list.Count;
            }
        }

        internal static void EquilibrateProbabilities(IReadOnlyList<IWithEditableProbability> list)
        {
            if (list.Count == 1)
            {
                var item = list[0];
                item.Probability = 1;
            }
            else if (list.Count > 1)
            {
                foreach (var item in list)
                {
                    if (item.Probability <= 0)
                    {
                        item.Probability = 1 / list.Count;
                    }
                }
                var sum = list.Sum(p => p.Probability);
                if (sum != 1)
                {
                    foreach (var item in list)
                    {
                        item.Probability = item.Probability / sum;
                    }
                }
            }
        }
    }
}