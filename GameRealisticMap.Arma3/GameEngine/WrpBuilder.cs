using BIS.Core.Streams;
using BIS.WRP;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Reporting;
using SixLabors.ImageSharp;

namespace GameRealisticMap.Arma3.GameEngine
{
    public class WrpBuilder
    {
        private readonly IProgressSystem progress;

        internal const int LandRange = 512; // Make a parameter for this ?

        public WrpBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public void Write(IArma3MapConfig config, ElevationGrid elevationGrid, IEnumerable<EditableWrpObject> objects, string targetFile)
        {
            var terrainTiler = new ImageryTiler(config.TileSize, config.Resolution, config.GetImagerySize());

            var wrp = CreateWorldWithoutObjects(elevationGrid, terrainTiler, config.PboPrefix);

            wrp.Objects.AddRange(objects);

            wrp.Objects.Add(EditableWrpObject.Dummy);

            StreamHelper.Write(wrp, targetFile);
        }

        public EditableWrp CreateWorldWithoutObjects(ElevationGrid elevationGrid, ImageryTiler terrainTiler, string pboPrefix)
        {
            var wrp = InitWrp(elevationGrid);

            SetElevation(elevationGrid, wrp);

            SetMaterials(terrainTiler, wrp, pboPrefix);

            SetMaterialIndexes(terrainTiler, wrp);

            return wrp;
        }

        public static EditableWrp InitWrp(ElevationGrid config)
        {
            var wrp = new EditableWrp();
            wrp.LandRangeX = LandRange;
            wrp.LandRangeY = LandRange;
            wrp.TerrainRangeX = config.Size;
            wrp.TerrainRangeY = config.Size;
            wrp.CellSize = config.Size / LandRange * config.CellSize.X;
            return wrp;
        }

        public void SetMaterials(ImageryTiler terrainTiler, EditableWrp wrp, string pboPrefix)
        {
            wrp.MatNames = new string[terrainTiler.Segments.Length + 1];
            var w = terrainTiler.Segments.GetLength(0);
            var h = terrainTiler.Segments.GetLength(1);
            wrp.MatNames[0] = string.Empty;
            using var report = progress.CreateStep("MatNames", w);
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
            var h = terrainTiler.Segments.GetLength(1);
            using var report = progress.CreateStep("MaterialIndex", LandRange);
            int cellPixelSize = (int)(wrp.CellSize / terrainTiler.Resolution);
            wrp.MaterialIndex = new ushort[LandRange * LandRange];
            for (int x = 0; x < LandRange; x++)
            {
                for (int y = 0; y < LandRange; y++)
                {
                    var p = new Point((x + 1) * cellPixelSize - 1, (LandRange - 1 - y + 1) * cellPixelSize - 1);
                    var segment = terrainTiler.All.First(s => s.ContainsImagePoint(p));
                    wrp.MaterialIndex[x + (y * LandRange)] = (ushort)(segment.X + (segment.Y * h) + 1);
                }
                report.ReportOneDone();
            }
        }

        private void SetElevation(ElevationGrid elevationGrid, EditableWrp wrp)
        {
            var size = elevationGrid.Size;
            using var report = progress.CreateStep("Elevation", size);
            wrp.Elevation = new float[size * size];
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    wrp.Elevation[x + (y * size)] = elevationGrid[x, y];
                }
                report.ReportOneDone();
            }
        }
        
    }
}
