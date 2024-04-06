using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Arma3.GameEngine.Materials;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using GameRealisticMap.Studio.Modules.AssetBrowser.Services;
using GameRealisticMap.Studio.Modules.AssetBrowser.Services.Gdt;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;
using GameRealisticMap.Studio.Toolkit;
using GameRealisticMap.Studio.UndoRedo;
using GameRealisticMap.Toolkit;
using Gemini.Framework;
using Gemini.Framework.Services;
using Gemini.Modules.UndoRedo;
using Microsoft.Win32;
using SixLabors.ImageSharp;
using Color = System.Windows.Media.Color;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels
{
    internal class GdtDetailViewModel : Document, IPersistedDocument, IModelImporterTarget
    {
        private readonly GdtCatalogItemType _itemType;

        public GdtDetailViewModel(GdtBrowserViewModel parent, GdtCatalogItem item)
        {
            ParentEditor = parent;

            var config = item.Config;
            if (config != null)
            {
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
            else
            {
                ClutterList = new UndoableObservableCollection<GdtClutterViewModel>();
            }

            CompositionImporter = new CompositionImporter(this);
            RemoveItem = new RelayCommand(i => ClutterList.RemoveUndoable(UndoRedoManager, (GdtClutterViewModel)i));

            _itemType = item.ItemType;
            _colorTexture = item.Material.ColorTexture;
            _normalTexture = item.Material.NormalTexture;
            _colorId = item.Material.Id.ToWpfColor();
            FakeSatPngImage = item.Material.FakeSatPngImage;
            Title = item.Title;
            UndoRedoManager.PropertyChanged += (_, _) => { IsDirty = true; };
        }

        public GdtBrowserViewModel ParentEditor { get; }


        protected override IUndoRedoManager CreateUndoRedoManager()
        {
            return ParentEditor.UndoRedoManager;
        }

        private Color _colorId;
        public Color ColorId
        {
            get { return _colorId; }
            set
            {
                if (Set(ref _colorId, value))
                {
                    ParentEditor.ComputeColorUniqueness();
                }
            }
        }

        private bool _isNotColorUnique;

        public bool IsNotColorUnique
        {
            get { return _isNotColorUnique; }
            set { Set(ref _isNotColorUnique, value); }
        }

        private string _colorTexture = string.Empty;
        public string ColorTexture
        {
            get { return _colorTexture; }
            set
            {
                if (Set(ref _colorTexture, value))
                {
                    GenerateFakeSatPngImage();
                }
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

        public bool IsEditable => _itemType == GdtCatalogItemType.Image;

        public bool IsReadOnly => !IsEditable;

        public UndoableObservableCollection<GdtClutterViewModel> ClutterList { get; }

        private string _title = string.Empty;
        public string Title { get { return _title; } set { if (Set(ref _title, value)) { UpdateDisplayName(); } } }

        private bool _isDirty;
        public bool IsDirty { get { return _isDirty; } set { if (Set(ref _isDirty, value)) { UpdateDisplayName(); } } }

        public string Name { get; } = string.Empty;

        public string Files { get; } = string.Empty;

        private bool _aceCanDig;
        public bool AceCanDig { get { return _aceCanDig; } set { Set(ref _aceCanDig, value); } }

        private string _soundEnviron = string.Empty;
        public string SoundEnviron { get { return _soundEnviron; } set { Set(ref _soundEnviron, value); } }

        private string _soundHit = string.Empty;
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

        private string _impact = string.Empty;
        public string Impact { get { return _impact; } set { Set(ref _impact, value); } }

        private double _surfaceFriction;
        public double SurfaceFriction { get { return _surfaceFriction; } set { Set(ref _surfaceFriction, value); } }

        private double _maxClutterColoringCoef;
        public double MaxClutterColoringCoef { get { return _maxClutterColoringCoef; } set { Set(ref _maxClutterColoringCoef, value); } }

        public bool IsNew => false;

        public string FileName => string.Empty;

        public string FilePath => string.Empty;

        private void GenerateFakeSatPngImage()
        {
            using var img = GdtHelper.GenerateFakeSatPngImage(IoC.Get<IArma3Previews>(), _colorTexture);
            if (img != null)
            {
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
            FakeSatPngImage = img.ToPngByteArray();
        }

        public GdtCatalogItem ToDefinition()
        {
            return new GdtCatalogItem(ToMaterial(), ToSurfaceConfig(), _itemType, Title);
        }

        public SurfaceConfig? ToSurfaceConfig()
        {
            return new SurfaceConfig(
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
        }

        public TerrainMaterial ToMaterial()
        {
            return new TerrainMaterial(NormalTexture, ColorTexture, _colorId.ToRgb24(), FakeSatPngImage);
        }

        public Task OpenMaterial()
        {
            return IoC.Get<IShell>().OpenDocumentAsync(this);
        }

        internal void SyncCatalog(GdtCatalogItem item)
        {
            // ColorTexture = item.Material.ColorTexture;
            // NormalTexture = item.Material.NormalTexture;
            // ColorId = item.Material.Id.ToWpfColor();
            // FakeSatPngImage = item.Material.FakeSatPngImage; 
            if (item.Config != null)
            {
                AceCanDig = item.Config.AceCanDig;
                SoundEnviron = item.Config.SoundEnviron;
                SoundHit = item.Config.SoundHit;
                Rough = item.Config.Rough;
                MaxSpeedCoef = item.Config.MaxSpeedCoef;
                Dust = item.Config.Dust;
                Lucidity = item.Config.Lucidity;
                GrassCover = item.Config.GrassCover;
                Impact = item.Config.Impact;
                SurfaceFriction = item.Config.SurfaceFriction;
                MaxClutterColoringCoef = item.Config.MaxClutterColoringCoef;
                ClutterList.Clear();
                ClutterList.AddRange(item.Config.Character.Select(c => new GdtClutterViewModel(c)));
            }
        }

        public Task GenerateColorId()
        {
            using var img = GdtHelper.GenerateFakeSatPngImage(ParentEditor.Previews, _colorTexture);
            ColorId = GdtHelper.AllocateUniqueColor(img, ParentEditor.AllItems.Where(a => a != this).Select(a => a.ColorId));
            return Task.CompletedTask;
        }

        internal TerrainMaterialData? ToData()
        {
            if (_itemType == GdtCatalogItemType.Image)
            {
                var imgStore = IoC.Get<IArma3ImageStorage>();

                return new TerrainMaterialData(
                    TerrainMaterialDataFormat.PAA, 
                    imgStore.ReadPaaBytes(ColorTexture), 
                    imgStore.ReadPaaBytes(NormalTexture));
            }
            return null;
        }

        public CompositionImporter CompositionImporter { get; }
        public RelayCommand RemoveItem { get; }

        public Task New(string fileName)
        {
            return Task.CompletedTask;
        }

        public Task Load(string filePath)
        {
            return Task.CompletedTask;
        }

        public Task Save(string filePath)
        {
            return ParentEditor.SaveChanges();
        }

        internal void OnSave()
        {
            if (_itemType == GdtCatalogItemType.Image )
            {
                if (_imageColorWasChanged && _imageColor != null)
                {
                    IoC.Get<IArma3ImageStorage>().SavePng(ColorTexture, _imageColor);
                    _imageColorWasChanged = false;
                }
                if (_imageNormalWasChanged && _imageNormal != null)
                {
                    IoC.Get<IArma3ImageStorage>().SavePng(NormalTexture, _imageNormal);
                    _imageNormalWasChanged = false;
                }
            }
            IsDirty = false;
        }

        public void AddComposition(Composition composition, ObjectPlacementDetectedInfos detected)
        {
            foreach(var obj in composition.Objects)
            {
                ClutterList.AddUndoable(UndoRedoManager, 
                    new GdtClutterViewModel(new ClutterConfig(Name + CaseConverter.ToPascalCase(obj.Model.Name),
                    1.0 / (ClutterList.Count + 1), 
                    obj.Model, 
                    0.5, 
                    false,
                    0.8, 
                    1.2)));
            }
        }

        private BitmapFrame? _imageColor;
        private bool _imageColorWasChanged;
        public BitmapFrame? ImageColor { get { return _imageColor; } set { Set(ref _imageColor, value); } }

        private BitmapFrame? _imageNormal;
        private bool _imageNormalWasChanged;
        public BitmapFrame? ImageNormal { get { return _imageNormal; } set { Set(ref _imageNormal, value); } }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            LoadImages();
            return base.OnInitializeAsync(cancellationToken);
        }

        private void LoadImages()
        {
            ImageColor = GetTextureImage(ColorTexture);
            ImageNormal = GetTextureImage(NormalTexture);
            _imageColorWasChanged = false;
            _imageNormalWasChanged = false;
        }

        private BitmapFrame? GetTextureImage(string texture)
        {
            return Arma3PreviewsHelper.GetBitmapFrame(ParentEditor.Previews.GetTexturePreview(texture));
        }

        public Task ShowPreview3D()
        {
            var tool = IoC.Get<PreviewToolViewModel>();
            var group = CreateBasePreview3d();
            tool.Model3DGroup = group;
            IoC.Get<IShell>().ShowTool(tool);
            return Task.CompletedTask;
        }

        public Task ShowPreview3DClutter()
        {
            var tool = IoC.Get<PreviewToolViewModel>();
            var group = CreateBasePreview3d();
            Gdt3dPreviewHelper.AddClutter(group, ClutterList.Select(c => c.ToDefinition()));
            tool.Model3DGroup = group;
            IoC.Get<IShell>().ShowTool(tool);
            return Task.CompletedTask;
        }

        private Model3DGroup CreateBasePreview3d()
        {
            if (ImageColor == null)
            {
                LoadImages();
            }
            return Gdt3dPreviewHelper.CreateBasePreview3d(ImageColor!, FakeSatPreview ?? ImageColor!);
        }

        public Task BrowseImageColor()
        {
            if (_itemType != GdtCatalogItemType.Image)
            {
                return Task.CompletedTask;
            }
            var img = FileDialogHelper.BrowseImage();
            if (img != null)
            {
                ImageColor = img;
                UndoRedoManager.ChangeProperty(this, o => o.ImageColor, img);
                _imageColorWasChanged = true;
            }
            return Task.CompletedTask;
        }

        public Task BrowseImageNormal()
        {
            if (_itemType != GdtCatalogItemType.Image)
            {
                return Task.CompletedTask;
            }
            var img = FileDialogHelper.BrowseImage();
            if (img != null)
            {
                UndoRedoManager.ChangeProperty(this, o => o.ImageNormal, img);
                _imageNormalWasChanged = true;
            }
            return Task.CompletedTask;
        }

        public Task GenerateImageNormal()
        {
            if (_itemType != GdtCatalogItemType.Image)
            {
                return Task.CompletedTask;
            }
            if (ImageColor != null)
            {
                UndoRedoManager.ChangeProperty(this, o => o.ImageNormal, BitmapFrame.Create(GdtHelper.GenerateNormalMap(ImageColor)));
            }
            return Task.CompletedTask;
        }


        public Task ColorFromArma3()
        {
            if (_itemType != GdtCatalogItemType.Image)
            {
                return Task.CompletedTask;
            }
            if (ImageColor != null)
            {
                UndoRedoManager.ChangeProperty(this, o => o.ImageColor, BitmapFrame.Create(ColorFix.FromArma3(ImageColor)));
            }
            return Task.CompletedTask;
        }

        public IEnumerable<string> GetModels()
        {
            return ClutterList.Select(c => c.Model.Path);
        }

        public IEnumerable<string> GetTextures()
        {
            return new[] { ColorTexture, NormalTexture };
        }

        public Task Remove()
        {
            return ParentEditor.Remove(this);
        }

        private void UpdateDisplayName()
        {
            DisplayName = IsDirty ? Title + "*" : Title;
        }
    }
}
