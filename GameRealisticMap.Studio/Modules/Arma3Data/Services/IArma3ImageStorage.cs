using System.IO;
using System.Threading.Tasks;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Studio.Modules.Arma3Data.Services
{
    /// <summary>
    /// Manages PNG/PAA image storage for ground textures (Ground Detail Texture Library).
    /// 
    /// Provides deferred PNG-to-PAA conversion (via TexConverter) so that
    /// large batches are processed as a post-save step rather than inline.
    /// </summary>
    internal interface IArma3ImageStorage
    {
        Stream CreatePng(string path);

        byte[] ReadPngBytes(string path);

        byte[] ReadPaaBytes(string path);

        bool HasToProcessPngToPaa { get; }

        Task ProcessPngToPaa(IProgressScope? progress = null);
    }
}
