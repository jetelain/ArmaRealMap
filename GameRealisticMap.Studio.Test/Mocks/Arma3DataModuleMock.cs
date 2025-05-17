using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Studio.Modules.Arma3Data;

namespace GameRealisticMap.Studio.Test.Mocks
{
    internal class Arma3DataModuleMock : IArma3DataModule
    {
        public ModelInfoLibrary Library => throw new NotImplementedException();

        public ProjectDrive ProjectDrive => throw new NotImplementedException();

        public ModelPreviewHelper ModelPreviewHelper => throw new NotImplementedException();

        public IEnumerable<string> ActiveMods { get; set; } = new string[0];

        public bool UsePboProject { get; set; }
        public string ProjectDriveBasePath { get => throw new NotImplementedException(); }

        public event EventHandler<EventArgs>? Reloaded;

        public Task ChangeActiveMods(IEnumerable<string> mods)
        {
            ActiveMods = mods;
            return Task.CompletedTask;
        }

        public IPboCompilerFactory CreatePboCompilerFactory()
        {
            throw new NotImplementedException();
        }

        public Task Reload()
        {
            Reloaded?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        public Task SaveLibraryCache()
        {
            return Task.CompletedTask;
        }

        public Task SetProjectDriveBasePath(string projectDriveBasePath)
        {
            throw new NotImplementedException();
        }
    }
}