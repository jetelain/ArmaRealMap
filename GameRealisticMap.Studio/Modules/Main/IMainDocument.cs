using System.IO;
using System.Threading.Tasks;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.Main
{
    internal interface IMainDocument : IPersistedDocument
    {
        Task SaveTo(Stream stream);
    }
}
