using BIS.Core.Streams;
using BIS.WRP;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Reporting;
using SixLabors.ImageSharp;

namespace GameRealisticMap.Arma3.GameEngine
{
    public class WrpCompiler
    {
        private readonly IProgressSystem progress;
        private readonly IGameFileSystemWriter fileSystemWriter;

        public WrpCompiler(IProgressSystem progress, IGameFileSystemWriter fileSystemWriter)
        {
            this.progress = progress;
            this.fileSystemWriter = fileSystemWriter;
        }

        public void Write(IArma3MapConfig config, ElevationGrid elevationGrid, IEnumerable<EditableWrpObject> objects)
        {
            Write(config, elevationGrid, new ImageryTiler(config), objects);
        }

        public void Write(IArma3MapConfig config, ElevationGrid elevationGrid, ImageryTiler terrainTiler, IEnumerable<EditableWrpObject> objects)
        {
            var wrp = CreateWorldWithoutObjects(elevationGrid, terrainTiler, config.PboPrefix);

            foreach(var obj in objects)
            {
                obj.ObjectID = wrp.Objects.Count + 1;
                wrp.Objects.Add(obj);
            }

            wrp.Objects.Add(EditableWrpObject.Dummy);

            using (var stream = fileSystemWriter.Create($"{config.PboPrefix}\\{config.WorldName}.wrp"))
            {
                StreamHelper.Write(wrp, stream);
            }
        }

        public EditableWrp CreateWorldWithoutObjects(ElevationGrid elevationGrid, ImageryTiler terrainTiler, string pboPrefix)
        {
            var wrp = InitWrp(elevationGrid, LandRange(elevationGrid.SizeInMeters.X));

            SetElevation(elevationGrid, wrp);

            SetMaterialAndIndexes(terrainTiler, wrp, pboPrefix);

            return wrp;
        }

        public void SetMaterialAndIndexes(ImageryTiler terrainTiler, EditableWrp wrp, string pboPrefix)
        {
            SetMaterials(terrainTiler, wrp, pboPrefix);

            SetMaterialIndexes(terrainTiler, wrp);
        }

        public static EditableWrp InitWrp(IElevationGridConfig config, int landRange)
        {
            var wrp = new EditableWrp();
            wrp.LandRangeX = landRange;
            wrp.LandRangeY = landRange;
            wrp.TerrainRangeX = config.Size;
            wrp.TerrainRangeY = config.Size;
            wrp.CellSize = config.Size * config.CellSize.X / landRange;
            return wrp;
        }

        public static int LandRange(float sizeInMeters)
        {
            if (sizeInMeters > 30720)
            {
                return 1024;
            }
            if (sizeInMeters > 15360)
            {
                return 512;
            }
            if (sizeInMeters > 7680)
            {
                return 256;
            }
            return 128;
        }

        private void SetMaterials(ImageryTiler terrainTiler, EditableWrp wrp, string pboPrefix)
        {
            wrp.MatNames = new string[terrainTiler.Segments.Length + 1];
            var w = terrainTiler.Segments.GetLength(0);
            var h = terrainTiler.Segments.GetLength(1);
            wrp.MatNames[0] = string.Empty;
            using var report = progress.CreateStep("MaterialNames", w);
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    wrp.MatNames[x + (y * h) + 1] = $"{pboPrefix}\\data\\layers\\p_{x:000}-{y:000}.rvmat";
                }
                report.ReportOneDone();
            }
        }

        private void SetMaterialIndexes(ImageryTiler terrainTiler, EditableWrp wrp)
        {
            var landRange = wrp.LandRangeX;
            var h = terrainTiler.Segments.GetLength(1);
            int cellPixelSize = (int)(wrp.CellSize / terrainTiler.Resolution);
            wrp.MaterialIndex = new ushort[landRange * landRange];
            using var report = progress.CreateStep("MaterialIndex", landRange);
            for (int x = 0; x < landRange; x++)
            {
                for (int y = 0; y < landRange; y++)
                {
                    var p = new Point((x + 1) * cellPixelSize - 1, (landRange - 1 - y + 1) * cellPixelSize - 1);
                    var segment = terrainTiler.All.First(s => s.ContainsImagePoint(p));
                    wrp.MaterialIndex[x + (y * landRange)] = (ushort)(segment.X + (segment.Y * h) + 1);
                }
                report.ReportOneDone();
            }
        }

        private void SetElevation(ElevationGrid elevationGrid, EditableWrp wrp)
        {
            var size = elevationGrid.Size;
            wrp.Elevation = new float[size * size];
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    wrp.Elevation[x + (y * size)] = elevationGrid[x, y];
                }
            }
        }
        
    }
}
