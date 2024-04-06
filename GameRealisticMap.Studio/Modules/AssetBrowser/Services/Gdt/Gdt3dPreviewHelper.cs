using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Caliburn.Micro;
using GameRealisticMap.Arma3.GameEngine.Materials;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using HelixToolkit.Wpf;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Services.Gdt
{
    internal class Gdt3dPreviewHelper
    {
        internal static void AddClutter(Model3DGroup group, IEnumerable<ClutterConfig> clutters)
        {
            var clutterPositions = Enumerable.Range(0, 20).Select(_ => new Point3D((Random.Shared.NextDouble() * 8) - 4, 0, (Random.Shared.NextDouble() * 8) - 4));

            foreach (var clutter in clutters)
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

        internal static Model3DGroup CreateBasePreview3d(BitmapSource diffuse, BitmapSource fakeSat)
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

            group.Children.Add(new GeometryModel3D(meshGdt, new DiffuseMaterial(new ImageBrush(ColorFix.ToArma3(diffuse)))));

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
            group.Children.Add(new GeometryModel3D(meshSat, new DiffuseMaterial(new ImageBrush(ColorFix.ToArma3(fakeSat)))));

            return group;
        }
    }
}
