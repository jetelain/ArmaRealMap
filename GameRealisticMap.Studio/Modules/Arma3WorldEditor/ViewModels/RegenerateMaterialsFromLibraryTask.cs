﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameRealisticMap.Arma3;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Edit;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Studio.Modules.Reporting;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    internal class RegenerateMaterialsFromLibraryTask : IProcessTask
    {
        private readonly ProjectDrive projectDrive;
        private readonly IArma3MapConfig config;
        private readonly List<MaterialItem> materials;

        public RegenerateMaterialsFromLibraryTask(ProjectDrive projectDrive, IArma3MapConfig config, List<MaterialItem> materials)
        {
            this.projectDrive = projectDrive;
            this.config = config;
            this.materials = materials;
        }

        public string Title => GameRealisticMap.Studio.Labels.ReGenerateFromLibrary;

        public bool Prompt => false;

        public async Task Run(IProgressTaskUI ui)
        {
            var gen = new MaterialUpdateGenerator(ui.Scope, projectDrive);
            gen.Generate(new TerrainMaterialLibrary(materials.Select(m => m.ToDefinition()).Where(d => d != null).ToList()!), config);
            await projectDrive.ProcessImageToPaa(ui.Scope);
        }
    }
}