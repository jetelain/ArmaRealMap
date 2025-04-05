using System.Threading.Tasks;
using BIS.WRP;
using GameRealisticMap.Arma3.Edit.Imagery;
using GameRealisticMap.Arma3.Edit.Imagery.Generic;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Studio.Modules.Reporting;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels.Import
{
    internal class RestoreImageryTask : IProcessTask
    {
        private readonly Arma3WorldEditorViewModel editor;
        private readonly EditableWrp initialWorld;
        private readonly GenericImageryInfos imagery;
        private readonly IProjectDrive projectDrive;

        public RestoreImageryTask(Arma3WorldEditorViewModel editor, EditableWrp initialWorld, GenericImageryInfos imagery, IProjectDrive projectDrive)
        {
            this.editor = editor;
            this.initialWorld = initialWorld;
            this.imagery = imagery;
            this.projectDrive = projectDrive;
        }

        public string Title => Labels.RestoreImagery;

        public bool Prompt => true;

        public async Task Run(IProgressTaskUI ui)
        {
            var initialMaterials = await editor.GetExportMaterialLibrary();
            var imgCompiler = new ImageryCompiler(initialMaterials, ui.Scope, projectDrive);
            var wrpCompiler = new WrpCompiler(ui.Scope, projectDrive);
            var tiler = await imgCompiler.Compile(imagery, new GenericImagerySource(imagery, projectDrive, initialMaterials));
            var newWorld = wrpCompiler.CreateWorldWithoutObjects(initialWorld.ToElevationGrid(), tiler, imagery.PboPrefix);
            newWorld.Objects = initialWorld.Objects;

            await projectDrive.ProcessImageToPaa(ui.Scope);

            editor.World = newWorld;
            editor.IsDirty = true;
            editor.GenericImagery = null;
            editor.GrmImagery = ExistingImageryInfos.TryCreate(projectDrive, imagery.PboPrefix, imagery.SizeInMeters);
            editor.Materials = await MaterialItem.Create(editor, newWorld, projectDrive, imagery.PboPrefix);
        }
    }
}
