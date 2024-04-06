using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Data
{
    internal static class BuiltinObjectsList
    {
        internal static List<string> Read(string obj)
        {
            var paths = new List<string>();
            using (var reader = new StreamReader(typeof(AssetBrowserViewModel).Assembly.GetManifestResourceStream("GameRealisticMap.Studio.Modules.AssetBrowser.Data." + obj + ".txt")!))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        paths.Add(line);
                    }
                }
            }

            return paths;
        }

        internal static List<string[]> ReadCsv(string obj)
        {
            var paths = new List<string[]>();
            using (var reader = new StreamReader(typeof(AssetBrowserViewModel).Assembly.GetManifestResourceStream("GameRealisticMap.Studio.Modules.AssetBrowser.Data." + obj + ".csv")!))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        paths.Add(line.Split(';'));
                    }
                }
            }
            return paths;
        }
    }
}
