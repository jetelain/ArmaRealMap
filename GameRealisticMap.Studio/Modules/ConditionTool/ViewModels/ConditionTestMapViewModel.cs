using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Conditions;
using GameRealisticMap.Geometries;
using GameRealisticMap.Osm;
using GameRealisticMap.Studio.Controls;
using GameRealisticMap.Studio.Modules.MapConfigEditor.ViewModels;
using GameRealisticMap.Studio.Modules.Reporting;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.ConditionTool.ViewModels
{
    internal class ConditionTestMapViewModel : Document
    {
        private readonly MapConfigEditorViewModel map;

        private Task? initialize;
        private ConditionEvaluator? conditionEvaluator;
        private IBuildContext? mapData;

        public List<TerrainPoint> IsTrue { get; private set; } = new List<TerrainPoint>();

        public List<TerrainPoint> IsFalse { get; private set; } = new List<TerrainPoint>();

        public IContext? Context => mapData;

        public float SizeInMeters { get; private set; } = 2500;

        public ConditionVM Condition { get; } = new ConditionVM();

        public bool IsWorking { get; set; } = true;

        public bool IsNotWorking => !IsWorking;

        public ConditionTestMapViewModel(MapConfigEditorViewModel map)
        {
            this.map = map;
            DisplayName = string.Format("Testing on {0}", map.FileName);
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            initialize = IoC.Get<IProgressTool>().RunTask("Generate preview for testing", DoGenerateData, false);
            return base.OnInitializeAsync(cancellationToken);
        }

        private async Task DoGenerateData(IProgressTaskUI taskUI)
        {
            var a3config = map.Config.ToArma3MapConfig();

            SizeInMeters = a3config.SizeInMeters;
            NotifyOfPropertyChange(nameof(SizeInMeters));

            var config = await map.GetBuildersConfigSafe(a3config);

            var catalog = new BuildersCatalog(taskUI, config);
            var loader = new OsmDataOverPassLoader(taskUI);
            var osmSource = await loader.Load(a3config.TerrainArea);
            var context = new BuildContext(catalog, taskUI, a3config.TerrainArea, osmSource, new ImageryOptions());
            conditionEvaluator = context.GetData<ConditionEvaluator>();
            GrmMapViewer.EnsureData(context);
            mapData = context;
            NotifyOfPropertyChange(nameof(Context));

            IsWorking = false;
            NotifyOfPropertyChange(nameof(IsWorking));
            NotifyOfPropertyChange(nameof(IsNotWorking));
        }

        internal Task TestCondition(PointCondition condition)
        {
            if (initialize == null)
            {
                return Task.CompletedTask;
            }
            if (!initialize.IsCompleted)
            {
                initialize.ContinueWith(_ => TestCondition(condition));
                return Task.CompletedTask;
            }
            var size = (int)(mapData!.Area.SizeInMeters * 10);
            var count = (int)Math.Min((mapData!.Area.SizeInMeters / 100) * (mapData!.Area.SizeInMeters / 100), 50_000);
            var points = Enumerable.Range(0, count).Select(_ => new TerrainPoint(Random.Shared.Next(0, size) / 10f, Random.Shared.Next(0, size) / 10f)).ToList();
            var isTrue = new List<TerrainPoint>();
            var isFalse = new List<TerrainPoint>();
            var sw = Stopwatch.StartNew();
            foreach (var point in points)
            {
                if (sw.ElapsedMilliseconds > 2000)
                {
                    break;
                }
                if (condition.Evaluate(conditionEvaluator!.GetPointContext(point)))
                {
                    isTrue.Add(point);
                }
                else
                {
                    isFalse.Add(point);
                }
            }
            sw.Stop();
            IsTrue = isTrue;
            IsFalse = isFalse;
            Condition.SetParsedCondition(condition);
            NotifyOfPropertyChange(nameof(IsTrue));
            NotifyOfPropertyChange(nameof(IsFalse));
            return Task.CompletedTask;
        }

        public Task RunCondition()
        {
            if (initialize != null && initialize.IsCompleted)
            {
                var def = Condition.ToDefinition();
                if (def != null)
                {
                    return TestCondition(def);
                }
            }
            return Task.CompletedTask;
        }
    }
}
