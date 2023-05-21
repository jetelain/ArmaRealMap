using System.Collections.Generic;
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
            _colorId = Color.FromRgb(terrainMaterial.Id.R, terrainMaterial.Id.G, terrainMaterial.Id.B);
            _sameAs = parent.Materials.FirstOrDefault(m => m.ColorId == ColorId);
            _colorTexture = terrainMaterial.ColorTexture;
            _normalTexture = terrainMaterial.NormalTexture;
        }

        public List<string> Others =>
            ParentEditor.Materials.Where(m => m != this && m.SameAs == null)
            .Select(m => m.IdText)
            .Concat(new[] { string.Empty })
            .ToList();

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
                    foreach (var sameAsSelf in ParentEditor.Materials.Where(m => m != this && m.SameAs == this))
                    {
                        sameAsSelf.SameAs = value;
                    }
                }
                else if (value == null && _sameAs != null)
                {
                    // TODO: Find an unique color
                }
                _sameAs = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(SameAsText));
                NotifyOfPropertyChange(nameof(IsNotSameAs));
            }
        }

        public bool IsNotSameAs
        {
            get { return _sameAs == null; }
        }

        public string SameAsText
        {
            get { return SameAs?.IdText ?? string.Empty; }
            set { SameAs = ParentEditor.Materials.FirstOrDefault(m => m != this && m.IdText == value); }
        }

        private Color _colorId;
        public Color ColorId
        {
            get { return _colorId; }
            set
            {
                _colorId = value;
                NotifyOfPropertyChange();
                foreach (var sameAsSelf in ParentEditor.Materials.Where(m => m != this && m.SameAs == this))
                {
                    sameAsSelf.ColorId = value;
                }
            }
        }

        private string _colorTexture;
        public string ColorTexture
        {
            get { return _colorTexture; }
            set
            {
                _colorTexture = value;
                NotifyOfPropertyChange();
                foreach (var sameAsSelf in ParentEditor.Materials.Where(m => m != this && m.SameAs == this))
                {
                    sameAsSelf.ColorTexture = value;
                }
            }
        }

        private string _normalTexture;
        public string NormalTexture
        {
            get { return _normalTexture; }
            set
            {
                _normalTexture = value;
                NotifyOfPropertyChange();
                foreach (var sameAsSelf in ParentEditor.Materials.Where(m => m != this && m.SameAs == this))
                {
                    sameAsSelf.NormalTexture = value;
                }
            }
        }

        public override TerrainMaterial ToDefinition()
        {
            return new TerrainMaterial(NormalTexture, _colorTexture, new Rgb24(_colorId.R, _colorId.G, _colorId.B), null /*TODO : FakeSatPngImage*/);
        }

        public override void Equilibrate()
        {

        }
    }
}