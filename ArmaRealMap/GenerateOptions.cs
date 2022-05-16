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

        [Option("no-elevation", Required = false, HelpText = "Do not generate elevation files, keep existing.")]
        public bool DoNotGenerateElevation { get; set; }

        [Option("no-objects", Required = false, HelpText = "Do not generate (most) objects files, keep existing.")]
        public bool DoNotGenerateObjects { get; set; }

        [Option("no-imagery", Required = false, HelpText = "Do not generate imagery files, keep existing.")]
        public bool DoNotGenerateImagery { get; set; }

        [Option("no-overpass", Required = false, HelpText = "Do not update OpenStreeMap data from overpass, keep existing.")]
        public bool DoNotUpdateOSM { get; set; }

        [Option("pack", Required = false, HelpText = "Generate a ZIP file with pre-cooked data.")]
        public string Pack { get; set; }

        [Option("mod", Required = false, HelpText = "Generate PBO to specified mod.")]
        public string TargetMod { get; set; }

        [Option("non-interactive", Required = false, HelpText = "Mikero's tool will not pause, preventing reading errors, but allowing scripting.")]
        public bool NonInterractive { get; set; }
    }

    [Verb("update", HelpText = "Download and update libraries (must be re-uploaded).")]
    internal class UpdateOptions : OptionsBase
    {

    }

    [Verb("show-config", HelpText = "Show configuration.")]
    internal class ShowConfigOptions : OptionsBase
    {
        [Value(0, MetaName = "config", HelpText = "JSON configuration file.", Required = false)]
        public string Source { get; set; }
    }

    [Verb("topaa", HelpText = "Mass convert files to PAA.")]
    internal class ConvertToPaaOptions : OptionsBase
    {
        [Value(0, MetaName = "dir", HelpText = "Directory containing files to convert.", Required = true)]
        public string Directory { get; set; }

        [Option("max-threads", Required = false, HelpText = "Maximum number of threads used.")]
        public int? MaxThreads { get; set; }
    }
}