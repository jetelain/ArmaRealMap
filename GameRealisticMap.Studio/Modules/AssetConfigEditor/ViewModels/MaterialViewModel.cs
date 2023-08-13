using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Studio.Modules.Arma3Data;
using Microsoft.Win32;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = System.Windows.Media.Color;
using Image = SixLabors.ImageSharp.Image;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal class MaterialViewModel : AssetIdBase<TerrainMaterialUsage, TerrainMaterial>
    {
        private byte[]? _fakeSatPngImage;
        private BitmapFrame? _fakeSatPreview;

        public MaterialViewModel(TerrainMaterialUsage id, TerrainMaterial terrainMaterial, AssetConfigEditorViewModel parent)
            : base(id, parent)
        {
            _colorId = Color.FromRgb(terrainMaterial.Id.R, terrainMaterial.Id.G, terrainMaterial.Id.B);
            _sameAs = parent.Materials.FirstOrDefault(m => m.ColorId == ColorId);
            _colorTexture = terrainMaterial.ColorTexture;
            _normalTexture = terrainMaterial.NormalTexture;
            FakeSatPngImage = terrainMaterial.FakeSatPngImage;
        }

        public override string Icon => $"pack://application:,,,/GameRealisticMap.Studio;component/Resources/Icons/Generic.png";

        public List<string> Others =>
            ParentEditor.Materials.Where(m => m != this && m.SameAs == null)
            .Select(m => m.PageTitle)
            .Concat(new[] { string.Empty })
            .ToList();

        MaterialViewModel? _sameAs;

        public MaterialViewModel? SameAs
        {
            get { return _sameAs; }
            set
            {
                // TODO: Undo/redo as it cannot be handled only on property value
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
            get { return SameAs?.PageTitle ?? string.Empty; }
            set { SameAs = ParentEditor.Materials.FirstOrDefault(m => m != this && m.PageTitle == value); }
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
                GenerateFakeSatPngImage();
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

        private byte[]? FakeSatPngImage
        {
            get { return _fakeSatPngImage; }
            set
            {
                _fakeSatPngImage = value;
                if (_fakeSatPngImage != null)
                {
                    using (MemoryStream stream = new MemoryStream(_fakeSatPngImage))
                    {
                        _fakeSatPreview = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                }
                else
                {
                    _fakeSatPreview = null;
                }
                NotifyOfPropertyChange(nameof(FakeSatPreview));
            }
        }

        public BitmapSource? FakeSatPreview => _fakeSatPreview;

        public override TerrainMaterial ToDefinition()
        {
            return new TerrainMaterial(_normalTexture, _colorTexture, new Rgb24(_colorId.R, _colorId.G, _colorId.B), FakeSatPngImage);
        }

        private void GenerateFakeSatPngImage()
        {
            var uri = IoC.Get<IArma3Previews>().GetTexturePreview(_colorTexture);
            if (uri != null && uri.IsFile)
            {
                using var img = Image.Load(uri.LocalPath);
                img.Mutate(d =>
                {
                    d.Resize(1, 1);
                    d.Resize(8, 8);
                });
                SetFakeSatImage(img);
            }
        }

        public Task RegenerateFakeSatImage()
        {
            GenerateFakeSatPngImage();
            return Task.CompletedTask;
        }

        public Task SelectFakeSatImage()
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "PNG Image|*.png";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            if (dialog.ShowDialog() == true)
            {
                using var img = Image.Load(dialog.FileName);
                SetFakeSatImage(img);
            }
            return Task.CompletedTask;
        }

        private void SetFakeSatImage(Image img)
        {
            var mem = new MemoryStream();
            img.SaveAsPng(mem);
            FakeSatPngImage = mem.ToArray();
        }

        public override void Equilibrate()
        {

        }

        public override void AddComposition(Composition model, ObjectPlacementDetectedInfos detected)
        {

        }

        public override IEnumerable<string> GetModels()
        {
            return Enumerable.Empty<string>();
        }
    }
}