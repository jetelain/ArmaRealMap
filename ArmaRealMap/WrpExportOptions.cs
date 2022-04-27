using CommandLine;
using CommandLine.Text;

namespace ArmaRealMap
{
    [Verb("wrp-export", HelpText = "Export WRP content to text files for easy change tracking.")]
    internal class WrpExportOptions : OptionsBase
    {
        [Value(0, MetaName = "wrp", HelpText = "WRP file.", Required = true)]
        public string Source { get; set; }

        [Value(1, MetaName = "folder", HelpText = "Target folder.", Required = true)]
        public string Target { get; set; }
    }
}