using System.Numerics;
using System.Text.Json;
using BIS.Core.Streams;
using BIS.P3D;
using BIS.P3D.MLOD;
using BIS.P3D.ODOL;
using ClipperLib;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Roads.Libraries;
using GameRealisticMap.Osm;
using GameRealisticMap.Reporting;
using NetTopologySuite.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using ModelInfo = GameRealisticMap.Arma3.TerrainBuilder.ModelInfo;
using Path = System.IO.Path;
using Polygon = NetTopologySuite.Geometries.Polygon;

namespace GameRealisticMap.Arma3.CommandLine
{
    internal class Program
    {

        /*
        var wrp1 = StreamHelper.Read<EditableWrp>(@"...");
        wrp1.LandRangeX = 1;
        wrp1.LandRangeY = 1;
        wrp1.MatNames = new[] { "-" };
        wrp1.MaterialIndex = new[] { (ushort)1 };
        wrp1.Elevation = new[] { 20f, 20f, 20f, 20f };
        StreamHelper.Write<EditableWrp>(wrp1, @"...");
        var wrp1a = StreamHelper.Read<EditableWrp>(@"...");
        */


        // @"C:\Program Files (x86)\Steam\steamapps\common\Arma 3\Addons"
        //var fs = new PboFileSystem( PboFileSystem.GetArma3Paths(@"C:\Program Files (x86)\Steam\steamapps\common\Arma 3"));
        //var sw = Stopwatch.StartNew();
        //fs.Exists("toto");
        //sw.Stop();

        //static void Process(string path)
        //{
        //    var p3d = StreamHelper.Read<ODOL>(path);
        //    var detected = ObjectPlacementDetectedInfos.CreateFromODOL(p3d);
        //    if (detected == null)
        //    {
        //        Console.WriteLine($"{Path.GetFileNameWithoutExtension(path)} No Geo LOD");
        //        return;
        //    }
        //    var geoLod = p3d.Lods.First(l => l.Resolution == 1E+13f);
        //    var scale = 30f;
        //    var s = MathF.Max(geoLod.Vertices.Select(v => v.Vector3 + p3d.ModelInfo.BoundingCenter.Vector3).Select(v => MathF.Abs(v.X)).Max(),geoLod.Vertices.Select(v => MathF.Abs(v.Z)).Max());
        //    var imageSize = new Vector2((MathF.Ceiling(s) + 1) * 2 * scale);
        //    var center = imageSize / 2;
        //    var image = new Image<Rgba32>((int)imageSize.X, (int)imageSize.Y, Color.Transparent.ToPixel<Rgba32>());
        //    var maxZ = geoLod.Vertices.Max(p => p.Y);
        //    var minZ = geoLod.Vertices.Min(p => p.Y);
        //    image.Mutate(d =>
        //    {
        //        foreach (var face in geoLod.Polygons.Faces.OrderBy(f => f.VertexIndices.Average(i => geoLod.Vertices[i].Vector3.Y)))
        //        {
        //            var points = face.VertexIndices.Select(i => geoLod.Vertices[i]).Select(p => p.Vector3 + p3d.ModelInfo.BoundingCenter.Vector3);
        //            var avgZ = (points.Average(p => p.Y) - minZ) / (maxZ - minZ);

        //            var pb = new PathBuilder();
        //            pb.AddLines(points.Select(p => (PointF)(new Vector2(p.X, p.Z) * scale + center))).CloseFigure();
        //            d.Fill(new SolidBrush(new Color(new Vector4(1- avgZ,1- avgZ,1- avgZ,1))), pb.Build());
        //        }

        //        d.Draw(new Pen(Color.Green, 1), new EllipsePolygon(detected.TrunkRadius.Center * scale + center, detected.TrunkRadius.Radius * scale));
        //        d.Fill(new SolidBrush(Color.Green), new EllipsePolygon(detected.TrunkRadius.Center * scale + center, 2.5f));

