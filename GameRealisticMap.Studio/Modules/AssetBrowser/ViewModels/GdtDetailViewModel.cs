using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Arma3.GameEngine.Materials;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using GameRealisticMap.Studio.Modules.AssetBrowser.Services;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;
using GameRealisticMap.Studio.Toolkit;
using GameRealisticMap.Studio.UndoRedo;
using Gemini.Framework;
using Gemini.Framework.Services;
using HelixToolkit.Wpf;
using Microsoft.Win32;
using NetTopologySuite.GeometriesGraph;
using SixLabors.ImageSharp;
using Color = System.Windows.Media.Color;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels
{
    internal class GdtDetailViewModel : Document, IPersistedDocument, IModelImporterTarget
    {
        private readonly GdtCatalogItemType itemType;

        public GdtDetailViewModel(GdtBrowserViewModel parent, GdtCatalogItem item)
        {
            ParentEditor = parent;

            var config = item.Config;
            if (config != null)
            {
                _name = config.Name;
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
                _name = string.Empty;
                ClutterList = new UndoableObservableCollection<GdtClutterViewModel>();
            }

            CompositionImporter = new CompositionImporter(this);
            RemoveItem = new RelayCommand(i => ClutterList.RemoveUndoable(UndoRedoManager, (GdtClutterViewModel)i));

            itemType = item.ItemType;
            _colorTexture = item.Material.ColorTexture;
            _normalTexture = item.Material.NormalTexture;
            _colorId = item.Material.Id.ToWpfColor();
            FakeSatPngImage = item.Material.FakeSatPngImage;

            DisplayName = Path.GetFileNameWithoutExtension(ColorTexture);
        }

        public GdtBrowserViewModel ParentEditor { get; }

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

        public bool IsEditable => itemType == GdtCatalogItemType.Image;

        public bool IsReadOnly => !IsEditable;

        public UndoableObservableCollection<GdtClutterViewModel> ClutterList { get; }

        private string _name;
        public string Name { get { return _name; } set { Set(ref _name, value); } }

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
            return new GdtCatalogItem(ToMaterial(), ToSurfaceConfig(), itemType);
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
                Name = item.Config.Name;
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
            if (itemType == GdtCatalogItemType.Image)
            {
                var imgStore = IoC.Get<IArma3ImageStorage>();

                return new TerrainMaterialData(
                    TerrainMaterialDataFormat.PNG, 
                    imgStore.ReadPngBytes(ColorTexture), 
                    imgStore.ReadPngBytes(NormalTexture));
            }
            return null;
        }

        public bool IsDirty { get; set; }
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

        public Task SaveImage()
        {
            if (itemType == GdtCatalogItemType.Image )
            {
                if (_imageColorWasChanged && _imageColor != null)
                {
                    IoC.Get<IArma3ImageStorage>().Save(ColorTexture, _imageColor);
                    _imageColorWasChanged = false;
                }
                if (_imageNormalWasChanged && _imageNormal != null)
                {
                    IoC.Get<IArma3ImageStorage>().Save(ColorTexture, _imageNormal);
                    _imageNormalWasChanged = false;
                }
            }
            return Task.CompletedTask;
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
            ImageColor = GetTextureImage(ColorTexture);
            ImageNormal = GetTextureImage(NormalTexture);
            _imageColorWasChanged = false;
            _imageNormalWasChanged = false;
            return base.OnInitializeAsync(cancellationToken);
        }

        private BitmapFrame? GetTextureImage(string texture)
        {
            var uri = ParentEditor.Previews.GetTexturePreview(texture);
            return uri == null ? null : BitmapFrame.Create(uri);
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
            AddClutter(group);
            tool.Model3DGroup = group;
            IoC.Get<IShell>().ShowTool(tool);
            return Task.CompletedTask;
        }
        private void AddClutter(Model3DGroup group)
        {
            var clutterPositions = Enumerable.Range(0, 20).Select(_ => new Point3D((Random.Shared.NextDouble() * 8) - 4, 0, (Random.Shared.NextDouble() * 8) - 4));

            foreach (var clutter in ClutterList)
            {
                var count = (int)Math.Round(clutter.Probability * 20);
                foreach (var pos in clutterPositions.Take(count))
                {
                    var preview = IoC.Get<IArma3Preview3D>().GetModel(clutter.Model.Path);
                    if (preview != null)
                    {
                        var matrix = Matrix3D.Identity;
                        var scale = clutter.ScaleMin + (Random.Shared.NextDouble() * (clutter.ScaleMax - clutter.ScaleMin));
                        matrix.Scale(new Vector3D(scale, scale, scale));
                        matrix.Translate(pos.ToVector3D());
                        preview.Transform = new MatrixTransform3D(matrix);
                        group.Children.Add(preview);
                    }

                }
                clutterPositions = clutterPositions.Skip(count);
            }
        }

        private Model3DGroup CreateBasePreview3d()
        {
            var group = new Model3DGroup();

            var meshGdt = new MeshGeometry3D();
            meshGdt.Positions = new Point3DCollection() {
                new Point3D(0, 0, 0),
                new Point3D(0, 0, 4),
                new Point3D(4, 0, 4),
                new Point3D(4, 0, 0),

                new Point3D(-4, 0, -4),
                new Point3D(-4, 0, 0),
                new Point3D(0, 0, 0),
                new Point3D(0, 0, -4),

                new Point3D(0, 0, -4),
                new Point3D(0, 0, 0),
                new Point3D(4, 0, 0),
                new Point3D(4, 0, -4),

                new Point3D(-4, 0, 0),
                new Point3D(-4, 0, 4),
                new Point3D(0, 0, 4),
                new Point3D(0, 0, 0),

            };
            meshGdt.TextureCoordinates = new PointCollection() {
                new System.Windows.Point(0,0),
                new System.Windows.Point(0,1),
                new System.Windows.Point(1,1),
                new System.Windows.Point(1,0),

                new System.Windows.Point(0,0),
                new System.Windows.Point(0,1),
                new System.Windows.Point(1,1),
                new System.Windows.Point(1,0),

                new System.Windows.Point(0,0),
                new System.Windows.Point(0,1),
                new System.Windows.Point(1,1),
                new System.Windows.Point(1,0),

                new System.Windows.Point(0,0),
                new System.Windows.Point(0,1),
                new System.Windows.Point(1,1),
                new System.Windows.Point(1,0)
            };
            meshGdt.TriangleIndices = new Int32Collection() {
                0, 1, 2,
                2, 3, 0,

                4, 5, 6,
                6, 7, 4,

                8, 9, 10,
                10, 11, 8,

                12, 13, 14,
                14, 15, 12
            };

            group.Children.Add(new GeometryModel3D(meshGdt, new DiffuseMaterial(new ImageBrush(ImageColor))));

            var meshSat = new MeshGeometry3D();
            meshSat.Positions = new Point3DCollection() {
                new Point3D(4, 0, -4),
                new Point3D(4, 0, 4),
                new Point3D(6, 0, 4),
                new Point3D(6, 0, -4),

                new Point3D(-4, 0, 4),
                new Point3D(-4, 0, 6),
                new Point3D(6, 0, 6),
                new Point3D(6, 0, 4),
            };
            meshSat.TextureCoordinates = new PointCollection() {
                new System.Windows.Point(0,0),
                new System.Windows.Point(0,1),
                new System.Windows.Point(1,1),
                new System.Windows.Point(1,0),

                new System.Windows.Point(0,0),
                new System.Windows.Point(0,1),
                new System.Windows.Point(1,1),
                new System.Windows.Point(1,0)
            };
            meshSat.TriangleIndices = new Int32Collection() {
                0, 1, 2,
                2, 3, 0,
                4, 5, 6,
                6, 7, 4,
            };
            group.Children.Add(new GeometryModel3D(meshSat, new DiffuseMaterial(new ImageBrush(FakeSatPreview))));

            return group;
        }

        public Task BrowseImageColor()
        {
            var img = FileDialogHelper.BrowseImage();
            if (img != null)
            {
                ImageColor = img;
                _imageColorWasChanged = true;
            }
            return Task.CompletedTask;
        }

        public Task BrowseImageNormal()
        {
            var img = FileDialogHelper.BrowseImage();
            if (img != null)
            {
                ImageNormal = img;
                _imageNormalWasChanged = true;
            }
            return Task.CompletedTask;
        }

        public Task GenerateImageNormal()
        {
            return Task.CompletedTask;
        }
    }
}
