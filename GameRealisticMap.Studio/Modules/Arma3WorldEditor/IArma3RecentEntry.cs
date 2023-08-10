using System;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor
{
    public interface IArma3RecentEntry
    {
        string? WorldName { get; }

        DateTime TimeStamp { get; }

        string? PboPrefix { get; }

        string? Description { get; }

        string? ModDirectory { get; }

        string? ConfigFile { get; }
    }
}
