using System;
using System.Threading.Tasks;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.Reporting
{
    /// <summary>
    /// Dockable tool panel that displays the progress of long-running background tasks
    /// (map generation, PBO packaging, etc.). Provides <see cref="StartTask"/> to create
    /// a scoped <see cref="IProgressTaskUI"/> and <see cref="RunTask"/> to execute an
    /// async operation with optional confirmation prompt.
    /// </summary>
    internal interface IProgressTool : ITool
    {
        bool IsRunning { get; }

        IProgressTaskUI StartTask(string name);

        Task? RunTask(string name, Func<IProgressTaskUI, Task> run, bool prompt = true);
    }
}
