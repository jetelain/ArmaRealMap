using System.Threading.Tasks;
using Caliburn.Micro;

namespace GameRealisticMap.Studio.Modules.Reporting
{
    internal static class ProgressToolHelper
    {
        public static Task? Run(this IProgressTool progressTool, IProcessTask task)
        {
            return progressTool.RunTask(task.Title, task.Run, task.Prompt);
        }

        public static bool Start(this IProgressTool progressTool, IProcessTask task)
        {
            return progressTool.Run(task) != null;
        }

        public static bool Start(IProcessTask task)
        {
            return Run(IoC.Get<IProgressTool>(), task) != null;
        }
    }
}
