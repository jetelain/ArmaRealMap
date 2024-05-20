using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Geometries;
using GameRealisticMap.IO.Converters;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using GameRealisticMap.Studio.Modules.CompositionTool.Views;
using GameRealisticMap.Studio.Toolkit;

namespace GameRealisticMap.Studio.Modules.CompositionTool.ViewModels
{
    internal class CompositionItem : PropertyChangedBase
    {
        private readonly TerrainBuilderObject terrainBuilderObject;
        private readonly Lazy<BitmapSource?> aerial;
        private readonly Lazy<ObservableCollection<RandomizationOperationVM>> randomizations;
        private readonly bool _hadRandomizations;

        public CompositionItem(CompositionObject source, CompositionViewModel parent)
        {
            terrainBuilderObject = source.ToTerrainBuilderObjectVerbatim();
            Model = terrainBuilderObject.Model;
            _x = terrainBuilderObject.Point.X;
            _y = terrainBuilderObject.Point.Y;
            _z = terrainBuilderObject.Elevation;
            _picth = terrainBuilderObject.Pitch;
            _yaw = terrainBuilderObject.Yaw;
            _roll = terrainBuilderObject.Roll;
            _scale = terrainBuilderObject.Scale;
            _hadRandomizations = source.Randomizations != null && source.Randomizations.Count > 0;
            randomizations = new(() => CreateRandomizationList(source));
            aerial = new Lazy<BitmapSource?>(() => IoC.Get<IArma3AerialImageService>().GetImage(Model.Path));
        }

        private ObservableCollection<RandomizationOperationVM> CreateRandomizationList(CompositionObject source)
        {
            var list = new ObservableCollection<RandomizationOperationVM>(RandomizationOperationVM.Create(source.Randomizations, this));
            list.CollectionChanged += (_, _) => { NotifyOfPropertyChange(nameof(HasRandomizations)); wasChanged = true; };
            return list;
        }

        public bool WasEdited => wasChanged 
            || _x != terrainBuilderObject.Point.X
            || _y != terrainBuilderObject.Point.Y
            || _z != terrainBuilderObject.Elevation
            || _picth != terrainBuilderObject.Pitch
            || _yaw != terrainBuilderObject.Yaw
            || _roll != terrainBuilderObject.Roll
            || _scale != terrainBuilderObject.Scale
            || (randomizations.IsValueCreated && randomizations.Value.Any(r => r.WasEdited));

        public ModelInfo Model { get; }

        private float _x;
        public float X { get { return _x; } set { _x = value; NotifyOfPropertyChange(); NotifyPreview(); } }

        private float _y;
        public float Y { get { return _y; } set { _y = value; NotifyOfPropertyChange(); NotifyPreview(); } }

        private float _z;
        public float Z { get { return _z; } set { _z = value; NotifyOfPropertyChange(); NotifyPreview(); } }

        private float _picth;
        public float Pitch { get { return _picth; } set { _picth = value; NotifyOfPropertyChange(); NotifyPreview(); } }

        private float _yaw;
        public float Yaw { get { return _yaw; } set { _yaw = value; NotifyOfPropertyChange(); NotifyPreview(); } }

        private float _roll;
        public float Roll { get { return _roll; } set { _roll = value; NotifyOfPropertyChange(); NotifyPreview(); } }

        private float _scale;
        private bool wasChanged;

        public float Scale { get { return _scale; } set { _scale = value; NotifyOfPropertyChange(); NotifyPreview(); } }

        internal CompositionObject ToDefinition()
        {
            return new CompositionObject(Model, ToTerrainBuilderObject().ToWrpTransform(), randomizations.Value.Select(v => v.ToDefinition()).ToList());
        }

        internal TerrainBuilderObject ToTerrainBuilderObject()
        {
            return new TerrainBuilderObject(Model, new TerrainPoint(X, Y), Z, ElevationMode.Absolute, Yaw, Pitch, Roll, Scale);
        }

        private void NotifyPreview()
        {
            NotifyOfPropertyChange(nameof(PreviewGeoAxisY));
            NotifyOfPropertyChange(nameof(PreviewVisualAxisY));
            NotifyOfPropertyChange(nameof(PreviewGeoAxisZ));
            NotifyOfPropertyChange(nameof(PreviewVisualAxisZ));
            NotifyOfPropertyChange(nameof(PreviewGeoAxisX));
            NotifyOfPropertyChange(nameof(PreviewVisualAxisX));
            NotifyOfPropertyChange(nameof(AerialMatrix));
        }

        public ObservableCollection<RandomizationOperationVM> Randomizations => randomizations.Value;

        public bool HasRandomizations => randomizations.IsValueCreated ? randomizations.Value.Count > 0 : _hadRandomizations;

