using CommandLine;

namespace GameRealisticMap.Arma3.CommandLine
{
    [Verb("gentb")]
    internal class GenerateTerrainBuilderOptions : MapOptionsBase
    {
        [Option('t', "target", Required = true, HelpText = "Target directory")]
        public string TargetDirectory { get; set; } = string.Empty;
    }
}