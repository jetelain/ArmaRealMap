using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels;
using GameRealisticMap.Studio.Modules.CompositionTool;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor
{
    [Export(typeof(IEditorProvider))]
    [Export(typeof(AssetConfigEditorProvider))]
    internal class AssetConfigEditorProvider : IEditorProvider
    {
        private readonly IArma3DataModule _arma3Data;
        private readonly IShell _shell;
        private readonly ICompositionTool _composition;

        [ImportingConstructor]
        public AssetConfigEditorProvider(IArma3DataModule arma3Data, IShell shell, ICompositionTool composition)
        {
            _arma3Data = arma3Data;
            _shell = shell;
            _composition = composition;
        }

        public IEnumerable<EditorFileType> FileTypes
        {
            get
            {
                yield return new EditorFileType(Labels.Arma3AssetsConfiguration, ".grma3a", new Uri("pack://application:,,,/GameRealisticMap.Studio;component/Resources/Icons/AssetConfig.png"));
            }
        }

        public bool CanCreateNew => true;

        public bool Handles(string path)
        {
            return string.Equals(Path.GetExtension(path), ".grma3a", StringComparison.OrdinalIgnoreCase);
        }

        public IDocument Create() => new AssetConfigEditorViewModel(_arma3Data, _shell, _composition);

        public async Task New(IDocument document, string name) => await ((AssetConfigEditorViewModel)document).New(name);

        public async Task Open(IDocument document, string path) => await ((AssetConfigEditorViewModel)document).Load(path);
    }
}
