using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using Caliburn.Micro;
using GameRealisticMap.Arma3;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Studio.Modules.Arma3Data;
using Gemini.Framework;
using Microsoft.Win32;

namespace GameRealisticMap.Studio.Modules.CompositionTool.ViewModels
{
    internal class CompositionImporter
    {
        private readonly IModelImporterTarget target;
        private readonly Arma3DataModule arma3DataModule;

        public CompositionImporter(IModelImporterTarget target)
            : this(target, IoC.Get<Arma3DataModule>(), IoC.Get<IWindowManager>())
        {

        }

        public CompositionImporter(IModelImporterTarget target, Arma3DataModule arma3DataModule, IWindowManager windowManager)
        {
            this.arma3DataModule = arma3DataModule;
            this.target = target;
            AddSingle = new RelayCommand(_ => AddSingleHandler());
            AddComposition = new AsyncCommand(() => windowManager.ShowDialogAsync(new CompositionSelectorViewModel(target))); // TODO
        }

        private void AddSingleHandler()
        {
            // TODO: create an internal library + browser
            var dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = "P3D|*.p3d";
            dialog.InitialDirectory = Directory.Exists("P:") ? "P:\\" : Arma3ToolsHelper.GetProjectDrivePath();
            if (dialog.ShowDialog() == true)
            {
                FromFiles(dialog.FileNames);
            }
        }

        public ICommand AddSingle { get; }

        public ICommand AddComposition { get; }

        public void FromFiles(IEnumerable<string> paths)
        {
            FromModels(arma3DataModule.Import(paths));
        }

        public void FromModels(IEnumerable<ModelInfo> models)
        {
            foreach (var model in models)
            {
                var detected = ObjectPlacementDetectedInfos.CreateFromModel(model, arma3DataModule.Library);
                if (detected != null)
                {
                    target.AddComposition(Composition.CreateSingleFrom(model), detected);
                }
            }
        }
    }
}
