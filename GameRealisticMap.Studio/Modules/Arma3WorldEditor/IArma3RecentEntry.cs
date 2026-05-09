using System;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor
{
    /// <summary>
    /// A recently-opened Arma 3 world entry stored in the user's history.
    /// Provides quick-open shortcuts in the Studio start page and file menus.
    /// </summary>
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
