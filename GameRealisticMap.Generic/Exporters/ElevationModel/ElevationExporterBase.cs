using System.Numerics;
using GameRealisticMap.ElevationModel;
using Pmad.Cartography.DataCells.FileFormats;

namespace GameRealisticMap.Generic.Exporters.ElevationModel
{
    internal abstract class ElevationExporterBase : IExporter
    {
        public abstract string Name { get; }

        protected abstract ElevationGrid GetDataCell(IBuildContext context);

        public IEnumerable<ExportFormat> Formats => [ExportFormat.EsriAscii, ExportFormat.DemDataCell, ExportFormat.WavefrontObj];

        public Task Export(string filename, ExportFormat format, IBuildContext context, IDictionary<string, object>? properties)
        {
            switch (format)
            {
                case ExportFormat.EsriAscii:
                    return ExportEsriAscii(context, filename);
                case ExportFormat.WavefrontObj:
                    return ExportWavefrontObj(context, filename, properties);
                case ExportFormat.DemDataCell:
                    return ExportDdc(context, filename);
            }
            return Task.FromException(new ApplicationException($"Format '{format}' is not supported by '{Name}'"));
        }

        private async Task ExportWavefrontObj(IBuildContext context, string filename, IDictionary<string, object>? properties)
        {
            var cell = GetDataCell(context);

            var materialFile = Path.ChangeExtension(filename, ".mtl");

            await WriteMaterial(materialFile);

            await WriteObj(filename, cell, materialFile);
        }

        private static async Task WriteObj(string filename, ElevationGrid cell, string materialFile)
        {
            using var writer = File.CreateText(filename);
            
            // Center, and offset elevation
            var offsetZ = -Math.Max(0, cell.ToDataCell().Data.Cast<float>().Min());
            var offsetXY = cell.SizeInMeters / -2;

            await writer.WriteLineAsync("# elevation");
            await writer.WriteLineAsync($"mtllib {Path.GetFileName(materialFile)}");
            await writer.WriteLineAsync("usemtl satellitetexture");

            var vertices = new int[cell.Size, cell.Size];
            var index = 1; // Obj indexes starts at 1
            var textureSpace = new Vector2(cell.Size, -cell.Size);
            var textureOffset = new Vector2(0, 1);
            for (int x = 0; x < cell.Size; x++)
            {
                for (int y = 0; y < cell.Size; y++)
                {
                    var gridPoint = new Vector2(x, y);
                    var vertice = (gridPoint * cell.CellSize.X) + offsetXY;
                    var normal = cell.NormalAtGrid(gridPoint);
                    var texture = textureOffset + (gridPoint / textureSpace);
                    var z = cell[x, y] + offsetZ;
                    await writer.WriteLineAsync(FormattableString.Invariant($"v {vertice.X:F2} {vertice.Y:F2} {z:F3}"));
                    await writer.WriteLineAsync(FormattableString.Invariant($"vt {texture.X:F7} {texture.Y:F7}"));
                    await writer.WriteLineAsync(FormattableString.Invariant($"vn {normal.X:F7} {normal.Y:F7} {normal.Z:F7}"));
                    vertices[x, y] = index;
                    index++;
                }
            }

            for (int x = 0; x < cell.Size - 1; x++)
            {
                for (int y = 0; y < cell.Size - 1; y++)
                {
                    //   N
                    //   ^. .. .. 
                    //   | \| \|
                    // 1 b--c--+-..
                    //   |\ |\ |\
                    //   | \| \| .
                    // 0 a--d--+--> E
                    //   0  1  2
                    var a = vertices[x, y];
                    var b = vertices[x, y + 1];
                    var c = vertices[x + 1, y + 1];
                    var d = vertices[x + 1, y];

                    await writer.WriteLineAsync(FormattableString.Invariant($"f {a}/{a}/{a} {d}/{d}/{d} {b}/{b}/{b}"));
                    await writer.WriteLineAsync(FormattableString.Invariant($"f {d}/{d}/{d} {c}/{c}/{c} {b}/{b}/{b}"));
                }
            }
        }

        private async Task WriteMaterial(string materialFile)
        {
            using var writer = File.CreateText(materialFile);
            await writer.WriteLineAsync("newmtl satellitetexture");
            await writer.WriteLineAsync("  Ka 1.000 1.000 1.000");
            await writer.WriteLineAsync("  Kd 1.000 1.000 1.000");
            await writer.WriteLineAsync("  map_Ka satellite.png");
            await writer.WriteLineAsync("  map_Kd satellite.png");
        }

        private Task ExportDdc(IBuildContext context, string filename)
        {
            var cell = GetDataCell(context);
            cell.ToDataCell().Save(filename);
            return Task.CompletedTask;
        }

        private Task ExportEsriAscii(IBuildContext context, string filename)
        {
            var cell = GetDataCell(context);
            using var writer = File.CreateText(filename);
            EsriAsciiHelper.SaveDataCell(writer, cell.ToDataCell());
            return Task.CompletedTask;
        }
    }
}
