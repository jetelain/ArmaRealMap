using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArmaRealMap.ElevationModel;
using ArmaRealMap.TerrainData.GroundDetailTextures;
using BIS.Core.Streams;
using BIS.WRP;
using SixLabors.ImageSharp;

namespace ArmaRealMap
{
    internal class WrpBuilder
    {

        public static void Build(Config config, ElevationGrid elevationGrid, TerrainTiler terrainTiler)
        {
            
            var wrp = new EditableWrp();
            wrp.LandRangeX = 512;
            wrp.LandRangeY = 512;
            wrp.TerrainRangeX = config.GridSize;
            wrp.TerrainRangeY = config.GridSize;
            wrp.CellSize = config.GridSize / 512 * config.CellSize;

            SetElevation(config, elevationGrid, wrp);

            SetMaterials(config, terrainTiler, wrp);

            // NEXT STEP: Process objects !

            wrp.Objects.Add(EditableWrpObject.Dummy);


            Directory.CreateDirectory(config.Target.WorldPhysicalPath);
            StreamHelper.Write(wrp, Path.Combine(config.Target.WorldPhysicalPath, config.Target.WorldName + ".wrp"));

        }

        private static void SetMaterials(Config config, TerrainTiler terrainTiler, EditableWrp wrp)
        {
            wrp.MatNames = new string[terrainTiler.Segments.Length + 1];
            var w = terrainTiler.Segments.GetLength(0);
            var h = terrainTiler.Segments.GetLength(1);
            wrp.MatNames[0] = string.Empty;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    wrp.MatNames[x + (y * h) + 1] = config.Target.GetLayerLogicalPath($"p_{x:000}-{y:000}.rvmat");
                }
            }

            int cellPixelSize = (int)(wrp.CellSize / config.Resolution);
            var half = cellPixelSize / 2;
            wrp.MaterialIndex = new ushort[512 * 512];
            for (int x = 0; x < 512; x++)
            {
                for (int y = 0; y < 512; y++)
                {
                    var p = new Point(x * cellPixelSize + half, (511-y) * cellPixelSize + half);
                    var segment = terrainTiler.All.First(s => s.ContainsImagePoint(p));
                    wrp.MaterialIndex[x + (y * 512)] = (ushort)(segment.X + (segment.Y * h) + 1);
                }
            }
            var dbg = string.Join("\r\n", wrp.MaterialIndex.Select(m => wrp.MatNames[m]));
        }

        private static void SetElevation(Config config, ElevationGrid elevationGrid, EditableWrp wrp)
        {
            wrp.Elevation = new float[config.GridSize * config.GridSize];
            for (int x = 0; x < config.GridSize; x++)
            {
                for (int y = 0; y < config.GridSize; y++)
                {
                    wrp.Elevation[x + (y * config.GridSize)] = elevationGrid.elevationGrid[x, y];
                }
            }
        }

    }
}
