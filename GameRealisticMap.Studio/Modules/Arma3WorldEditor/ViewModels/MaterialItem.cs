using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BIS.WRP;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Edit.Imagery;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels.MassEdit;
using GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    internal class MaterialItem : PropertyChangedBase
    {
        private readonly Arma3WorldEditorViewModel parent;
        private string texture;
        private GdtDetailViewModel? libTexture;

        public MaterialItem(Arma3WorldEditorViewModel parent, string texture, GdtDetailViewModel? libTexture)
        {
            this.parent = parent;
            this.texture = texture;
            this.libTexture = libTexture;
        }

        public static async Task<List<MaterialItem>> Create(Arma3WorldEditorViewModel parent, EditableWrp wrp, ProjectDrive projectDrive, string pboPrefix)
        {
            var textures = await IdMapHelper.GetUsedTextureList(wrp, projectDrive);
            if (textures.Count == 0)
            {
                return new List<MaterialItem>();
            }
            return await Create(parent, pboPrefix, textures);
        }

        public static async Task<List<MaterialItem>> Create(Arma3WorldEditorViewModel parent, string pboPrefix, List<GroundDetailTexture> textures)
        {
            var items = new List<MaterialItem>();
            var lib = IoC.Get<GdtBrowserViewModel>();
            foreach (var texture in textures)
            {
                var libTexture = await lib.Resolve(texture.ColorTexture);
                if (libTexture == null)
                {
                    if (texture.ColorTexture.StartsWith(pboPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        libTexture = await lib.Resolve("{PboPrefix}" + texture.ColorTexture.Substring(pboPrefix.Length));
                    }
                    else
                    {
                        libTexture = await lib.ImportExternal(texture.ColorTexture, texture.NormalTexture);
                    }
                }
                items.Add(new MaterialItem(parent, texture.ColorTexture, libTexture));
            }
            return items;
        }

        public string ColorTexture 
        { 
            get { return texture; } 
            set { Set(ref texture, value); } 
        }

        public GdtDetailViewModel? LibraryItem
        {
            get { return libTexture; }
            set 
            { 
                if (Set(ref libTexture, value))
                { 
                    NotifyOfPropertyChange(nameof(IsFromLibrary));
                } 
            }
        }

        public bool IsFromLibrary => libTexture != null; 
        
        public Task OpenMaterial()
        {
            return libTexture?.OpenMaterial() ?? Task.CompletedTask;
        }

        public Task ReplaceMaterial()
        {
            return IoC.Get<IWindowManager>().ShowDialogAsync(new ReplaceMaterialViewModel(this, parent));
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
