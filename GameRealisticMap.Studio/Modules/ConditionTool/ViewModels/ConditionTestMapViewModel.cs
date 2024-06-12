using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Conditions;
using GameRealisticMap.Geometries;
using GameRealisticMap.Osm;
using GameRealisticMap.Studio.Modules.Main.Services;
using GameRealisticMap.Studio.Modules.MapConfigEditor.ViewModels;
using GameRealisticMap.Studio.Modules.Reporting;
using GameRealisticMap.Studio.Shared;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Modules.ConditionTool.ViewModels
{
    internal class ConditionTestMapViewModel : Document
    {
        private readonly MapConfigEditorViewModel map;

        private IConditionToolWorker? worker;

        private Task? initialize;
        private ConditionEvaluator? conditionEvaluator;
        private IBuildContext? mapData;

        public List<ITerrainEnvelope> IsTrue { get; private set; } = new List<ITerrainEnvelope>();

        public List<ITerrainEnvelope> IsFalse { get; private set; } = new List<ITerrainEnvelope>();

        public PreviewMapData? PreviewMapData { get; set; }

        public float SizeInMeters { get; private set; } = 250;

        public List<ConditionToken> Tokens => worker?.Tokens ?? new List<ConditionToken>();

        public bool IsWorking { get; set; } = true;

        public bool IsNotWorking => !IsWorking;

        public string Stats { get; set; } = string.Empty;

        public MapConfigEditorViewModel Map => map;

        public ConditionTestMapViewModel(MapConfigEditorViewModel map)
        {
            this.map = map;
            DisplayName = string.Format(GameRealisticMap.Studio.Labels.TagsTester, map.FileName);
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            initialize = IoC.Get<IProgressTool>().RunTask(GameRealisticMap.Studio.Labels.GeneratePreview, DoGenerateData, false);
            return base.OnInitializeAsync(cancellationToken);
        }

        private async Task DoGenerateData(IProgressTaskUI taskUI)
        {
            var a3config = map.Config.ToArma3MapConfig();

            SizeInMeters = a3config.SizeInMeters;
            NotifyOfPropertyChange(nameof(SizeInMeters));

            var config = await map.GetBuildersConfigSafe(a3config);

            var sources = IoC.Get<IGrmConfigService>().GetSources();
            var catalog = new BuildersCatalog(taskUI, config, sources);
            var loader = new OsmDataOverPassLoader(taskUI, sources);
            var osmSource = await loader.Load(a3config.TerrainArea);
            var context = new BuildContext(catalog, taskUI, a3config.TerrainArea, osmSource, new MapProcessingOptions());
            conditionEvaluator = context.GetData<ConditionEvaluator>();
            PreviewMapData = new PreviewMapData(context);
            mapData = context;
            NotifyOfPropertyChange(nameof(PreviewMapData));

            IsWorking = false;
            NotifyOfPropertyChange(nameof(IsWorking));
            NotifyOfPropertyChange(nameof(IsNotWorking));

            OnUIThread(() => IoC.Get<IShell>().ShowTool<IConditionTool>());
        }

        internal Task TestCondition2<TCondition, TContext, TGeometry>(ConditionToolWorker<TCondition, TContext, TGeometry> worker, IConditionSampleProvider<TGeometry> provider)
            where TCondition : class, ICondition<TContext>
            where TContext : IConditionContext<TGeometry>
            where TGeometry : ITerrainEnvelope
        {
            if (initialize == null)
            {
                return Task.CompletedTask;
            }
            if (!initialize.IsCompleted)
            {
                initialize.ContinueWith(_ => TestCondition2(worker, provider));
                return Task.CompletedTask;
            }
            this.worker = worker;
            var points = provider.GetSamplePoints(mapData!);
            var isTrue = new List<ITerrainEnvelope>();
            var isFalse = new List<ITerrainEnvelope>();
            var sw = Stopwatch.StartNew();
            foreach (var point in points)
            {
                if (sw.ElapsedMilliseconds > 2000)
                {
                    break;
                }
                if (worker.Evaluate(conditionEvaluator!, point))
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

            var total = isTrue.Count + isFalse.Count;
            Stats = string.Format(Labels.TagsTesterStats, total, isTrue.Count * 100.0 / total, (double)sw.ElapsedMilliseconds / total);

            NotifyOfPropertyChange(nameof(IsTrue));
            NotifyOfPropertyChange(nameof(IsFalse));
            NotifyOfPropertyChange(nameof(Stats));
            NotifyOfPropertyChange(nameof(Tokens));
            return Task.CompletedTask;
        }

        public Task RunRandom()
        {
            if (initialize != null && initialize.IsCompleted && worker != null)
            {
                return worker.TestOnMapRandom(this);
            }
            return Task.CompletedTask;
        }

        public Task RunViewport(ITerrainEnvelope envelope)
        {
            if (initialize != null && initialize.IsCompleted && worker != null)
            {
                return worker.TestOnMapViewport(this, envelope);
            }
            return Task.CompletedTask;
        }
    }
}
