namespace GameRealisticMap.Studio.Modules.ConditionTool.ViewModels
{
    /// <summary>
    /// Minimal interface exposing the editable condition expression string.
    /// Implemented by <see cref="ConditionVMBase{TCondition,TContext,TGeometry}"/> and
    /// consumed by the condition editor text box binding.
    /// </summary>
    internal interface IConditionVM
    {
        string Condition { get; set; }
    }
}