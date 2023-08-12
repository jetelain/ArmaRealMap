using System;
using System.Threading;
using GameRealisticMap.Reporting;
using NLog;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    internal class BasicProgressSystem : ProgressSystemBase, IProgressInteger, IProgressPercent
    {
        private readonly IProgress<double> target;
        private readonly Logger logger;
        private int currentStepTotal = 0;
        private int currentStep = 0;

        public BasicProgressSystem(IProgress<double> target, Logger logger)
        {
            this.target = target;
            this.logger = logger;
        }

        public override IProgressInteger CreateStep(string name, int total)
        {
            currentStepTotal = total;
            return this;
        }

        public override IProgressPercent CreateStepPercent(string name)
        {
            currentStepTotal = 0;
            return this;
        }

        public void Dispose()
        {
            target.Report(100.0);
        }

        public void Report(int value)
        {
            if (currentStepTotal != 0)
            {
                Report(value * 100.0 / currentStepTotal);
            }
        }

        public void Report(double value)
        {
            target.Report(value);
        }

        public void ReportOneDone()
        {
            Interlocked.Increment(ref currentStep);
            Report(currentStep);
        }

        public override void WriteLine(string message)
        {
            logger.Debug(message);
        }
    }
}