using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using GameRealisticMap.Arma3;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine.Materials;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using GameRealisticMap.Studio.Modules.AssetBrowser.Services;
using GameRealisticMap.Studio.Toolkit;
using GameRealisticMap.Toolkit;
using Gemini.Framework;
using Microsoft.Win32;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels
{
    internal class GdtCreateViewModel : WindowBase
    {
        private readonly GdtBrowserViewModel parent;

        public CollectionViewSource LibraryItemsViewSource { get; }

        public GdtCreateViewModel(GdtBrowserViewModel parent)
        {
            this.parent = parent;

            LibraryItemsViewSource = new CollectionViewSource();
            LibraryItemsViewSource.Source = parent.AllItems;
            LibraryItemsViewSource.SortDescriptions.Add(new SortDescription(nameof(GdtDetailViewModel.DisplayName), ListSortDirection.Ascending));

            CopyConfigFrom = parent.AllItems.FirstOrDefault();

            Name = "gdt_grm_" + Base36Converter.Convert(Random.Shared.NextInt64());
        }

        public GdtDetailViewModel? Result { get; internal set; }

        public Task Cancel() => TryCloseAsync(false);

        private string _colorTexture = string.Empty;
        public string ColorTexture
        {
            get { return _colorTexture; }
            set { Set(ref _colorTexture, value); }
        }

        private string _normalTexture = string.Empty;
        public string NormalTexture
        {
            get { return _normalTexture; }
            set { Set(ref _normalTexture, value); }
        }

        public GdtDetailViewModel? CopyConfigFrom { get; set; }

        public Task ImportExisting()
        {
            if ( string.IsNullOrEmpty(ColorTexture))
            {
                TextureError = "Color texture is required.";
                return Task.CompletedTask;
            }
            if (string.IsNullOrEmpty(NormalTexture))
            {
                TextureError = "Normal texture is required.";
                return Task.CompletedTask;
            }
            if (parent.AllItems.Any(p => string.Equals(p.ColorTexture, ColorTexture, StringComparison.OrdinalIgnoreCase)))
            {
                TextureError = "Texture is already registred.";
                return Task.CompletedTask;
            }
            return Import(ColorTexture, NormalTexture, null, GdtCatalogItemType.GameData, TextureTitle);
        }

        private async Task Import(string colorTexture, string normalTexture, SurfaceConfig? surfaceConfig, GdtCatalogItemType itemType, string title)
        {
            using var fakeSat = GdtHelper.GenerateFakeSatPngImage(parent.Previews, colorTexture);
            if (fakeSat == null)
            {
                TextureError = "Texture was not found.";
                return;
            }
            var color = GdtHelper.AllocateUniqueColor(fakeSat, parent.AllItems.Select(i => i.ColorId));
            var config = new GdtCatalogItem(new TerrainMaterial(normalTexture, colorTexture, color.ToRgb24(), fakeSat?.ToPngByteArray()), surfaceConfig, itemType, title);


            Result = await parent.Add(config);

            await TryCloseAsync(true);
        }

        public async Task ImportImage()
        {
            if (ImageColor == null)
            {
                ImageError = "Color texture image is required.";
                return;
            }
            if (!Arma3ImageHelper.IsValidSize(ImageColor))
            {
                ImageError = "Color texture image size is invalid.";
                return;
            }
            if (ImageNormal == null)
            {
                await GenerateImageNormal();
                if (ImageNormal == null)
                {
                    ImageError = "Normal texture image is required.";
                    return;
                }
            }
            if (!Arma3ImageHelper.IsValidSize(ImageColor))
            {
                ImageError = "Normal texture image size is invalid.";
                return;
            }
            var lcName = Name.ToLowerInvariant();
            if (!Arma3ConfigHelper.IsValidClassName(lcName))
            {
                ImageError = Arma3ConfigHelper.ValidClassNameMessage(Name);
                return;
            }
            var nameWithoutExtension = $"{lcName}_co";
            if (parent.AllItems.Any(i => (i.ToSurfaceConfig()?.Match(nameWithoutExtension) ?? false) || string.Equals(nameWithoutExtension, Path.GetFileNameWithoutExtension(i.ColorTexture), StringComparison.OrdinalIgnoreCase) ))
            {
                ImageError = "Name is already used.";
                return;
            }
            var surfaceConfig = CopyConfigFrom?.ToSurfaceConfig();
            if (surfaceConfig != null)
            {
                surfaceConfig = surfaceConfig.WithNameAndFiles(CaseConverter.ToPascalCase(Name), $"{lcName}_*"); 
                if (parent.AllItems.Any(i => surfaceConfig.Match(Path.GetFileNameWithoutExtension(i.ColorTexture))))
                {
                    ImageError = "Name would conflict with existing textures.";
                    return;
                }
            }

            var storage = IoC.Get<IArma3ImageStorage>();

            storage.SavePng($"{{PboPrefix}}\\data\\gdt\\{lcName}_co.paa", ImageColor);

            storage.SavePng($"{{PboPrefix}}\\data\\gdt\\{lcName}_nopx.paa", ImageNormal);

            IoC.Get<IArma3ImageStorage>().ProcessPngToPaaBackground();

            await Import(
                $"{{PboPrefix}}\\data\\gdt\\{lcName}_co.paa", 
                $"{{PboPrefix}}\\data\\gdt\\{lcName}_nopx.paa",
                surfaceConfig,
                GdtCatalogItemType.Image,
                ImageTitle);
        }



        public Task BrowseExistingColor()
        {
            var value = BrowseProjectDrive();
            if (value != null)
            {
                ColorTexture = value;
                if (ColorTexture.EndsWith("_co.paa", StringComparison.OrdinalIgnoreCase))
                {
                    var normalCandidate = ColorTexture.Replace("_co.paa", "_nopx.paa");
                    if (File.Exists(Path.Combine(Arma3ToolsHelper.GetProjectDrivePath(), normalCandidate)))
                    {
                        NormalTexture = normalCandidate;
                    }
                }
            }
            return Task.CompletedTask;
        }
        public Task BrowseExistingNormal()
        {
            var value = BrowseProjectDrive();
            if (value != null)
            {
                NormalTexture = value;
            }
            return Task.CompletedTask;
        }

        private static string? BrowseProjectDrive()
        {
            var pDrive = Arma3ToolsHelper.GetProjectDrivePath();
            var dialog = new OpenFileDialog();
            dialog.Filter = "PAA File|*.paa";
            dialog.InitialDirectory = pDrive;
            if (dialog.ShowDialog() == true && dialog.FileName.StartsWith(pDrive, StringComparison.OrdinalIgnoreCase))
            {
                return dialog.FileName.Substring(pDrive.Length + 1);
            }
            return null;
        }

        private BitmapFrame? _imageColor;
        public BitmapFrame? ImageColor { get { return _imageColor; } set {  Set(ref _imageColor, value); } }

        private BitmapFrame? _imageNormal;
        public BitmapFrame? ImageNormal { get { return _imageNormal; } set { Set(ref _imageNormal, value); } }

        private string _name = string.Empty;
        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }

        private string _textureTitle = string.Empty;
        public string TextureTitle
        {
            get { return _textureTitle; }
            set { Set(ref _textureTitle, value); }
        }


        private string _imageTitle = string.Empty;
        public string ImageTitle
        {
            get { return _imageTitle; }
            set { Set(ref _imageTitle, value); }
        }

        private string _imageError = string.Empty;
        public string ImageError
        {
            get { return _imageError; }
            set { Set(ref _imageError, value); }
        }

        private string _textureError = string.Empty;
        public string TextureError
        {
            get { return _textureError; }
            set { Set(ref _textureError, value); }
        }

        public Task BrowseImageColor()
        {
            ImageColor = FileDialogHelper.BrowseImage() ?? ImageColor;
            return Task.CompletedTask;
        }
        
        public Task BrowseImageNormal()
        {
            ImageNormal = FileDialogHelper.BrowseImage() ?? ImageNormal;
            return Task.CompletedTask;
        }
        
        public Task GenerateImageNormal()
        {
            if (ImageColor != null)
            {
                ImageNormal = GdtHelper.GenerateNormalMap(ImageColor);
            }
            return Task.CompletedTask;
        }
    }
}