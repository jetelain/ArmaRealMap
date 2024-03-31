using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Studio.Modules.Arma3Data.Services
{
    internal interface IArma3ImageStorage
    {
        Stream CreatePng(string path);

        byte[] ReadPngBytes(string path);

        byte[] ReadPaaBytes(string path);

        bool HasToProcessPngToPaa { get; }

        Task ProcessPngToPaa(IProgressSystem? progress = null);
    }
}
