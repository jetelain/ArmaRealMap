using System.Linq;
using System.Windows.Media;
using GameRealisticMap.Arma3.Assets;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal class MaterialViewModel : AssetBase<TerrainMaterialUsage, TerrainMaterial>
    {
        public MaterialViewModel(TerrainMaterialUsage id, TerrainMaterial terrainMaterial, AssetConfigEditorViewModel parent)
            : base(id, parent)
        {
            ColorId = Color.FromRgb(terrainMaterial.Id.R, terrainMaterial.Id.G, terrainMaterial.Id.B);

            _sameAs = parent.Materials.FirstOrDefault(m => m.ColorId == ColorId);

            ColorTexture = terrainMaterial.ColorTexture;

            NormalTexture = terrainMaterial.NormalTexture;
        }

        MaterialViewModel? _sameAs;
        public MaterialViewModel? SameAs 
        { 
            get { return _sameAs; }
            set 
            { 
                if (value != null)
                {
                    ColorId = value.ColorId;
                    ColorTexture = value.ColorTexture;
                    NormalTexture = value.NormalTexture;
                    NotifyOfPropertyChange(nameof(ColorId));
                    NotifyOfPropertyChange(nameof(ColorTexture));
                    NotifyOfPropertyChange(nameof(NormalTexture));
                }
                else if (value == null && _sameAs != null)
                {
                    // TODO: Find an unique color
                    NotifyOfPropertyChange(nameof(ColorId));
                }
                _sameAs = value;
                NotifyOfPropertyChange();
            } 
        }

        public Color ColorId { get; set; }

        public string ColorTexture { get; set; }

        public string NormalTexture { get; set; }

        public override TerrainMaterial ToDefinition()
        {
            return new TerrainMaterial(NormalTexture, ColorTexture, new Rgb24(ColorId.R, ColorId.G, ColorId.B), null /*TODO !*/);
        }
    }
}