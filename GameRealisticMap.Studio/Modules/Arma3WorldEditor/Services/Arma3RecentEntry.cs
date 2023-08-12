using System;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.Services
{
    internal class Arma3RecentEntry : IArma3RecentEntry
    {
        public string? WorldName { get; set; }

        public DateTime TimeStamp { get; set; }

        public string? PboPrefix { get; set; }

        public string? Description { get; set; }

        public string? ModDirectory { get; set; }

        public string? ConfigFile { get; set; }
    }
}
