using CommandLine;

namespace GameRealisticMap.Arma3.CommandLine
{
    [Verb("layer")]
    internal class GenerateObjectLayerOptions : MapOptionsBase
    {
        [Option('l', "layer", Required = true, HelpText = "Requested layer")]
        public string LayerName { get; set; } = string.Empty;

        [Option('t', "target", Required = true, HelpText = "Target directpry")]
        public string TargetDirectory { get; set; } = string.Empty;
    }
}
