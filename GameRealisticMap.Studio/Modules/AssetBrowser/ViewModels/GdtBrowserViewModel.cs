using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.AssetBrowser.Services;
using Gemini.Framework;
using System.Threading;
using Caliburn.Micro;
using System.Windows.Data;
using System.ComponentModel;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Arma3;
using System.IO;
using BIS.PBO;
using BIS.Core.Config;
using SixLabors.ImageSharp.PixelFormats;
using System.Windows.Media;
using ISColor = SixLabors.ImageSharp.Color;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels
{
    [Export(typeof(GdtBrowserViewModel))]
    internal class GdtBrowserViewModel : BrowserViewModelBase
    {
        private readonly IArma3Previews _arma3Previews;
        private readonly IGdtCatalogService _catalogService;

        [ImportingConstructor]
        public GdtBrowserViewModel(IArma3DataModule arma3DataModule, IArma3Previews arma3Previews, IGdtCatalogService catalogService, IArma3ModsService arma3ModsService)
            : base(arma3DataModule, arma3ModsService)
        {
            _arma3Previews = arma3Previews;
            _catalogService = catalogService;
            DisplayName = "Ground Detail Texture Browser";
        }

        public BindableCollection<GdtDetailViewModel>? AllItems { get; private set; }

        public ICollectionView? Items { get; private set; }

        public IArma3Previews Previews => _arma3Previews;

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            var items = await _catalogService.GetOrLoad();

            AllItems = new BindableCollection<GdtDetailViewModel>(items.Select(m => new GdtDetailViewModel(this, m)));
            Items = CollectionViewSource.GetDefaultView(AllItems);
            Items.SortDescriptions.Add(new SortDescription(nameof(GdtDetailViewModel.DisplayName), ListSortDirection.Ascending));
            Items.Filter = (item) => Filter((GdtDetailViewModel)item);

            NotifyOfPropertyChange(nameof(Items));

            _ = Task.Run(() => UpdateActiveMods());

            if (items.Count == 0)
            {
                _ = Task.Run(() => ImportVanilla());
            }
        }

        private void ImportVanilla()
        {
            var pboFS = (_arma3DataModule.ProjectDrive.SecondarySource as PboFileSystem);
            if (pboFS != null)
            {
                ImportItems(new GdtImporter(_arma3DataModule.Library).ImportVanilla(pboFS));
            }
        }

        private void ImportItems(List<GdtImporterItem> gdtImporterItems)
        {
            IsImporting = true;

            var items = AllItems;
            if (items != null)
            {
                foreach (var item in gdtImporterItems)
                {
                    var existing = items.FirstOrDefault(i => string.Equals(i.ColorTexture, item.ColorTexture, StringComparison.OrdinalIgnoreCase));
                    if (existing != null)
                    {
                        existing.RefreshConfig(item);
                    }
                    else
                    {
                        items.Add(new GdtDetailViewModel(this, item));
                    }
                }
            }

            IsImporting = false;
        }

        private async Task SaveChanges()
        {
            var items = AllItems;
            if (items != null)
            {
                await _catalogService.SaveChanges(items.Select(i => i.ToDefinition()).ToList());
            }
        }

        internal Color AllocateUniqueColor(Color? avgColor)
        {
            Color wanted;

            if (avgColor != null)
            {
                var hsl = ColorSpaceConverter.ToHsl(new Rgb24(avgColor.Value.R, avgColor.Value.G, avgColor.Value.B));
                wanted = HslToWpfRgb(new Hsl(hsl.H, 1f, 0.5f));
            }
            else
            {
                wanted = RandomColor();
            }
            var items = AllItems;
            if (items != null)
            {
                int attempt = 1;
                while (items.Any(i => i.ColorId == wanted))
                {
                    wanted = RandomColor(attempt);
                    attempt++;
                }
            }
            return wanted;
        }

        private static Color HslToWpfRgb(Hsl hsl)
        {
            var rgb = (Rgb24)ColorSpaceConverter.ToRgb(hsl);
            return Color.FromRgb(rgb.R, rgb.G, rgb.B);
        }

        private static Color RandomColor(int attempt = 0)
        {
            if (attempt < 3)
            {
                return HslToWpfRgb(new Hsl(Random.Shared.Next(0, 360), 1f, 0.5f));
            }
            return Color.FromRgb((byte)Random.Shared.Next(64, 192), (byte)Random.Shared.Next(64, 192), (byte)Random.Shared.Next(64, 192));
        }
        private bool Filter(GdtDetailViewModel item)
        {
            if (!string.IsNullOrEmpty(filterText))
            {
                return item.DisplayName.Contains(filterText, StringComparison.OrdinalIgnoreCase) 
                    || item.ColorTexture.Contains(filterText, StringComparison.OrdinalIgnoreCase);
            }
            return true;
        }

        protected override void RefreshFilter()
        {
            Items?.Refresh();
        }
    }
}
