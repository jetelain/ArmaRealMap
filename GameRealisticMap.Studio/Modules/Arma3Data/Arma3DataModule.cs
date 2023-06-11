using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Arma3.TerrainBuilder;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.Arma3Data
{
    [Export(typeof(IArma3DataModule))]
    [Export(typeof(IModule))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class Arma3DataModule : ModuleBase, IArma3DataModule
    {
        public static Uri NoPreview = new Uri("pack://application:,,,/GameRealisticMap.Studio;component/Resources/noimage.png");

        public event EventHandler<EventArgs>? Reloaded;

        public ModelInfoLibrary Library { get; private set; }

        public ProjectDrive ProjectDrive { get; private set; }

        public ModelPreviewHelper ModelPreviewHelper { get; private set; }

        public string PreviewCachePath { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GameRealisticMap",
            "Arma3",
            "Previews");

        public string LibraryCachePath { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GameRealisticMap",
            "Arma3",
            "modelinfo.json");

        public WorkspaceSettings? Settings { get; set; }

        public IEnumerable<string> ActiveMods  => (ProjectDrive.SecondarySource as PboFileSystem)?.ModsPaths ?? new List<string>();

        public override void Initialize()
        {
            Initialize(Settings ?? WorkspaceSettings.Load().Result);
        }

        private void Initialize(WorkspaceSettings settings)
        {
            ProjectDrive = settings.CreateProjectDrive();

            Library = new ModelInfoLibrary(ProjectDrive);

            ModelPreviewHelper = new ModelPreviewHelper(Library);
        }

        public override async Task PostInitializeAsync()
        {
            await LoadLibraryFromCache().ConfigureAwait(false);
        }

        private async Task LoadLibraryFromCache()
        {
            if (File.Exists(LibraryCachePath))
            {
                await Library.LoadFrom(LibraryCachePath).ConfigureAwait(false);
            }
        }

        public async Task SaveLibraryCache()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LibraryCachePath)!);
            await Library.SaveTo(LibraryCachePath).ConfigureAwait(false);
        }

        public async Task Reload()
        {
            Initialize();

            await LoadLibraryFromCache().ConfigureAwait(false);

            Reloaded?.Invoke(this, EventArgs.Empty);
        }

        public async Task ChangeActiveMods(IEnumerable<string> mods)
        {
            var newMods = mods.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var currentMods = ActiveMods.ToHashSet(StringComparer.OrdinalIgnoreCase);
            if ( newMods.Count != currentMods.Count || newMods.Intersect(currentMods).Count() != newMods.Count)
            {
                var settings = Settings ?? new WorkspaceSettings();
                settings.ModsPaths = mods.ToList();
                await settings.Save().ConfigureAwait(false);

                Settings = settings;
                await Reload().ConfigureAwait(false);
            }
        }
    }
}
