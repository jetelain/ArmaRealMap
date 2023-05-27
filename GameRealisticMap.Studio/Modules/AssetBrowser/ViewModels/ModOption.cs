using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels
{
    internal class ModOption
    {
        public ModOption(string name, string modId)
        {
            Name = name;
            ModId = modId;
        }

        public string Name { get; }

        public string ModId { get; }
    }
}
