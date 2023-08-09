using System;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor
{
    public interface IArma3WorldEntry
    {
        string? WorldName { get; }

        DateTime TimeStamp { get; }

        string? PboPrefix { get; }

        string? Description { get; }

        string? ModDirectory { get; }
    }
}
