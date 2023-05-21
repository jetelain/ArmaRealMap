﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels;
using GameRealisticMap.Studio.Modules.CompositionTool;
using GameRealisticMap.Studio.Modules.MapConfigEditor.ViewModels;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor
{
    [Export(typeof(IEditorProvider))]
    internal class AssetConfigEditorProvider : IEditorProvider
    {
        private readonly Arma3DataModule _arma3Data;
        private readonly IShell _shell;
        private readonly ICompositionTool _composition;

        [ImportingConstructor]
        public AssetConfigEditorProvider(Arma3DataModule arma3Data, IShell shell, ICompositionTool composition)
        {
            _arma3Data = arma3Data;
            _shell = shell;
            _composition = composition;
        }

        public IEnumerable<EditorFileType> FileTypes
        {
            get
            {
                yield return new EditorFileType("Arma 3 Assets Configuration", ".grma3a");
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