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
    [Export("Arma3WorldEditorProvider", typeof(IEditorProvider))]
    internal class Arma3WorldEditorProvider : IEditorProvider
    {
        private readonly IArma3DataModule arma3Data;
        private readonly IWindowManager windowManager;
        private readonly IArma3RecentHistory history;

        [ImportingConstructor]
        public Arma3WorldEditorProvider(IArma3DataModule arma3Data, IWindowManager windowManager, IArma3RecentHistory history)
        {
            this.arma3Data = arma3Data;
            this.windowManager = windowManager;
            this.history = history;
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

        public IDocument Create() => new Arma3WorldEditorViewModel(arma3Data, windowManager, history);

        public Task New(IDocument document, string name) => throw new NotSupportedException("You cannot create an empty world file.");

        public async Task Open(IDocument document, string path) => await ((Arma3WorldEditorViewModel)document).Load(path);
    }
}
