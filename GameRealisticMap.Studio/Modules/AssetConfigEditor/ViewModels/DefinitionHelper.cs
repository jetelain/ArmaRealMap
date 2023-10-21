using System.Collections.Generic;
using System.Linq;
using GameRealisticMap.Studio.UndoRedo;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal static class DefinitionHelper
    {
        internal static double GetNewItemProbility<T>(IReadOnlyList<T> list)
            where T : IWithEditableProbability
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

        internal static void EquilibrateProbabilities<T>(IReadOnlyList<T> list)
            where T : IWithEditableProbability
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
        internal static void Equiprobable(IReadOnlyList<IWithEditableProbability> items, IUndoRedoManager undoRedoManager)
        {
            if (items.Count > 0)
            {
                var probability = 1d / items.Count;
                foreach (var item in items)
                {
                    undoRedoManager.ChangeProperty(item, i => i.Probability, probability);
                }
            }
        }

        internal static IEnumerable<T> EquilibrateProbabilities<T>(this IEnumerable<T> enumerable)
            where T : IWithEditableProbability
        {
            var result = enumerable.ToList();
            EquilibrateProbabilities(result);
            return result;
        }

    }
}
