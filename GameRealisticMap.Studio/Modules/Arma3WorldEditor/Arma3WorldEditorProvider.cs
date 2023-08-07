using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor
{
    [Export(typeof(IEditorProvider))]
    internal class Arma3WorldEditorProvider : IEditorProvider
    {
        private readonly IArma3DataModule arma3Data;
        private readonly IWindowManager windowManager;

        [ImportingConstructor]
        public Arma3WorldEditorProvider(IArma3DataModule arma3Data, IWindowManager windowManager)
        {
            this.arma3Data = arma3Data;
            this.windowManager = windowManager;
        }

        public IEnumerable<EditorFileType> FileTypes
        {
            get
            {
                yield return new EditorFileType(Labels.Arma3AssetsConfiguration, ".wrp", new Uri("pack://application:,,,/GameRealisticMap.Studio;component/Resources/Icons/MapFile.png"));
            }
        }

        public bool CanCreateNew => false;

        public bool Handles(string path)
        {
            return string.Equals(Path.GetExtension(path), ".wrp", StringComparison.OrdinalIgnoreCase);
        }

        public IDocument Create() => new Arma3WorldEditorViewModel(arma3Data, windowManager);

        public Task New(IDocument document, string name) => throw new NotSupportedException("You cannot create an empty world file.");

        public async Task Open(IDocument document, string path) => await ((Arma3WorldEditorViewModel)document).Load(path);
    }
}
