using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        public float X { get { return _x; } set { _x = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(PreviewPath)); } }

        private float _y;
        public float Y { get { return _y; } set { _y = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(PreviewPath)); } }

        private float _z;
        public float Z { get { return _z; } set { _z = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(PreviewPath)); } }

        private float _picth;
        public float Pitch { get { return _picth; } set { _picth = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(PreviewPath)); } }

        private float _yaw;
        public float Yaw { get { return _yaw; } set { _yaw = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(PreviewPath)); } }

        private float _roll;
        public float Roll { get { return _roll; } set { _roll = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(PreviewPath)); } }

        private float _scale;
        public float Scale { get { return _scale; } set { _scale = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(PreviewPath)); } }

        internal CompositionObject ToDefinition()
        {
            return new CompositionObject(Model, ToTerrainBuilderObject().ToWrpTransform());
        }

        internal TerrainBuilderObject ToTerrainBuilderObject()
        {
            return new TerrainBuilderObject(Model, new TerrainPoint(X, Y), Z, ElevationMode.Absolute, Yaw, Pitch, Roll, Scale);
        }

        public string PreviewPath
        {
            get
            {
                var polygons = IoC.Get<IArma3DataModule>().ModelPreviewHelper.ToPolygons(ToTerrainBuilderObject());
                return ToPath(polygons);
            }
        }

        public string PreviewVisualPath
        {
            get
            {
                var polygons = IoC.Get<IArma3DataModule>().ModelPreviewHelper.ToVisualPolygons(ToTerrainBuilderObject());
                return ToPath(polygons);
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
    }
}