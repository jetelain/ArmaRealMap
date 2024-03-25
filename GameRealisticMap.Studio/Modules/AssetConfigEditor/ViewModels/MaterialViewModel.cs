using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Arma3.GameEngine.Materials;
using GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels;
using GameRealisticMap.Studio.Toolkit;
using Color = System.Windows.Media.Color;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal class MaterialViewModel : AssetIdBase<TerrainMaterialUsage, TerrainMaterial>
    {
        private byte[]? _fakeSatPngImage;
        private string _colorTexture;
        private string _normalTexture;
        private Color _colorId;
        private bool _useLibraryColor;
        private GdtDetailViewModel? _libraryItem;

        public MaterialViewModel(TerrainMaterialUsage id, TerrainMaterial terrainMaterial, GdtDetailViewModel? libraryItem, AssetConfigEditorViewModel parent)
            : base(id, parent)
        {
            _colorId = terrainMaterial.Id.ToWpfColor();
            _colorTexture = terrainMaterial.ColorTexture;
            _normalTexture = terrainMaterial.NormalTexture;
            _fakeSatPngImage = terrainMaterial.FakeSatPngImage;
            _libraryItem = libraryItem;
            if (libraryItem != null)
            {
                _useLibraryColor = libraryItem.ColorId == _colorId;
            }
        }

        internal static async Task<MaterialViewModel> Create(TerrainMaterialUsage id, Arma3Assets arma3Assets, AssetConfigEditorViewModel parent)
        {
            var mat = arma3Assets.Materials.GetMaterialByUsage(id);
            var surf = arma3Assets.Materials.GetSurface(mat);

            GdtDetailViewModel? libraryItem = null;
            if (!string.IsNullOrEmpty(mat.ColorTexture))
            {
                libraryItem = await IoC.Get<GdtBrowserViewModel>().Resolve(mat, surf);
            }
            return new MaterialViewModel(id, mat, libraryItem, parent);
        }

        public override string Icon => $"pack://application:,,,/GameRealisticMap.Studio;component/Resources/Icons/Generic.png";

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            LibraryItemsViewSource = new CollectionViewSource();
            LibraryItemsViewSource.Source = IoC.Get<GdtBrowserViewModel>().AllItems;
            LibraryItemsViewSource.SortDescriptions.Add(new SortDescription(nameof(GdtDetailViewModel.DisplayName), ListSortDirection.Ascending));
            NotifyOfPropertyChange(nameof(LibraryItemsViewSource));

            await base.OnInitializeAsync(cancellationToken);
        }

        public bool UseLibraryColor
        {
            get { return _useLibraryColor; }
            set 
            { 
                if (Set(ref _useLibraryColor, value)) 
                {
                    NotifyOfPropertyChange(nameof(ColorId));
                    NotifyOfPropertyChange(nameof(UseCustomColor));
                }
            }
        }

        public bool UseCustomColor
        {
            get { return !UseLibraryColor; }
            set { UseLibraryColor = !UseLibraryColor; }
        }

        public Color ColorId
        {
            get { return _colorId; }
            set { Set(ref _colorId, value); }
        }

        public Color ActualColorId
        {
            get
            {
                if (UseLibraryColor)
                {
                    return LibraryItem?.ColorId ?? _colorId;
                }
                return _colorId;
            }
        }

        public string ColorTexture
        {
            get 
            {
                if (_libraryItem != null)
                {
                    return _libraryItem.ColorTexture;
                }
                return _colorTexture;
            }
        }

        public string NormalTexture
        {
            get 
            {
                if (_libraryItem != null)
                {
                    return _libraryItem.NormalTexture;
                }
                return _normalTexture; 
            }
        }

        public CollectionViewSource? LibraryItemsViewSource { get; private set; }

        public GdtDetailViewModel? LibraryItem
        {
            get { return _libraryItem; }
            set 
            {
                if (Set(ref _libraryItem, value))
                {
                    NotifyOfPropertyChange(nameof(ColorTexture));
                    NotifyOfPropertyChange(nameof(NormalTexture));
                    if (_useLibraryColor)
                    {
                        NotifyOfPropertyChange(nameof(ColorId));
                    }
                }
            }
        }

        public override TerrainMaterial ToDefinition()
        {
            if (_libraryItem != null)
            {
                var material = _libraryItem.ToMaterial();
                if (UseCustomColor)
                {
                    return new TerrainMaterial(material.NormalTexture, material.ColorTexture, _colorId.ToRgb24(), material.FakeSatPngImage);
                }
                return material;
            }
            // Was unable to find item in library, keep as-is
            return new TerrainMaterial(_normalTexture, _colorTexture, _colorId.ToRgb24(), _fakeSatPngImage);
        }

        public SurfaceConfig? GetSurfaceConfig()
        {
            return _libraryItem?.ToSurfaceConfig();
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

        public Task OpenMaterial()
        {
            return LibraryItem?.OpenMaterial() ?? Task.CompletedTask;
        }

        internal TerrainMaterialData? GetData()
        {
            return _libraryItem?.ToData();
        }
    }
}