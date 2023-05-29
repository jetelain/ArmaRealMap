using System;
using System.Diagnostics;
using Caliburn.Micro;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Studio.Modules.Reporting.ViewModels
{
    internal class ProgressStep : PropertyChangedBase, IProgressInteger, IProgressPercent
    {
        private readonly int total;
        private readonly ProgressTask task;
        private readonly Stopwatch elapsed;
        private readonly Stopwatch lastReport;
        private readonly object locker = new object();
        private int lastDone = 0;

        public ProgressStep(string taskName, int total, ProgressTask task)
        {
            this.total = total;
            this.task = task;
            task.WriteLine($"**** Begin '{taskName}'");
            StepName = taskName;
            elapsed = Stopwatch.StartNew();
            lastReport = Stopwatch.StartNew();
        }

        public int Total => total;

        public double Percent { get; private set; }

        public string Left { get; private set; } = string.Empty;

        public string StepName { get; }

        public void ReportOneDone()
        {
            ++lastDone;
            DrawDone(lastDone);
        }

        public void ReportItemsDone(int done)
        {
            lastDone = done;
            DrawDone(done);
        }

        private void DrawDone(int done)
        {
            if (lastReport.ElapsedMilliseconds > 500)
            {
                lock (locker)
                {
                    if (lastReport.ElapsedMilliseconds > 500)
                    {
                        lastReport.Restart();
                        Percent = done * 100.0 / total;
                        if (done > 0)
                        {
                            var milisecondsLeft = elapsed.ElapsedMilliseconds * (total - done) / done;
                            if (milisecondsLeft > 120000d)
                            {
                                Left = $"{Math.Round(milisecondsLeft / 60000d)} min left";
                            }
                            else
                            {
                                Left = $"{Math.Ceiling(milisecondsLeft / 1000d)} sec left";
                            }
                        }
                        NotifyOfPropertyChange(nameof(Percent));
                        NotifyOfPropertyChange(nameof(Left));
                    }
                }
            }
        }

        public void TaskDone()
        {
            elapsed.Stop();
            task.WriteLine($"** '{StepName}' done in {elapsed.ElapsedMilliseconds} msec");
            if (elapsed.ElapsedMilliseconds < 5000)
            {
                Left = $"Done in {elapsed.ElapsedMilliseconds} msec";
            }
            else
            {
                Left = $"Done in {Math.Ceiling(elapsed.ElapsedMilliseconds / 1000d)} sec";
            }
            Percent = 100.0;
            NotifyOfPropertyChange(nameof(Percent));
            NotifyOfPropertyChange(nameof(Left));
        }

        public void Dispose()
        {
            TaskDone();
        }

        public void Report(int value)
        {
            ReportItemsDone(value);
        }

        public void Report(double value)
        {
            Report((int)(value * 10));
        }
    }
}
