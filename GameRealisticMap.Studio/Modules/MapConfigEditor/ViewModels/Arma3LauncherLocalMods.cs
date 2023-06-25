using System;
using System.Collections.Generic;

namespace GameRealisticMap.Studio.Modules.MapConfigEditor.ViewModels
{
    public class Arma3LauncherLocalMods
    {
        public List<string> autodetectionDirectories { get; set; }
        public string dateCreated { get; set; }
        public List<string> knownLocalMods { get; set; }
        public List<string> userDirectories { get; set; }
    }
}
