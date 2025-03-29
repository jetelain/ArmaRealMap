using System.Numerics;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;

namespace GameRealisticMap.Arma3.Edit.Imagery.Generic
{
    internal class TerrainRvMatInfos
    {
        internal static readonly Regex ShaderMatch = new Regex(@"PixelShaderID\s*=\s*""TerrainX"";", RegexOptions.CultureInvariant);

        internal static readonly Regex SatMatch = new Regex(@"class Stage0[\s\S]*texture\s*=\s*""([^""]*)"";\s*texGen\s*=\s*3;", RegexOptions.CultureInvariant | RegexOptions.Multiline);

        internal static readonly Regex MaskMatch = new Regex(@"class Stage1[\s\S]*texture\s*=\s*""([^""]*)"";\s*texGen\s*=\s*4;", RegexOptions.CultureInvariant | RegexOptions.Multiline);

        internal static readonly Regex UvTransformMatch = new Regex(@"aside\[\]\s*=\s*\{\s*(?<UA>[\d\.\-]+),\s*0,\s*0\s*\};\s*up\[\]\s*=\s*\{\s*0,\s*0,\s*(?<UA2>[\d\.\-]+)\s*\};\s*dir\[\]\s*=\s*\{\s*0,\s*(?<UANeg>[\d\.\-]+),\s*0\s*\};\s*pos\[\]\s*=\s*\{\s*(?<UB>[\d\.\-]+),\s*(?<VB>[\d\.\-]+),\s*0\s*\};", RegexOptions.CultureInvariant | RegexOptions.Multiline);

        public required string Sat { get; internal set; }
        public required string Mask { get; internal set; }
        public required List<string> Textures { get; internal set; }
        public required List<string> Normals { get; internal set; }
        public List<Point> CellIndexes { get; internal set; } = new List<Point>();
        public float UA { get; internal set; }
        public float UB { get; internal set; }
        public float VB { get; internal set; }
    }
}