using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine.Materials;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.AssetBrowser.Services;
using GameRealisticMap.Studio.UndoRedo;
using Gemini.Framework;
using Gemini.Framework.Services;
using Microsoft.Win32;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Color = System.Windows.Media.Color;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels
{
    internal class GdtDetailViewModel : Document
    {
        private GdtDetailViewModel(GdtBrowserViewModel parent, SurfaceConfig config)
        {
            ParentEditor = parent;

            Name = config.Name;
            Files = config.Files;
            _aceCanDig = config.AceCanDig;
            _soundEnviron = config.SoundEnviron;
            _soundHit = config.SoundHit;
            _rough = config.Rough;
            _maxSpeedCoef = config.MaxSpeedCoef;
            _dust = config.Dust;
            _lucidity = config.Lucidity;
            _grassCover = config.GrassCover;
            _impact = config.Impact;
            _surfaceFriction = config.SurfaceFriction;
            _maxClutterColoringCoef = config.MaxClutterColoringCoef;

            ClutterList = new UndoableObservableCollection<GdtClutterViewModel>(config.Character.Select(c => new GdtClutterViewModel(c)));
        }

        public GdtDetailViewModel(GdtBrowserViewModel parent, GdtCatalogItem item)
            : this(parent, item.Config)
        {
            _colorTexture = item.Material.ColorTexture;
            _normalTexture = item.Material.NormalTexture;
            _colorId = Color.FromRgb(item.Material.Id.R, item.Material.Id.G, item.Material.Id.B);
            _fakeSatPngImage = item.Material.FakeSatPngImage;
        }

        public GdtDetailViewModel(GdtBrowserViewModel parent, GdtImporterItem item)
            : this(parent, item.Config)
        {
            _colorTexture = item.ColorTexture;
            _normalTexture = item.NormalTexture;
            var avgColor = GenerateFakeSatPngImage();
            _colorId = parent.AllocateUniqueColor(avgColor);
            DisplayName = Path.GetFileNameWithoutExtension(ColorTexture);
            IsEditable = false;
        }

        public GdtBrowserViewModel ParentEditor { get; }

        private Color _colorId;
        public Color ColorId
        {
            get { return _colorId; }
            set { Set(ref _colorId, value); }
        }

        private string _colorTexture = string.Empty;
        public string ColorTexture
        {
            get { return _colorTexture; }
            set
            {
                _colorTexture = value;
                GenerateFakeSatPngImage();
                NotifyOfPropertyChange();
            }
        }

        private string _normalTexture = string.Empty;

        public string NormalTexture
        {
            get { return _normalTexture; }
            set { Set(ref _normalTexture, value); }
        }

        private byte[]? _fakeSatPngImage;
        private byte[]? FakeSatPngImage
        {
            get { return _fakeSatPngImage; }
            set
            {
                _fakeSatPngImage = value;
                if (_fakeSatPngImage != null)
                {
                    using (var stream = new MemoryStream(_fakeSatPngImage))
                    {
                        FakeSatPreview = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                }
                else
                {
                    FakeSatPreview = null;
                }
                NotifyOfPropertyChange(nameof(FakeSatPreview));
            }
        }

        public BitmapSource? FakeSatPreview { get; private set; }

        public bool IsEditable { get; }

        public UndoableObservableCollection<GdtClutterViewModel> ClutterList { get; }

        public string? Name { get; }

        public string Files { get; }

        private bool _aceCanDig;
        public bool AceCanDig { get { return _aceCanDig; } set { Set(ref _aceCanDig, value); } }

        private string _soundEnviron;
        public string SoundEnviron { get { return _soundEnviron; } set { Set(ref _soundEnviron, value); } }

        private string _soundHit;
        public string SoundHit { get { return _soundHit; } set { Set(ref _soundHit, value); } }

        private double _rough;
        public double Rough { get { return _rough; } set { Set(ref _rough, value); } }

        private double _maxSpeedCoef;
        public double MaxSpeedCoef { get { return _maxSpeedCoef; } set { Set(ref _maxSpeedCoef, value); } }

        private double _dust;
        public double Dust { get { return _dust; } set { Set(ref _dust, value); } }

        private double _lucidity;
        public double Lucidity { get { return _lucidity; } set { Set(ref _lucidity, value); } }

        private double _grassCover;
        public double GrassCover { get { return _grassCover; } set { Set(ref _grassCover, value); } }

        private string _impact;
        public string Impact { get { return _impact; } set { Set(ref _impact, value); } }

        private double _surfaceFriction;
        public double SurfaceFriction { get { return _surfaceFriction; } set { Set(ref _surfaceFriction, value); } }

        private double _maxClutterColoringCoef;
        public double MaxClutterColoringCoef { get { return _maxClutterColoringCoef; } set { Set(ref _maxClutterColoringCoef, value); } }

        private Color? GenerateFakeSatPngImage()
        {
            var uri = IoC.Get<IArma3Previews>().GetTexturePreview(_colorTexture);
            if (uri != null && uri.IsFile)
            {
                using var img = Image.Load<Rgb24>(uri.LocalPath);
                img.Mutate(d =>
                {
                    d.Resize(1, 1);
                    d.Resize(8, 8);
                });
                SetFakeSatImage(img);
                var px = img[0, 0];
                return Color.FromRgb(px.R, px.G, px.B);
            }
            return null;
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

        public GdtCatalogItem ToDefinition()
        {
            var config = new SurfaceConfig(
                Name,
                AceCanDig,
                Files,
                SoundEnviron,
                SoundHit,
                Rough,
                MaxSpeedCoef,
                Dust,
                Lucidity,
                GrassCover,
                Impact,
                SurfaceFriction,
                MaxClutterColoringCoef,
                ClutterList.Select(c => c.ToDefinition()).ToList());

            var material = new TerrainMaterial(NormalTexture, ColorTexture, new Rgb24(_colorId.R, _colorId.G, _colorId.B), FakeSatPngImage);

            return new GdtCatalogItem(material, config);
        }

        internal void RefreshConfig(GdtImporterItem item)
        {
            

            throw new NotImplementedException();
        }

        public Task OpenMaterial()
        {
            return IoC.Get<IShell>().OpenDocumentAsync(this);
        }
    }
}
