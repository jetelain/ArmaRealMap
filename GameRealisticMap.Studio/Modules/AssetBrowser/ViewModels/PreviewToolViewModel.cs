using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using BIS.Core.Streams;
using BIS.P3D.ODOL;
using BIS.P3D;
using Caliburn.Micro;
using GameRealisticMap.Studio.Modules.Arma3Data;
using Gemini.Framework;
using Gemini.Framework.Services;
using BIS.P3D.MLOD;
using System;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels
{
    [Export]
    internal class PreviewToolViewModel : Tool
    {
        public override PaneLocation PreferredLocation => PaneLocation.Right;

        private readonly IArma3DataModule _arma3DataModule;

        [ImportingConstructor]
        public PreviewToolViewModel(IArma3DataModule arma3DataModule)
        {
            _arma3DataModule = arma3DataModule;
            DisplayName = "3D Preview";
        }

        public void SetP3d(string path)
        {
            Model3DGroup = null;


            var p3d = _arma3DataModule.Library.ReadODOL(path);
            if (p3d != null)
            {
                MLOD? mlod = null;
                using (var stream = _arma3DataModule.ProjectDrive.OpenFileIfExists(path))
                {
                    if (P3D.IsMLOD(stream))
                    {
                        mlod = new MLOD(stream);
                    }
                }

                var uri = IoC.Get<IArma3Previews>();

                var lods = p3d.Lods.Where(l => l.FaceCount > 0 && l.Resolution < 1000).OrderBy(l => l.Resolution).ToList();

                var visualLod = lods.FirstOrDefault(l => !l.Materials.Any(m => m.PixelShader == 103)) ?? lods.FirstOrDefault();

                if (visualLod != null)
                {
                    var positions = new Point3DCollection(
                        visualLod.Vertices.Select(v => v.Vector3).Select(v => new Point3D(v.X, v.Y, v.Z)).Concat(

                            new[]{new Point3D(0,0,0),new Point3D(0,0,0)}
                            
                            )  );

                    var group = new Model3DGroup();
                    var uvset = visualLod.UvSets[0].GetUV();
                    var textureCoordinates = new PointCollection(uvset.Select(c => new System.Windows.Point(Clamp(c.X), Clamp(c.Y))).Concat(

                        new[] {new System.Windows.Point(0,0), new System.Windows.Point(1,1)}
                        
                        ));

                    var z = mlod?.Lods[0].Taggs.OfType<UVSetTagg>().FirstOrDefault();

                    var normals = new Vector3DCollection(visualLod.Normals != null ? visualLod.Normals.Select(n => new Vector3D(n.X, n.Y, n.Z)) : visualLod.NormalsCompressed.Select(n => new Vector3D(n.X, n.Y, n.Z)));

                    foreach (var section in visualLod.Sections)
                    {
                        string texture = string.Empty;
                        if (section.TextureIndex != -1)
                        {
                            texture = visualLod.Textures[section.TextureIndex];
                        }
                        var mesh = new MeshGeometry3D();
                        mesh.Positions = positions;
                        mesh.TextureCoordinates = textureCoordinates;
                        //mesh.Normals = normals;
                        mesh.TriangleIndices = new Int32Collection(section.GetFaces(visualLod.Polygons.Faces)
                            .SelectMany(f => f.VertexIndices.Length == 3 ? f.VertexIndices : f.VertexIndices.Take(3).Concat(f.VertexIndices.Skip(2).Concat(f.VertexIndices.Take(1)))));
                       
                        var file = uri.GetTexturePreview(texture);
                        var material = new DiffuseMaterial();
                        material.Brush = file != null ? new ImageBrush(new BitmapImage(file)) { Stretch = Stretch.Fill } : new SolidColorBrush(Colors.DarkRed);
                        var faceModel = new GeometryModel3D(mesh, material);
                        faceModel.BackMaterial = material;
                        group.Children.Add(faceModel);
                    }
                    Model3DGroup = group;
                }
            }
            NotifyOfPropertyChange(nameof(Model3DGroup));

            

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
