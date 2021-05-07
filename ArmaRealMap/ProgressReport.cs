using System;
using System.Diagnostics;

namespace ArmaRealMap
{
    public class ProgressReport
    {
        private readonly string taskName;
        private readonly int itemsToDo;
        private readonly Stopwatch sw;
        private readonly Stopwatch lastReport;
        private readonly int top;
        private readonly object locker = new object();
        private int lastDone = 0;

        public ProgressReport(string taskName, int itemsToDo)
        {
            this.taskName = taskName;
            this.itemsToDo = itemsToDo;
            this.sw = Stopwatch.StartNew();
            this.lastReport = Stopwatch.StartNew();

            Console.Write(taskName);
            this.top = Console.CursorTop;
            WritePercent(0);
        }

        public int Total
        {
            get { return itemsToDo; }
        }

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
                        WritePercent(done * 100.0 / itemsToDo);

                        if (done > 0)
                        {
                            var milisecondsLeft = sw.ElapsedMilliseconds * (itemsToDo - done) / done;
                            if (milisecondsLeft > 120000d)
                            {
                                Console.Write($"{Math.Round(milisecondsLeft / 60000d)} min left");
                            }
                            else
                            {
                                Console.Write($"{Math.Ceiling(milisecondsLeft / 1000d)} sec left");
                            }
                        }
                        CleanEndOfLine();
                    }
                }
            }
        }

        private void WritePercent(double percent)
        {
            var cols = Math.Max(0,Math.Min(20,(int)(percent / 5)));
            Console.SetCursorPosition(20, top);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(new string('#', cols));
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(new string('-', 20 - cols));
            Console.Write(' ');
            Console.Write($"{percent,6:0.00} % ");
        }

        public void TaskDone()
        {
            sw.Stop();
            WritePercent(100d);
            Console.Write($"Done in {Math.Ceiling(sw.ElapsedMilliseconds / 1000d)} sec");
            CleanEndOfLine();
        }

        private static void CleanEndOfLine()
        {
            Console.WriteLine(new string(' ', Console.BufferWidth - Console.CursorLeft - 1));
        }
    }
}
