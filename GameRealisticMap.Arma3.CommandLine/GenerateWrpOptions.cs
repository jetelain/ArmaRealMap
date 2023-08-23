using CommandLine;

namespace GameRealisticMap.Arma3.CommandLine
{
    [Verb("genwrp")]
    internal class GenerateWrpOptions : MapOptionsBase
    {
        [Option("skip-paa", HelpText = "Skip PNG to PAA conversion")]
        public bool SkipPaa { get; set; }
    }
}