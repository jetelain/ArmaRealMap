using System.Threading.Tasks;

namespace GameRealisticMap.Studio.Modules.Reporting
{
    internal interface IProcessTask
    {
        string Title { get; }

        bool Prompt { get; }

        Task Run(IProgressTaskUI ui);
    }
}
