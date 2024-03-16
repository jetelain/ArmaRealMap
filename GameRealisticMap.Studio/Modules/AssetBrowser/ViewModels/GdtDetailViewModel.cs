using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.AssetBrowser.Services;
using Gemini.Framework;
using Gemini.Framework.Services;
using Microsoft.Win32;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = System.Windows.Media.Color;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels
{
    internal class GdtDetailViewModel : Document
    {

        public GdtDetailViewModel(GdtBrowserViewModel parent, GdtCatalogItem item) 
        {
            ParentEditor = parent;
        }

        public GdtDetailViewModel(GdtBrowserViewModel parent, GdtImporterItem item)
        {
            ParentEditor = parent;
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
            set
            {
                _colorId = value;
                NotifyOfPropertyChange();
            }
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
            set
            {
                _normalTexture = value;
                NotifyOfPropertyChange();
            }
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
            return new GdtCatalogItem();
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
