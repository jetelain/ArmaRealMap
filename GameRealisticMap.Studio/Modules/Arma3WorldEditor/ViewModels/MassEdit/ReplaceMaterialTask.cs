using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BIS.WRP;
using GameRealisticMap.Arma3;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Edit;
using GameRealisticMap.Arma3.Edit.Imagery;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Studio.Modules.Reporting;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels.MassEdit
{
    internal class ReplaceMaterialTask : IProcessTask
    {
        private readonly EditableWrp wrp;
        private readonly ProjectDrive projectDrive;
        private readonly IArma3MapConfig config;
        private readonly List<MaterialItem> materials;
        private readonly string source;
        private readonly TerrainMaterial replacement;

        public ReplaceMaterialTask(EditableWrp wrp, ProjectDrive projectDrive, IArma3MapConfig config, List<MaterialItem> materials, string source, TerrainMaterial replacement)
        {
            this.wrp = wrp;
            this.projectDrive = projectDrive;
            this.config = config;
            this.materials = materials;
            this.source = source;
            this.replacement = replacement;
        }

        public string Title => Labels.ReplaceTexture;

        public bool Prompt => false;

        public async Task Run(IProgressTaskUI ui)
        {
            var gen = new MaterialUpdateGenerator(ui, projectDrive);
            await gen.Replace(IdMapHelper.GetRvMatList(wrp), source, replacement, config);
            gen.Generate(new TerrainMaterialLibrary(materials.Select(m => m.ToDefinition()).Where(d => d != null).ToList()!), config);
            await projectDrive.ProcessImageToPaa(ui);
        }
    }
}