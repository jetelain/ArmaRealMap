using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Studio.Modules.Arma3Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = System.Windows.Media.Color;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal class MaterialViewModel : AssetBase<TerrainMaterialUsage, TerrainMaterial>
    {
        private byte[]? _fakeSatPngImage;

        public MaterialViewModel(TerrainMaterialUsage id, TerrainMaterial terrainMaterial, AssetConfigEditorViewModel parent)
            : base(id, parent)
        {
            _colorId = Color.FromRgb(terrainMaterial.Id.R, terrainMaterial.Id.G, terrainMaterial.Id.B);
            _sameAs = parent.Materials.FirstOrDefault(m => m.ColorId == ColorId);
            _colorTexture = terrainMaterial.ColorTexture;
            _normalTexture = terrainMaterial.NormalTexture;
            _fakeSatPngImage = terrainMaterial.FakeSatPngImage;
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
                _fakeSatPngImage = null; // RESET
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
            if (_fakeSatPngImage == null)
            {
                var uri = IoC.Get<IArma3Previews>().GetTexturePreview(_colorTexture);
                if (uri != null && uri.IsFile)
                {
                    var img = Image.Load(uri.LocalPath);
                    img.Mutate(d => d.Resize(8, 8));
                    var mem = new MemoryStream();
                    img.SaveAsPng(mem);
                    _fakeSatPngImage = mem.ToArray();
                }
            }
            return new TerrainMaterial(_normalTexture, _colorTexture, new Rgb24(_colorId.R, _colorId.G, _colorId.B), _fakeSatPngImage);
        }

        public override void Equilibrate()
        {

        }
    }
}