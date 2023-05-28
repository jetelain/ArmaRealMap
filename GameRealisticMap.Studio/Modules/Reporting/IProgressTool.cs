using GameRealisticMap.Reporting;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.Reporting
{
    internal interface IProgressTool : ITool
    {
        bool IsRunning { get; }

        IProgressTaskUI StartTask(string name);
    }
}
