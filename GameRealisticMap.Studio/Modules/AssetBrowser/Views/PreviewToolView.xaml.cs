using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Views
{
    /// <summary>
    /// Logique d'interaction pour PreviewToolView.xaml
    /// </summary>
    public partial class PreviewToolView : UserControl
    {
        private Point start;
        private Point3D position;
        private Vector3D lightDirection;

        public PreviewToolView()
        {
            InitializeComponent();   
        }




        //protected override void OnMouseWheel(MouseWheelEventArgs e)
        //{
        //    if (e.Delta > 0)
        //    {
        //        PCamera.Position = PCamera.Position - PCamera.LookDirection;
        //    }
        //    else if (e.Delta < 0)
        //    {
        //        PCamera.Position = PCamera.Position + PCamera.LookDirection;
        //    }
        //    base.OnMouseWheel(e);
        //}

        //protected override void OnMouseDown(MouseButtonEventArgs e)
        //{
        //    start = e.GetPosition(this);
        //    position = PCamera.Position;
        //    lightDirection = DLight.Direction;
        //    CaptureMouse();
        //    base.OnMouseDown(e);
        //}

        //protected override void OnMouseMove(MouseEventArgs e)
        //{
        //    if (IsMouseCaptured)
        //    {
        //        var delta = e.GetPosition(this) - start;
        //        //start = e.GetPosition(this);

        //        if ( e.LeftButton == MouseButtonState.Pressed)
        //        {
        //            var newPosition = new Vector3((float)position.X, (float)position.Y, (float)position.Z);
        //            var newLightDirection = new Vector3((float)lightDirection.X, (float)lightDirection.Y, (float)lightDirection.Z);

        //            newPosition = Vector3.Transform(newPosition, Matrix4x4.CreateRotationY((float)delta.X / 50f));
        //            newLightDirection = Vector3.Transform(newLightDirection, Matrix4x4.CreateRotationY((float)delta.X / 50f));

        //            var direction = Vector3.Normalize(-newPosition);
        //            PCamera.Position = new Point3D(newPosition.X, newPosition.Y, newPosition.Z);
        //            PCamera.LookDirection = new Vector3D(direction.X, direction.Y, direction.Z);
        //            DLight.Direction = new Vector3D(newLightDirection.X, newLightDirection.Y, newLightDirection.Z);
        //        }
        //        else if (e.RightButton == MouseButtonState.Pressed)
        //        {

        //        }
        //    }
        //    base.OnMouseMove(e);
        //}

        //protected override void OnMouseUp(MouseButtonEventArgs e)
        //{
        //    ReleaseMouseCapture();
        //    base.OnMouseUp(e);
        //}



    }
}
