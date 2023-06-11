using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using GameRealisticMap.Arma3;

namespace GameRealisticMap.Studio.Modules.Arma3Data.Services
{
    [Export(typeof(IArma3ModsService))]
    internal class Arma3ModsService : IArma3ModsService
    {
        public ModInfo? GetMod(string steamId)
        {
            var wk = Arma3ToolsHelper.GetArma3WorkshopPath();
            if (!string.IsNullOrEmpty(wk))
            {
                var d = Path.Combine(wk, steamId); 
                if ( Directory.Exists(d) )
                {
                    return new ModInfo(GetModName(d), Path.Combine(d, "addons"), Path.GetFileName(d));
                }
            }
            return null;
        }

        public List<ModInfo> GetModsList()
        {
            var wk = Arma3ToolsHelper.GetArma3WorkshopPath();
            if (!string.IsNullOrEmpty(wk))
            {
                return Directory.GetDirectories(wk)
                    .Where(d => Directory.Exists(Path.Combine(d, "addons")))
                    .Select(d => new ModInfo( GetModName(d), Path.Combine(d, "addons"), Path.GetFileName(d)))
                    .ToList()
                    .OrderBy(d => d.Name)
                    .ToList();
            }
            return new List<ModInfo>();
        }

        private string GetModName(string path)
        {
            var config = Path.Combine(path, "meta.cpp");
            if (File.Exists(config))
            {
                var nameLine = File.ReadAllLines(config).FirstOrDefault(l => l.StartsWith("name ="));
                if (nameLine != null)
                {
                    return nameLine.Substring(6).Trim(' ', '"', ';');
                }
            }
            config = Path.Combine(path, "mod.cpp");
            if (File.Exists(config))
            {
                var nameLine = File.ReadAllLines(config).FirstOrDefault(l => l.StartsWith("name ="));
                if (nameLine != null)
                {
                    return nameLine.Substring(6).Trim(' ', '"', ';');
                }
            }
            return Path.GetFileName(path);
        }
    }
}
