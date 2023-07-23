using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Text;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Geometries;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.CompositionTool.Views;

namespace GameRealisticMap.Studio.Modules.CompositionTool.ViewModels
{
    internal class CompositionItem : PropertyChangedBase
    {
        private readonly TerrainBuilderObject terrainBuilderObject;

        public CompositionItem(CompositionObject source, CompositionViewModel parent)
        {
            terrainBuilderObject = source.ToTerrainBuilderObject(System.Numerics.Matrix4x4.Identity, ElevationMode.Absolute);
            Model = terrainBuilderObject.Model;
            _x = terrainBuilderObject.Point.X;
            _y = terrainBuilderObject.Point.Y;
            _z = terrainBuilderObject.Elevation;
            _picth = terrainBuilderObject.Pitch;
            _yaw = terrainBuilderObject.Yaw;
            _roll = terrainBuilderObject.Roll;
            _scale = terrainBuilderObject.Scale;
        }

        public bool WasEdited => _x != terrainBuilderObject.Point.X
            || _y != terrainBuilderObject.Point.Y
            || _z != terrainBuilderObject.Elevation
            || _picth != terrainBuilderObject.Pitch
            || _yaw != terrainBuilderObject.Yaw
            || _roll != terrainBuilderObject.Roll
            || _scale != terrainBuilderObject.Scale;

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
        public float Scale { get { return _scale; } set { _scale = value; NotifyOfPropertyChange(); NotifyPreview(); } }

        internal CompositionObject ToDefinition()
        {
            return new CompositionObject(Model, ToTerrainBuilderObject().ToWrpTransform());
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
        }

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
    }
}