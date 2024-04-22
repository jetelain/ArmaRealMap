using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BIS.WRP;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Edit.Imagery;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    internal class MaterialItem : PropertyChangedBase
    {
        private string texture;
        private GdtDetailViewModel? libTexture;

        public MaterialItem(string texture, GdtDetailViewModel? libTexture)
        {
            this.texture = texture;
            this.libTexture = libTexture;
        }

        public static async Task<List<MaterialItem>> Create(EditableWrp wrp, ProjectDrive projectDrive, string pboPrefix)
        {
            var items = new List<MaterialItem>();
            var textures = await IdMapHelper.GetUsedTextureList(wrp, projectDrive);
            if (textures.Count > 0)
            {
                foreach (var texture in textures)
                {
                    var libTexture = await IoC.Get<GdtBrowserViewModel>().Resolve(texture);
                    if (libTexture == null && texture.StartsWith(pboPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        libTexture = await IoC.Get<GdtBrowserViewModel>().Resolve("{PboPrefix}" + texture.Substring(pboPrefix.Length));
                    }
                    items.Add(new MaterialItem(texture, libTexture));
                }
            }
            return items;
        }

        public string ColorTexture 
        { 
            get { return texture; } 
            private set { Set(ref texture, value); } 
        }

        public bool IsFromLibrary => libTexture != null; 
        
        public Task OpenMaterial()
        {
            return libTexture?.OpenMaterial() ?? Task.CompletedTask;
        }

        internal TerrainMaterialDefinition? ToDefinition()
        {
            if (libTexture != null)
            {
                return new TerrainMaterialDefinition(libTexture.ToMaterial(), new TerrainMaterialUsage[0], libTexture.ToSurfaceConfig(), libTexture.ToData());
            }
            return null;
        }
    }
}