        public string PreviewGeoAxisY
        {
            get
            {
                return ToPath(IoC.Get<IArma3DataModule>().ModelPreviewHelper.ToGeoAxisY(ToTerrainBuilderObject()));
            }
        }

        public string PreviewVisualAxisY
        {
            get
            {
                return ToPath(IoC.Get<IArma3DataModule>().ModelPreviewHelper.ToVisualAxisY(ToTerrainBuilderObject()));
            }
        }

        public string PreviewGeoAxisZ
        {
            get
            {
                return ToPath(IoC.Get<IArma3DataModule>().ModelPreviewHelper.ToGeoAxisZ(ToTerrainBuilderObject()));
            }
        }

        public string PreviewVisualAxisZ
        {
            get
            {
                return ToPath(IoC.Get<IArma3DataModule>().ModelPreviewHelper.ToVisualAxisZ(ToTerrainBuilderObject()));
            }
        }


        public string PreviewGeoAxisX
        {
            get
            {
                return ToPath(IoC.Get<IArma3DataModule>().ModelPreviewHelper.ToGeoAxisX(ToTerrainBuilderObject()));
            }
        }

        public string PreviewVisualAxisX
        {
            get
            {
                return ToPath(IoC.Get<IArma3DataModule>().ModelPreviewHelper.ToVisualAxisX(ToTerrainBuilderObject()));
            }
        }

        public BitmapSource? AerialImage => aerial.Value;

        public Matrix AerialMatrix
        {
            get
            {
                var matrix = new Matrix(1, 0, 0, 1, -(AerialImage?.PixelWidth ?? 0) / 8, -(AerialImage?.PixelHeight ?? 0) / 8);
                matrix.Append(ToTerrainBuilderObject().ToWrpTransform().ToAerialWpfMatrix());
                return matrix;
            }
        }

        public double AerialWidth => aerial.Value?.PixelWidth / 4 ?? 0;

        public double AerialHeight => aerial.Value?.PixelHeight / 4 ?? 0;

        private static string ToPath(IEnumerable<TerrainPolygon> polygons)
        {
            var sb = new StringBuilder();
            foreach (var polygon in polygons)
            {
                for (int i = 0; i < polygon.Shell.Count; ++i)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(' ');
                    }
                    var point = polygon.Shell[i];
                    if (i == 0)
                    {
                        sb.Append('M');
                    }
                    else
                    {
                        sb.Append('L');
                    }
                    sb.Append((point.X * CanvasGrid.Scale + CanvasGrid.HalfSize).ToString(CultureInfo.InvariantCulture));
                    sb.Append(',');
                    sb.Append((point.Y * -CanvasGrid.Scale + CanvasGrid.HalfSize).ToString(CultureInfo.InvariantCulture));
                    sb.Append(' ');
                }
                sb.Append(" Z");
            }
            return sb.ToString();
        }

        internal void Transform(Matrix4x4 transform)
        {
            var newMatrix = ToTerrainBuilderObject().ToWrpTransform() * transform;
            var edited = new TerrainBuilderObject(Model, newMatrix, ElevationMode.Absolute);
            _x = edited.Point.X;
            _y = edited.Point.Y;
            _z = edited.Elevation;
            _picth = edited.Pitch;
            _yaw = edited.Yaw;
            _roll = edited.Roll;

            NotifyOfPropertyChange(nameof(X));
            NotifyOfPropertyChange(nameof(Y));
            NotifyOfPropertyChange(nameof(Z));
            NotifyOfPropertyChange(nameof(Pitch));
            NotifyOfPropertyChange(nameof(Yaw));
            NotifyOfPropertyChange(nameof(Roll));
            NotifyPreview();
        }

        public Task AddRandomization(string what)
        {
            switch(what)
            {
                case "ScaleUniform":
                    Randomizations.Insert(0, new(new(RandomizationOperation.ScaleUniform, 0.8f, 1.2f, new (X, Z, Y)), this));
                    break;
                case "RotateY":
                    Randomizations.Add(new(new(RandomizationOperation.RotateY, 0, 360, new (X, Z, Y)), this));
                    break;
                case "TranslateRadiusXZ":
                    Randomizations.Add(new(new(RandomizationOperation.TranslateRadiusXZ, 0, 1, new(X, Z, Y)), this));
                    break;
                case "TranslateX":
                    Randomizations.Add(new(new(RandomizationOperation.TranslateX, 0, 1, null), this)); 
                    break;
                case "TranslateZ":
                    Randomizations.Add(new(new(RandomizationOperation.TranslateZ, 0, 1, null), this));
                    break;
                case "TranslateY":
                    Randomizations.Add(new(new(RandomizationOperation.TranslateY, 0, 1, null), this));
                    break;
            }
            return Task.CompletedTask;
        }
    }
}