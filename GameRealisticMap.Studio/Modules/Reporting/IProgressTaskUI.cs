using System;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Studio.Modules.Reporting
{
    internal interface IProgressTaskUI
    {
        IProgressScope Scope { get; }

        void AddSuccessAction(Action action, string label, string description = "");

        void Done();
    }
}
