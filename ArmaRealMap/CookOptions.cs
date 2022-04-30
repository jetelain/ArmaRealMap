using CommandLine;

namespace ArmaRealMap
{
    [Verb("cook", HelpText = "Take pre-cooked mad data, binarize and generate a PBO (with mikero's tools).")]
    internal class CookOptions : OptionsBase
    {
        [Value(0, MetaName = "pack", HelpText = "Package generated.", Required = true)]
        public string Pack { get; set; }

        [Value(1, MetaName = "mod", HelpText = "Target mod.", Required = false)]
        public string TargetMod { get; set; }

        [Option("max-threads", Required = false, HelpText = "Maximum number of threads used.")]
        public int? MaxThreads { get; set; }

        [Option("non-interactive", Required = false, HelpText = "Mikero's tool will not pause, preventing reading errors, but allowing scripting.")]
        public bool NonInterractive { get; set; }
    }
}