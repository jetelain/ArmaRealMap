using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using GameRealisticMap.Studio.Modules.GenericMapConfigEditor.ViewModels;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Modules.MapConfigEditor
{
    [Export(typeof(IEditorProvider))]
    [Export("GenericMapConfigEditorProvider", typeof(IEditorProvider))]
    [Export(typeof(GenericMapConfigEditorProvider))]
    internal class GenericMapConfigEditorProvider : IEditorProvider
    {

        internal static string IconSource = $"pack://application:,,,/GameRealisticMap.Studio;component/Resources/Icons/MapConfig.png";

        private readonly IShell _shell;

        [ImportingConstructor]
        public GenericMapConfigEditorProvider(IShell shell)
        {
            _shell = shell;
        }

        public IEnumerable<EditorFileType> FileTypes
        {
            get
            {
                yield return new EditorFileType("Generic Map Config", ".grmap", new Uri(IconSource));
            }
        }

        public bool CanCreateNew => true;

        public bool Handles(string path)
        {
            return string.Equals(Path.GetExtension(path), ".grmap", StringComparison.OrdinalIgnoreCase);
        }

        public IDocument Create() => new GenericMapConfigEditorViewModel(_shell);

        public async Task New(IDocument document, string name) => await ((GenericMapConfigEditorViewModel)document).New(name);

        public async Task Open(IDocument document, string path) => await ((GenericMapConfigEditorViewModel)document).Load(path);
    }
}
