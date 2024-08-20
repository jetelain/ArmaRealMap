using System.IO;
using System.Threading.Tasks;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Studio.Modules.Arma3Data.Services
{
    internal interface IArma3ImageStorage
    {
        Stream CreatePng(string path);

        byte[] ReadPngBytes(string path);

        byte[] ReadPaaBytes(string path);

        bool HasToProcessPngToPaa { get; }

        Task ProcessPngToPaa(IProgressScope? progress = null);
    }
}
