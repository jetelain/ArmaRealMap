using Gemini.Modules.Output;
using NLog;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Studio.Modules.Reporting.ViewModels
{
    public class GrmProgressRender : WpfProgressRender2
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

        protected override void WriteLine(ProgressItemViewModel2 progressItemViewModel, string message)
        {
            base.WriteLine(progressItemViewModel, message);
            output.AppendLine(message);
        }

    }
}
