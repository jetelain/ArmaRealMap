using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BIS.Core.Streams;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Edit;
using GameRealisticMap.Arma3.GameEngine.Roads;
using GameRealisticMap.Studio.Modules.Reporting;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    internal class NewPboPrefixViewModel : WindowBase
    {
        private readonly Arma3WorldEditorViewModel worldVM;

        public string FilePath { get; }

        public string InitialFilePath { get; }

        public string OldPboPrefix { get; }

        public string OldWorldName { get; }

        public string NewPboPrefix { get; }

        public string NewWorldName { get; }

        public bool HasValidPboPrefix { get; }

        public NewPboPrefixViewModel(Arma3WorldEditorViewModel worldVM, string initialFilePath, string filePath)
        {
            this.worldVM = worldVM;
            if (worldVM.ConfigFile == null)
            {
                throw new ArgumentException("ConfigFile is required"); ;
            }
            InitialFilePath = initialFilePath;
            FilePath = filePath;
            OldPboPrefix = worldVM.ConfigFile.PboPrefix;
            OldWorldName = worldVM.ConfigFile.WorldName;
            var targetDirectoryName = Path.GetDirectoryName(filePath)!;
            NewPboPrefix = worldVM.ProjectDrive.GetGamePath(Path.GetDirectoryName(filePath)!);
            NewWorldName = Path.GetFileNameWithoutExtension(filePath);
            HasValidPboPrefix = !string.Equals(targetDirectoryName, NewPboPrefix, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(OldPboPrefix,NewPboPrefix,StringComparison.OrdinalIgnoreCase);
        }

        public Task Cancel() => TryCloseAsync(false);

        public async Task SaveWrpOnly()
        {
            StreamHelper.Write(worldVM.World, FilePath);
            await TryCloseAsync(true);
        }

        public async Task SaveFullCopy()
        {
            if (worldVM.Roads == null)
            {
                worldVM.LoadRoads();
            }
            var task = IoC.Get<IProgressTool>().StartTask("Save");
            try
            {
                var worker = new WrpRenameWorker(task.Scope, worldVM.ProjectDrive, OldPboPrefix, NewPboPrefix);
                worldVM.ConfigFile = await worker.UpdateConfig(worldVM.ConfigFile!, NewWorldName);
                await worker.RenameAndCopyMaterials(worldVM.World!);
                await worker.CopyReferencedFiles();

                StreamHelper.Write(worldVM.World, FilePath);

                if (worldVM.Roads != null && !string.IsNullOrEmpty(worldVM.ConfigFile.Roads))
                {
                    worldVM.ProjectDrive.CreateDirectory(worldVM.ConfigFile.Roads);
                    new RoadsSerializer(worldVM.ProjectDrive).Serialize(worldVM.ConfigFile.Roads, worldVM.Roads.Roads.Where(r => !r.IsRemoved), worldVM.Roads.RoadTypeInfos);
                }
                await worldVM.Dependencies.Save(FilePath);
                worldVM.UpdateBackupsList(FilePath);
                worldVM.IsRoadsDirty = false;
            }
            catch (Exception ex)
            {
                task.Scope.Failed(ex);
                await TryCloseAsync(false);
                return;
            }
            finally
            {
                task.Done();
            }
            await TryCloseAsync(true);
        }

    }
}
