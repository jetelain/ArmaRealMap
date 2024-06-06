using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using GameRealisticMap.Studio.Modules.GenericMapConfigEditor.ViewModels;
using GameRealisticMap.Studio.Modules.Main.Services;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Modules.MapConfigEditor
{
    [Export(typeof(IEditorProvider))]
    [Export("GenericMapConfigEditorProvider", typeof(IEditorProvider))]
    [Export(typeof(GenericMapConfigEditorProvider))]
    internal class GenericMapConfigEditorProvider : IEditorProvider
    {

        internal static string IconSource = $"pack://application:,,,/GameRealisticMap.Studio;component/Resources/Icons/MapExportConfig.png";

        private readonly IShell shell;
        private readonly IGrmConfigService grmConfig;

        [ImportingConstructor]
        public GenericMapConfigEditorProvider(IShell shell, IGrmConfigService grmConfig)
        {
            this.shell = shell;
            this.grmConfig = grmConfig;
        }

        public IEnumerable<EditorFileType> FileTypes
        {
            get
            {
                yield return new EditorFileType("Generic Map Configuration", ".grmm", new Uri(IconSource));
            }
        }

        public bool CanCreateNew => true;

        public bool Handles(string path)
        {
            return string.Equals(Path.GetExtension(path), ".grmm", StringComparison.OrdinalIgnoreCase);
        }

        public IDocument Create() => new GenericMapConfigEditorViewModel(shell, grmConfig);

        public async Task New(IDocument document, string name) => await ((GenericMapConfigEditorViewModel)document).New(name);

        public async Task Open(IDocument document, string path) => await ((GenericMapConfigEditorViewModel)document).Load(path);
    }
}
