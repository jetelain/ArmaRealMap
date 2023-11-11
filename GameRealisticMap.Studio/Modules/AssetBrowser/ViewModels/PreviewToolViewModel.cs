using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using BIS.P3D.ODOL;
using Caliburn.Micro;
using GameRealisticMap.Studio.Modules.Arma3Data;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels
{
    [Export]
    internal class PreviewToolViewModel : Tool
    {
        public override PaneLocation PreferredLocation => PaneLocation.Right;

        private readonly IArma3DataModule _arma3DataModule;
        private readonly IArma3Previews _arma3Previews;

        [ImportingConstructor]
        public PreviewToolViewModel(IArma3DataModule arma3DataModule, IArma3Previews arma3Previews)
        {
            _arma3DataModule = arma3DataModule;
            _arma3Previews = arma3Previews;
            DisplayName = "3D Preview";
        }

        public void SetP3d(string path)
        {
            Model3DGroup = null;

            var p3d = _arma3DataModule.Library.ReadODOL(path);
            if (p3d != null)
            {
                var lods = p3d.Lods.Where(l => l.FaceCount > 0 && l.Resolution < 1000).OrderBy(l => l.Resolution).ToList();
                var visualLod = lods.FirstOrDefault(l => !l.Materials.Any(m => m.PixelShader == 103)) ?? lods.FirstOrDefault();
                if (visualLod != null)
                {
                    var positions = GetPositions(visualLod);
                    var textureCoordinates = GetTextureCoordinates(visualLod);
                    var group = new Model3DGroup();
                    //var normals = new Vector3DCollection(visualLod.Normals != null ? visualLod.Normals.Select(n => new Vector3D(n.X, n.Y, n.Z)) : visualLod.NormalsCompressed.Select(n => new Vector3D(n.X, n.Y, n.Z)));

                    foreach (var section in visualLod.Sections)
                    {
                        var material = GetMaterial(visualLod, section);
                        var mesh = new MeshGeometry3D();
                        mesh.Positions = positions;
                        mesh.TextureCoordinates = textureCoordinates;
                        // mesh.Normals = normals;
                        mesh.TriangleIndices = new Int32Collection(section.GetFaces(visualLod.Polygons.Faces).SelectMany(GetTriangeIndices));
                        var faceModel = new GeometryModel3D(mesh, material) { BackMaterial = material };
                        group.Children.Add(faceModel);
                    }
                    Model3DGroup = group;
                }
            }
            NotifyOfPropertyChange(nameof(Model3DGroup));
        }

        private static IEnumerable<int> GetTriangeIndices(Polygon f)
        {
            if (f.VertexIndices.Length == 3)
            {
                return f.VertexIndices;
            }
            return f.VertexIndices.Take(3).Concat(f.VertexIndices.Skip(2).Concat(f.VertexIndices.Take(1)));
        }

        private static Point3DCollection GetPositions(LOD visualLod)
        {
            return new Point3DCollection(
                visualLod.Vertices.Select(v => v.Vector3).Select(v => new Point3D(v.X, v.Y, v.Z)).Concat(
                    new[] { new Point3D(0, 0, 0), new Point3D(0, 0, 0) }
                    ));
        }

        private PointCollection GetTextureCoordinates(LOD visualLod)
        {
            var uvset = visualLod.UvSets[0].GetUV();
            var textureCoordinates = new PointCollection(uvset.Select(c => new System.Windows.Point(Clamp(c.X), Clamp(c.Y))).Concat(
                new[] { new System.Windows.Point(0, 0), new System.Windows.Point(1, 1) }
                ));
            return textureCoordinates;
        }

        private Material GetMaterial(LOD visualLod, Section section)
        {
            string texture = string.Empty;
            if (section.TextureIndex != -1)
            {
                texture = visualLod.Textures[section.TextureIndex];
            }
            var file = _arma3Previews.GetTexturePreview(texture);
            if (file != null)
            {
                return new DiffuseMaterial(new ImageBrush(new BitmapImage(file)) { Stretch = Stretch.Fill }); 
            }
            return new DiffuseMaterial(new SolidColorBrush(Colors.DarkRed));
        }

        private double Clamp(float v)
        {
            if ( v >= 0 )
            {
                return v % 1f;
            }
            return (v % 1f) + 1f;
        }

        public Model3DGroup? Model3DGroup {  get; set; }

    }
}