        //        d.Draw(new Pen(Color.Blue, 1), new EllipsePolygon(detected.GeneralRadius.Center * scale + center, detected.GeneralRadius.Radius * scale));
        //        d.Fill(new SolidBrush(Color.Blue), new EllipsePolygon(detected.GeneralRadius.Center * scale + center, 2.5f));

        //        d.Draw(new Pen(Color.Red, 1), new RectangleF(detected.Rectangle.Min * scale + center, new SizeF(detected.Rectangle.Size * scale)));
        //        d.Fill(new SolidBrush(Color.Red), new EllipsePolygon(detected.Rectangle.Center * scale + center, 2.5f));
        //    });

        //    image.SaveAsPng(Path.GetFileNameWithoutExtension(path) + ".png");
        //}

        static async Task Main(string[] args)
        {
            var projectDrive = new ProjectDrive(@"d:\Julien\Documents\Arma 3 Projects");

            projectDrive.AddMountPoint(@"z\arm\addons", @"C:\Users\Julien\source\repos\ArmaRealMap\PDrive\z\arm\addons");

            var a3config = new Arma3MapConfigJson()
            {
                SouthWest = "47.6856, 6.8270",
                GridSize = 1024,
                GridCellSize = 2.5f,
                AssetConfigFile = @"C:\Users\Julien\source\repos\ArmaRealMap\ArmToGrmA3\bin\Debug\net6.0\CentralEurope.json",
                TileSize = 512
            }.ToArma3MapConfig();



            var progress = new ConsoleProgressSystem();

            var library = new ModelInfoLibrary(projectDrive);

            var assets = await Arma3Assets.LoadFromFile(library, a3config.AssetConfigFile);

            var catalog = new BuildersCatalog(progress, assets);

            var loader = new OsmDataOverPassLoader(progress);

            var osmSource = await loader.Load(a3config.TerrainArea);

            var context = new BuildContext(catalog, progress, a3config.TerrainArea, osmSource, a3config.Imagery);

            var generator = new Arma3MapGenerator(assets, progress, projectDrive, projectDrive);

            generator.WriteDirectlyWrp(a3config, context, a3config.TerrainArea);

            await projectDrive.ProcessImageToPaa(progress);

            //return;

            //var config = new TestMapConfig();
            //var grid = new ElevationGrid(config.GridSize, config.GridCellSize);
            //for (var ex = 0; ex < grid.Size; ++ex)
            //{
            //    for (var ey = 0; ey < grid.Size; ++ey)
            //    {
            //        if (ex % 4 == 1 || ex % 4 == 2)
            //        {
            //            grid[ex, ey] = 21;
            //        }
            //        else
            //        {
            //            grid[ex, ey] = 20;
            //        }
            //    }
            //}

            //var objects = new List<TerrainBuilderObject>();
            //var model = new ModelInfo("Sign_Arrow_Direction_F", "a3\\misc_f\\Helpers\\Sign_Arrow_Direction_F.p3d", "a3_misc_f", new Vector3(0, 0.058088847f, 0.029880643f));

            ///*var anglesToTest = new float[] { 0, 45, 90, 135, 180, 225, 270, 315 };
            //var pos = 1;
            //foreach (var yaw in anglesToTest)
            //{
            //    foreach (var pitch in anglesToTest)
            //    {
            //        foreach (var roll in anglesToTest)
            //        {
            //            objects.Add(new TerrainBuilderObject(model, new TerrainPoint(pos, 1), 0, ElevationMode.Relative, yaw, pitch, roll, 1));
            //            pos++;
            //        }
            //    }
            //}*/

            // p:\z\arm\addons\arm_testworld

            //var progress = new ConsoleProgressSystem();
            //var projectDrive = new ProjectDrive();
            //projectDrive.AddMountPoint(@"z\arm\addons", @"C:\Users\Julien\source\repos\ArmaRealMap\PDrive\z\arm\addons");

            //var materials = new TestTerrainMaterialLibrary();

            //var tiler = new ImageryTiler(config.TileSize, config.Resolution, config.GetImagerySize());

            //var targetPath = @"C:\Users\Julien\source\repos\ArmaRealMap\PDrive\z\arm\addons\arm_testworld\data\layers";
            //Directory.CreateDirectory(targetPath);

            //var targetId = new Image<Rgba32>(tiler.TileSize, tiler.TileSize, Color.Black.ToPixel<Rgba32>());
            //var targetSat = new Image<Rgba32>(tiler.TileSize, tiler.TileSize, Color.Gray.ToPixel<Rgba32>());


            //DrawRectangle(targetId, x: 064, y: 064, w: 224, h: 112, c: new Rgba32(0, 0, 0, 255));
            //DrawRectangle(targetId, x: 288, y: 064, w: 224, h: 112, c: new Rgba32(255, 0, 0, 255));
            //DrawRectangle(targetId, x: 512, y: 064, w: 224, h: 112, c: new Rgba32(0, 255, 0, 255));
            //DrawRectangle(targetId, x: 736, y: 064, w: 224, h: 112, c: new Rgba32(0, 0, 255, 255));

            //DrawRectangle(targetId, x: 064, y: 176, w: 224, h: 112, c: new Rgba32(0, 0, 0, 128));
            //DrawRectangle(targetId, x: 288, y: 176, w: 224, h: 112, c: new Rgba32(255, 0, 0, 128));
            //DrawRectangle(targetId, x: 512, y: 176, w: 224, h: 112, c: new Rgba32(0, 255, 0, 128));
            //DrawRectangle(targetId, x: 736, y: 176, w: 224, h: 112, c: new Rgba32(0, 0, 255, 128));

            //DrawRectangle(targetId, x: 064, y: 288, w: 224, h: 112, c: new Rgba32(0, 0, 0, 255));
            //DrawRectangle(targetId, x: 288, y: 288, w: 224, h: 112, c: new Rgba32(255, 0, 0, 255));
            //DrawRectangle(targetId, x: 512, y: 288, w: 224, h: 112, c: new Rgba32(0, 255, 0, 255));
            //DrawRectangle(targetId, x: 736, y: 288, w: 224, h: 112, c: new Rgba32(0, 0, 255, 255));

            //DrawRectangle(targetId, x: 064, y: 400, w: 224, h: 112, c: new Rgba32(0, 0, 0, 0));
            //DrawRectangle(targetId, x: 288, y: 400, w: 224, h: 112, c: new Rgba32(255, 0, 0, 0));
            //DrawRectangle(targetId, x: 512, y: 400, w: 224, h: 112, c: new Rgba32(0, 255, 0, 0));
            //DrawRectangle(targetId, x: 736, y: 400, w: 224, h: 112, c: new Rgba32(0, 0, 255, 0));

            //DrawRectangle(targetId, x: 064, y: 512, w: 224, h: 112, c: new Rgba32(0, 0, 0, 128));
            //DrawRectangle(targetId, x: 288, y: 512, w: 224, h: 112, c: new Rgba32(255, 0, 0, 128));
            //DrawRectangle(targetId, x: 512, y: 512, w: 224, h: 112, c: new Rgba32(0, 255, 0, 128));
            //DrawRectangle(targetId, x: 736, y: 512, w: 224, h: 112, c: new Rgba32(0, 0, 255, 128));

            //DrawRectangle(targetId, x: 064, y: 624, w: 224, h: 112, c: new Rgba32(0, 0, 0, 255));
            //DrawRectangle(targetId, x: 288, y: 624, w: 224, h: 112, c: new Rgba32(255, 0, 0, 255));
            //DrawRectangle(targetId, x: 512, y: 624, w: 224, h: 112, c: new Rgba32(0, 255, 0, 255));
            //DrawRectangle(targetId, x: 736, y: 624, w: 224, h: 112, c: new Rgba32(0, 0, 255, 255));


            //foreach (var tile in tiler.All)
            //{
            //    var rvmat = ImageryCompiler.MakeRvMat(tile, config, materials.Materials);
            //    File.WriteAllText(Path.Combine(targetPath, $"P_{tile.X:000}-{tile.Y:000}.rvmat"), rvmat);
            //    targetId.Save(Path.Combine(targetPath, $"m_{tile.X:000}_{tile.Y:000}_lca.png"));
            //    targetSat.Save(Path.Combine(targetPath, $"s_{tile.X:000}_{tile.Y:000}_lco.png"));
            //}

            ////var objects = new List<TerrainBuilderObject>();

            ////var model = new ModelInfo("Sign_Arrow_Direction_F", "a3\\misc_f\\Helpers\\Sign_Arrow_Direction_F.p3d", "a3_misc_f", new Vector3(0, 0.058088847f, 0.029880643f));
            //var model2 = new ModelInfo("Sign_Arrow_F", "a3\\misc_f\\Helpers\\Sign_Arrow_F.p3d", "a3_misc_f", new Vector3(0, 0.3732606f, 0));
            //// BoundingCenter	{0,0.3732606,0}
            ///*
            //var anglesToTest = new float[] { 0, 45, 90, 135, 180, 225, 270, 315 };
            //var pos = 1;
            //foreach(var yaw in anglesToTest)
            //{
            //    foreach (var pitch in anglesToTest)
            //    {
            //        foreach (var roll in anglesToTest)
            //        {
            //            objects.Add(new TerrainBuilderObject(model, new TerrainPoint(pos, 1), 0, ElevationMode.Relative, yaw, pitch, roll, 1));
            //            pos++;
            //        }
            //    }
            //}*/

            //var result = string.Join("\r\n", objects.Select(o => o.ToTerrainBuilderCSV()));


            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model, new TerrainPoint(2, 3), -0,   0));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model, new TerrainPoint(3, 3), -45,  0));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model, new TerrainPoint(3, 2), -90,  0));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model, new TerrainPoint(3, 1), -135, 0));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model, new TerrainPoint(2, 1), -180, 0));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model, new TerrainPoint(1, 1), -225, 0));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model, new TerrainPoint(1, 2), -270, 0));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model, new TerrainPoint(1, 3), -315, 0));

            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model2, new TerrainPoint(6, 3), -0,   45));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model2, new TerrainPoint(7, 3), -45,  45));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model2, new TerrainPoint(7, 2), -90,  45));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model2, new TerrainPoint(7, 1), -135, 45));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model2, new TerrainPoint(6, 1), -180, 45));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model2, new TerrainPoint(5, 1), -225, 45));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model2, new TerrainPoint(5, 2), -270, 45));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model2, new TerrainPoint(5, 3), -315, 45));

            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model2, new TerrainPoint(10, 3), -0,   90));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model2, new TerrainPoint(11, 3), -45,  90));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model2, new TerrainPoint(11, 2), -90,  90));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model2, new TerrainPoint(11, 1), -135, 90));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model2, new TerrainPoint(10, 1), -180, 90));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model2, new TerrainPoint(9, 1),  -225, 90));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model2, new TerrainPoint(9, 2),  -270, 90));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model2, new TerrainPoint(9, 3),  -315, 90));

            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model2, new TerrainPoint(14, 3), -0,   -45));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model2, new TerrainPoint(15, 3), -45,  -45));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model2, new TerrainPoint(15, 2), -90,  -45));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model2, new TerrainPoint(15, 1), -135, -45));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model2, new TerrainPoint(14, 1), -180, -45));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model2, new TerrainPoint(13, 1), -225, -45));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model2, new TerrainPoint(13, 2), -270, -45));
            //objects.Add(TerrainBuilderObject.RelativePitchThenYaw(model2, new TerrainPoint(13, 3), -315, -45));

            //var wrpBuilder = new WrpCompiler(progress, projectDrive);

            //wrpBuilder.Write(config, grid, objects.Select((obj,idx) => obj.ToWrpObject(grid)));


        }


        //private static void DrawRectangle(Image<Rgba32> targetId, int x, int y, int w, int h, Rgba32 c)
        //{
        //    for(var dx = 0;dx < w; dx++)
        //    {
        //        for (var dy = 0; dy < h; dy++)
        //        {
        //            targetId[x+dx, y+dy] = c;
        //        }
        //    }
        //}
    }
}