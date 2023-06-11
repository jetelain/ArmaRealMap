using System;
using System.Threading.Tasks;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.Reporting
{
    internal interface IProgressTool : ITool
    {
        bool IsRunning { get; }

        IProgressTaskUI StartTask(string name);

        void RunTask(string name, Func<IProgressTaskUI, Task> run);
    }
}
