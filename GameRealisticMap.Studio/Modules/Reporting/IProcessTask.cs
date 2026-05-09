using System.Threading.Tasks;

namespace GameRealisticMap.Studio.Modules.Reporting
{
    /// <summary>
    /// Encapsulates a named async operation that runs inside the progress tool.
    /// Implement <see cref="Run"/> to execute the work and update <see cref="IProgressTaskUI"/>.
    /// </summary>
    internal interface IProcessTask
    {
        string Title { get; }

        bool Prompt { get; }

        Task Run(IProgressTaskUI ui);
    }
}
