using CommandLine;
using CommandLine.Text;

namespace ArmaRealMap
{
    internal abstract class OptionsBase
    {
        [Option('g', "global", Required = false, HelpText = "Global configuration file.")]
        public string Global { get; set; }
    }

    [Verb("generate", HelpText = "Generate map data.")]
    internal class GenerateOptions : OptionsBase
    {
        [Value(0, MetaName = "config", HelpText = "JSON configuration file.", Required = true)]
        public string Source { get; set; }
    }

    [Verb("update", HelpText = "Download and update libraries (must be re-uploaded).")]
    internal class UpdateOptions : OptionsBase
    {

    }
}