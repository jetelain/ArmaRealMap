using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Studio.Modules.MapConfigEditor.ViewModels;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Modules.MapConfigEditor
{
    [Export(typeof(IEditorProvider))]
    [Export(typeof(MapConfigEditorProvider))]
    internal class MapConfigEditorProvider : IEditorProvider
    {
        public IEnumerable<EditorFileType> FileTypes
        {
            get
            {
                yield return new EditorFileType("Arma 3 Map Config", ".grma3m", new Uri("pack://application:,,,/GameRealisticMap.Studio;component/Resources/Icons/MapConfig.png"));
            }
        }

        public bool CanCreateNew => true;

        public bool Handles(string path)
        {
            return string.Equals(Path.GetExtension(path), ".grma3m", StringComparison.OrdinalIgnoreCase);
        }

        public IDocument Create() => new MapConfigEditorViewModel();

        public async Task New(IDocument document, string name) => await ((MapConfigEditorViewModel)document).New(name);

        public async Task Open(IDocument document, string path) => await ((MapConfigEditorViewModel)document).Load(path);
    }
}
