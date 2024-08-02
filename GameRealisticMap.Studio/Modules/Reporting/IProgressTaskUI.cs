using System;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Studio.Modules.Reporting
{
    internal interface IProgressTaskUI : IDisposable
    {
        IProgressScope Scope { get; }

        void AddSuccessAction(Action action, string label, string description = "");
    }
}
