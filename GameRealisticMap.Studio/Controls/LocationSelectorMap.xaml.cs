using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CoordinateSharp;
using GameRealisticMap.Studio.Shared;
using GameRealisticMap.Studio.Toolkit;
using MapControl;
using MapControl.Caching;

namespace GameRealisticMap.Studio.Controls
{
    /// <summary>
    /// Logique d'interaction pour LocationSelectorMap.xaml
    /// </summary>
    public partial class LocationSelectorMap : UserControl
    {
        static LocationSelectorMap()
        {
            ImageLoader.HttpClient.DefaultRequestHeaders.Add("User-Agent", "GameRealisticMap Studio/"+ App.GetAppVersion());
            TileImageLoader.Cache = new ImageFileCache(TileImageLoader.DefaultCacheFolder);
        }

        public LocationSelection? MapSelection
        {
            get { return (LocationSelection?)GetValue(MapSelectionProperty); }
            set { SetValue(MapSelectionProperty, value); }
        }

        public static readonly DependencyProperty MapSelectionProperty =
            DependencyProperty.Register(nameof(MapSelection), typeof(LocationSelection), typeof(LocationSelectorMap), new PropertyMetadata(null, LocationChanged));

        private static void LocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as LocationSelectorMap)?.LocationChanged((LocationSelection?)e.NewValue);
        }

        public LocationSelectorMap()
        {
            InitializeComponent();
        }

        private Location? startLocation;
        private bool isFromCenter;

        private void LocationChanged(LocationSelection? newValue)
        {
            if ( newValue != null )
            {
                var area = newValue.TerrainArea;
                var points = area.TerrainBounds.Shell.Select(area.TerrainPointToLatLng).Select(l => new Location(l.Y, l.X));
                if (points.Any())
                {
                    MapControl.ZoomToBounds(new BoundingBox(points.Min(p => p.Latitude), points.Min(p => p.Longitude), points.Max(p => p.Latitude), points.Max(p => p.Longitude)));
                    RectangleSelected.Locations = points;
                }
                else
                {
                    RectangleSelected.Locations = new List<Location>();
                }
            }
            else
            {
                RectangleSelected.Locations = new List<Location>();
            }
        }

        private void Map_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                startLocation = MapControl.ViewToLocation(e.GetPosition(MapControl));
                isFromCenter = false;
                RectanglePreview.Visibility = Visibility.Visible;
            }
            else if (e.LeftButton == MouseButtonState.Pressed && (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
            {
                startLocation = MapControl.ViewToLocation(e.GetPosition(MapControl));
                isFromCenter = true;
                RectanglePreview.Visibility = Visibility.Visible;
            }
            else
            {
                startLocation = null;
                RectanglePreview.Visibility = Visibility.Collapsed;
            }
        }

        private void Map_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (startLocation != null)
            {
                var area = GetPseudoArea(startLocation, MapControl.ViewToLocation(e.GetPosition(MapControl)), out var center);
                MapSelection = new LocationSelection(FormattableString.Invariant($"{center.Latitude.ToDouble()}, {center.Longitude.ToDouble()}"), true, area);

                startLocation = null;
                RectanglePreview.Visibility = Visibility.Collapsed;
            }
        }

        private void Map_MouseMove(object sender, MouseEventArgs e)
        {
            if (startLocation != null && e.LeftButton == MouseButtonState.Pressed)
            {
                UpdatePreviewRectangle(MapControl.ViewToLocation(e.GetPosition(MapControl)));
            }
        }

        private void UpdatePreviewRectangle(Location secondLocation)
        {
            if (startLocation != null)
            {
                var area = GetPseudoArea(startLocation, secondLocation, out _);

                RectanglePreview.Locations = new List<Location>() {
                    ToLocation(area.TerrainPointToLatLng(new Geometries.TerrainPoint(0,0))),
                    ToLocation(area.TerrainPointToLatLng(new Geometries.TerrainPoint(0,area.SizeInMeters))),
                    ToLocation(area.TerrainPointToLatLng(new Geometries.TerrainPoint(area.SizeInMeters,area.SizeInMeters))),
                    ToLocation(area.TerrainPointToLatLng(new Geometries.TerrainPoint(area.SizeInMeters,0))),
                    ToLocation(area.TerrainPointToLatLng(new Geometries.TerrainPoint(0,0)))
                };
            }
        }

        private Location ToLocation(GeoAPI.Geometries.Coordinate coordinate)
        {
            return new Location(coordinate.Y, coordinate.X);
        }

        private TerrainAreaUTM GetPseudoArea(Location p1, Location p2, out Coordinate center)
        {
            return GetPseudoArea(new Coordinate(p1.Latitude, p1.Longitude, new EagerLoad(false)), new Coordinate(p2.Latitude, p2.Longitude, new EagerLoad(false)), out center);
        }

        private TerrainAreaUTM GetPseudoArea(Coordinate p1, Coordinate p2, out Coordinate center)
        {
            var distance = p1.Get_Distance_From_Coordinate(p2, Shape.Ellipsoid);
            center = new Coordinate(p1.Latitude.ToDouble(), p1.Longitude.ToDouble());
            var bearing = (-distance.Bearing - 90) * Math.PI / 180;
            var size = Math.Max(Math.Abs(Math.Sin(bearing) * distance.Meters), Math.Abs(Math.Cos(bearing) * distance.Meters));
            if (isFromCenter)
            {
                return TerrainAreaUTM.CreateFromCenter(center, 1, (int)size * 2);
            }
            else
            {
                center.Move(p2, distance.Meters / 2, Shape.Ellipsoid);
                return TerrainAreaUTM.CreateFromCenter(center, 1, (int)size);
            }
        }

        private void MapControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            ShellHelper.OpenUri(FormattableString.Invariant($"https://www.openstreetmap.org/#map={Math.Round(MapControl.ZoomLevel)}/{MapControl.Center.Latitude}/{MapControl.Center.Longitude}"));
        }
    }
}
