using System;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Studio.Modules.Reporting
{
    /// <summary>
    /// UI handle for a single running background task. Exposes the root
    /// <see cref="Pmad.ProgressTracking.IProgressScope"/> used to report sub-steps,
    /// allows registering post-completion actions, and signals task completion.
    /// </summary>
    internal interface IProgressTaskUI
    {
        IProgressScope Scope { get; }

        void AddSuccessAction(Action action, string label, string description = "");

        void Done();
    }
}
