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
using GameRealisticMap.Studio.Modules.Reporting;
using GameRealisticMap.Studio.Toolkit;
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

            Name = "gdt_" + Base36Converter.Convert(Random.Shared.NextInt64());
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
            if (parent.AllItems.Any(p => string.Equals(p.ColorTexture, ColorTexture, StringComparison.OrdinalIgnoreCase)))
            {
                // Duplicate
                return Task.CompletedTask;
            }

            return Import(ColorTexture, NormalTexture, null, GdtCatalogItemType.GameData);
        }

        private Task Import(string colorTexture, string normalTexture, SurfaceConfig? surfaceConfig, GdtCatalogItemType itemType)
        {
            using var fakeSat = GdtHelper.GenerateFakeSatPngImage(parent.Previews, colorTexture);
            if (fakeSat == null)
            {
                // File not found
                return Task.CompletedTask;
            }
            var color = GdtHelper.AllocateUniqueColor(fakeSat, parent.AllItems.Select(i => i.ColorId));
            var config = new GdtCatalogItem(new TerrainMaterial(normalTexture, colorTexture, color.ToRgb24(), fakeSat?.ToPngByteArray()), surfaceConfig, itemType);
            Result = new GdtDetailViewModel(parent, config);
            parent.AllItems.Add(Result);
            return TryCloseAsync(true);
        }

        public async Task ImportImage()
        {
            if (ImageColor == null)
            {
                // Missing
                return;
            }

            if (ImageNormal == null)
            {
                await GenerateImageNormal();
                if (ImageNormal == null)
                {
                    return;
                }
            }

            var lcName = Name.ToLowerInvariant();

            var storage = IoC.Get<IArma3ImageStorage>();

            storage.SavePng($"{{PboPrefix}}\\data\\gdt\\{lcName}_co.paa", ImageColor);
            storage.SavePng($"{{PboPrefix}}\\data\\gdt\\{lcName}_nopx.paa", ImageNormal);

            IoC.Get<IArma3ImageStorage>().ProcessPngToPaaBackground();

            var surfaceConfig = CopyConfigFrom?.ToSurfaceConfig();
            if (surfaceConfig != null)
            {
                surfaceConfig = surfaceConfig.WithNameAndFiles(CaseConverter.ToPascalCase(Name), $"{lcName}_*");
            }

            await Import(
                $"{{PboPrefix}}\\data\\gdt\\{lcName}_co.paa", 
                $"{{PboPrefix}}\\data\\gdt\\{lcName}_nopx.paa",
                surfaceConfig,
                GdtCatalogItemType.Image);
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
            return Task.CompletedTask;
        }
    }
}