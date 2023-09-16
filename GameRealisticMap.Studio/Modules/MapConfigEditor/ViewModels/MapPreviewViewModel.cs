using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Fences;
using GameRealisticMap.ManMade.Objects;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Nature;
using GameRealisticMap.Nature.Lakes;
using GameRealisticMap.Nature.Trees;
using GameRealisticMap.Osm;
using GameRealisticMap.Reporting;
using GameRealisticMap.Studio.Modules.Reporting;
using GameRealisticMap.Studio.Shared;
using GameRealisticMap.Studio.Toolkit;
using Gemini.Framework;
using GeoAPI.Geometries;

namespace GameRealisticMap.Studio.Modules.MapConfigEditor.ViewModels
{
    internal class MapPreviewViewModel : Document
    {
        private readonly MapConfigEditorViewModel map;

        public PreviewMapData? PreviewMapData { get; set; }

        private IBuildContext? mapData;

        public float SizeInMeters { get; private set; } = 2500;

        public bool IsWorking { get; set; } = true;

        public bool IsNotWorking => !IsWorking;

        public MapConfigEditorViewModel Map => map;

        public List<OptionalPreviewLayerVM> Optionals { get; }

        public MapPreviewViewModel(MapConfigEditorViewModel map)
        {
            this.map = map;
            DisplayName = string.Format(GameRealisticMap.Studio.Labels.PreviewTitle, map.FileName);

            Optionals = new List<OptionalPreviewLayerVM>()
            {
                new OptionalPreviewLayerVM(this, "Sidewalks", ctx => ctx.GetData<SidewalksData>().Paths),

                new OptionalPreviewLayerVM(this, "Fences", ctx => ctx.GetData<FencesData>().Fences.Select(f => f.Path).ToList()),

                new OptionalPreviewLayerVM(this, "StreetLamps", 
                    ctx => ctx.GetData<ProceduralStreetLampsData>().Objects.Cast<IOrientedObject>()
                                .Concat(ctx.GetData<OrientedObjectData>().Objects.Where(o => o.TypeId == ObjectTypeId.StreetLamp)).Select(p => p.Point).ToList()),

                new OptionalPreviewLayerVM(this, "Trees", ctx => ctx.GetData<TreesData>().Points),

                new OptionalPreviewLayerVM(this, "TreeRows", ctx => ctx.GetData<TreeRowsData>().Rows),
                
                new OptionalPreviewLayerVM(this, "Objects",
                    ctx => ctx.GetData<OrientedObjectData>().Objects.Where(o => o.TypeId != ObjectTypeId.StreetLamp).Select(p => p.Point).ToList()),

                new OptionalPreviewLayerVM(this, "Contours", ctx => ctx.GetData<ElevationContourData>().Contours)
            };

            var builders = new BuildersCatalog(new NoProgressSystem(), new DefaultBuildersConfig());
            Optionals.AddRange(builders.VisitAll(new Visitor(this)).Where(o => o != null).Cast<OptionalPreviewLayerVM>());

        }

        internal class Visitor : IDataBuilderVisitor<OptionalPreviewLayerVM?>
        {
            private static readonly List<Type> Ignore = typeof(PreviewMapData).GetProperties().Select(p => p.PropertyType).Concat(new[] { typeof(LakesData) }).ToList();

            private readonly MapPreviewViewModel preview;

            public Visitor(MapPreviewViewModel preview)
            {
                this.preview = preview;
            }

            public OptionalPreviewLayerVM? Visit<TData>(IDataBuilder<TData> builder) where TData : class
            {
                if (typeof(IPolygonTerrainData).IsAssignableFrom(typeof(TData)) && !Ignore.Contains(typeof(TData)))
                {
                    return new OptionalPreviewLayerVM(preview, builder.GetType().Name.Replace("Builder",""), ctx => ((IPolygonTerrainData)ctx.GetData<TData>()).Polygons);
                }
                return null;
            }
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            IoC.Get<IProgressTool>().RunTask(GameRealisticMap.Studio.Labels.GeneratePreview, DoGenerateData, false);
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
            PreviewMapData = new PreviewMapData(context);
            mapData = context;
            NotifyOfPropertyChange(nameof(PreviewMapData));
            IsWorking = false;
            NotifyOfPropertyChange(nameof(IsWorking));
            NotifyOfPropertyChange(nameof(IsNotWorking));
        }

        public void AddOptionalLayer(string name, Func<IContext, PreviewAdditionalLayer> generate)
        {
            var data = PreviewMapData;
            if (data != null && mapData != null)
            {
                if (!data.Additional.Any(a => a.Name == name))
                {
                    Task.Run(() => DoAddional(mapData, data, generate));
                }
            }
        }

        private void DoAddional(IContext context, PreviewMapData data, Func<IContext, PreviewAdditionalLayer> generate)
        {
            IsWorking = true;
            NotifyOfPropertyChange(nameof(IsWorking));
            NotifyOfPropertyChange(nameof(IsNotWorking));

            PreviewMapData = new PreviewMapData(context, data.Additional.Concat(new[] { generate(context) }));
            NotifyOfPropertyChange(nameof(PreviewMapData)); 

            IsWorking = false;
            NotifyOfPropertyChange(nameof(IsWorking));
            NotifyOfPropertyChange(nameof(IsNotWorking));
        }

        internal void RemoveOptionalLayer(string name)
        {
            var data = PreviewMapData;
            if (data != null && mapData != null)
            {
                PreviewMapData = new PreviewMapData(mapData, data.Additional.Where(a => a.Name != name));
                NotifyOfPropertyChange(nameof(PreviewMapData));
            }
        }
        private string GetPosition(ITerrainArea area, ITerrainEnvelope terrainEnvelope)
        {
            var center = (terrainEnvelope.MaxPoint.Vector + terrainEnvelope.MinPoint.Vector) / 2;
            var pos = area.TerrainPointToLatLng(new TerrainPoint(center));
            return FormattableString.Invariant($"17/{pos.Y}/{pos.X}");
        }

        internal void ViewOSM(ITerrainEnvelope terrainEnvelope)
        {
            if (mapData != null)
            {
                ShellHelper.OpenUri("https://www.openstreetmap.org/#map="+ GetPosition(mapData.Area, terrainEnvelope));
            }
        }

        internal void EditOSM(ITerrainEnvelope terrainEnvelope)
        {
            if (mapData != null)
            {
                ShellHelper.OpenUri("https://www.openstreetmap.org/edit#map=" + GetPosition(mapData.Area, terrainEnvelope));
            }
        }

        private void DoAllAddional(IContext context)
        {
            IsWorking = true;
            NotifyOfPropertyChange(nameof(IsWorking));
            NotifyOfPropertyChange(nameof(IsNotWorking));

            PreviewMapData = new PreviewMapData(context, Optionals.Select(o => o.Generate(context)));
            NotifyOfPropertyChange(nameof(PreviewMapData));

            IsWorking = false;
            NotifyOfPropertyChange(nameof(IsWorking));
            NotifyOfPropertyChange(nameof(IsNotWorking));

            foreach(var opt in Optionals)
            {
                opt.SetActualEnabled(true);
            }
        }

        public Task EnableAll()
        {
            if (mapData != null)
            {
                Task.Run(() => DoAllAddional(mapData));
            }
            return Task.CompletedTask;
        }
    }
}
