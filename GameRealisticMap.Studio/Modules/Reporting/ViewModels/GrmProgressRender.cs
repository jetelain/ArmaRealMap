using System.Linq;
using Gemini.Modules.Output;
using NLog;
using Pmad.ProgressTracking;
using Pmad.ProgressTracking.Wpf;

namespace GameRealisticMap.Studio.Modules.Reporting.ViewModels
{
    /// <summary>
    /// WPF progress renderer for GRM tasks. Extends <c>WpfProgressRender</c> from Pmad
    /// to also echo task start/end lines to the Gemini Output panel and NLog logger,
    /// providing a persistent text log alongside the live progress bar UI.
    /// </summary>
    public class GrmProgressRender : WpfProgressRender
    {
        private static readonly Logger logger = LogManager.GetLogger("Task");
        private readonly IOutput output;

        public GrmProgressRender(IOutput output)
        {
            this.output = output;
        }

        public override void Started(ProgressScope progressScope, ProgressBase item)
        {
            WriteLine($"**** Begin '{item.Name}'");
            base.Started(progressScope, item);
        }

        public override void Finished(ProgressBase progressBase)
        {
            WriteLine($"** '{progressBase.Name}' done in {progressBase.Elapsed}");
            base.Finished(progressBase);
        }

        public override void WriteLine(ProgressBase progressBase, string message)
        {
            logger.Debug(message);
            base.WriteLine(progressBase, message);
        }

        protected override void WriteLine(ProgressItemViewModel progressItemViewModel, string message)
        {
            base.WriteLine(progressItemViewModel, message);
            output.AppendLine(message);
        }
    }
}
