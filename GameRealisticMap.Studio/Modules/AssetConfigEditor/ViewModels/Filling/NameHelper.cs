using System;
using System.Collections.Generic;
using System.Linq;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling
{
    internal static class NameHelper
    {
        internal static string LargestCommonStart(IEnumerable<string> enumerable)
        {
            var common = enumerable.OrderBy(n => n.Length).ThenBy(n => n).FirstOrDefault();
            if (common == null)
            {
                return string.Empty;
            }
            while (!enumerable.All(n => n.StartsWith(common, StringComparison.OrdinalIgnoreCase)))
            {
                common = common.Substring(0, common.Length - 1);
            }
            return common.TrimEnd('_');
        }

        internal static string LargestCommonEnd(IEnumerable<string> enumerable)
        {
            var common = enumerable.OrderBy(n => n.Length).ThenBy(n => n).FirstOrDefault();
            if (common == null)
            {
                return string.Empty;
            }
            while (!enumerable.All(n => n.EndsWith(common, StringComparison.OrdinalIgnoreCase)))
            {
                common = common.Substring(1, common.Length - 1);
            }
            return common.TrimStart('_');
        }

        internal static string LargestCommon(IEnumerable<string> enumerable)
        {
            var fromStart = LargestCommonStart(enumerable);
            var fromEnd = LargestCommonEnd(enumerable);
            if (fromEnd.Length > fromStart.Length)
            {
                return fromEnd;
            }
            return fromStart;
        }

    }
}
