using GameRealisticMap.Studio.Modules.ConditionTool.ViewModels;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.ConditionTool
{
    internal interface IConditionTool : ITool
    {
        ConditionVM? Target { get; set; }
    }
}
