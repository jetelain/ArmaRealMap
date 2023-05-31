using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Algorithms.Filling;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Geometries;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.Views.Filling;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling
{
    internal abstract class AssetDensityBase<TId, TDefinition> : AssetProbabilityBase<TId, TDefinition>, IWithEditableProbability
        where TId : struct, Enum
        where TDefinition : class, IWithDensity, IWithProbability
    {

        protected AssetDensityBase(TId id, TDefinition? definition, AssetConfigEditorViewModel parent)
            : base(id, definition, parent)
        {
            _minDensity = definition?.MinDensity ?? 0.01;
            _maxDensity = definition?.MaxDensity ?? 0.01;
        }

        private double _minDensity;
        public double MinDensity
        {
            get { return _minDensity; }
            set { _minDensity = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(DensityText)); }
        }

        private double _maxDensity;
        public double MaxDensity
        {
            get { return _maxDensity; }
            set { _maxDensity = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(DensityText)); }
        }

        public double? ComputedMaxDensity { get; set; }

        public string DensityText
        {
            get
            {
                if (MaxDensity == MinDensity)
                {
                    return $"{MaxDensity} objects/m²";
                }
                return $"{MinDensity} to {MaxDensity} objects/m²";
            }
        }

        protected abstract double GetMaxDensity();

        public string Preview => $"pack://application:,,,/GameRealisticMap.Studio;component/Resources/Areas/{FillId}.png";

        public List<PreviewItem> PreviewItems { get; private set; } = new List<PreviewItem>();

        private bool isGeneratingPreview;
        public bool IsGeneratingPreview { get { return isGeneratingPreview; } private set { isGeneratingPreview = value; NotifyOfPropertyChange(); } }

        private string generatePreviewStatus = string.Empty;
        public string GeneratePreviewStatus { get { return generatePreviewStatus; } private set { generatePreviewStatus = value; NotifyOfPropertyChange(); } }

        public Task ComputeMaxDensity()
        {
            var max = Math.Round(GetMaxDensity(), 4);
            ComputedMaxDensity = max;
            NotifyOfPropertyChange(nameof(ComputedMaxDensity));
            return Task.CompletedTask;
        }

        private (List<TerrainBuilderObject>,string) GeneratePreviewItems()
        {
            var generator = CreatePreviewGenerator();
            var layer = new RadiusPlacedLayer<Composition>(new Vector2((float)PreviewGrid.SizeInMeters, (float)PreviewGrid.SizeInMeters));
            var surface = CreatePreviewBox();
            var sw = Stopwatch.StartNew();
            var wanted = generator.FillPolygons(layer, new List<TerrainPolygon>() { surface });
            sw.Stop();
            var effective = layer.Count;
            var area = surface.Area;
            string generatePreviewInfo;
            if ( effective < wanted )
            {
                generatePreviewInfo = $"{sw.ElapsedMilliseconds * 100d / area:0.00} msec/100 m². Density is too high, should reduce it to {effective / area}";
            }
            else
            {
                generatePreviewInfo = $"{sw.ElapsedMilliseconds * 100d / area:0.00} msec/100 m². Density is OK.";
            }
            return (layer.SelectMany(c => c.Model.ToTerrainBuilderObjects(c)).ToList(), generatePreviewInfo);
        }

        protected void SetPreview(List<TerrainBuilderObject> objects, string status = "")
        {
            var helper = IoC.Get<IArma3DataModule>().ModelPreviewHelper;
            GeneratePreviewStatus = "Loading objects shapes...";
            PreviewItems =
                objects.SelectMany(o => helper.ToVisualPolygons(o).Select(p => new PreviewItem(p, o.Model, o.Scale, true)))
                .Concat(objects.SelectMany(o => helper.ToPolygons(o).Select(p => new PreviewItem(p, o.Model, o.Scale, false))))
                .ToList();
            GeneratePreviewStatus = status;
            IsGeneratingPreview = false;
            NotifyOfPropertyChange(nameof(PreviewItems));
        }

        private TerrainPolygon CreatePreviewBox()
        {
            var width = GetPreviewWidth();
            return TerrainPolygon.FromRectangle(new TerrainPoint((float)(PreviewGrid.SizeInMeters - width) / 2, 0), new TerrainPoint((float)(PreviewGrid.SizeInMeters + width) / 2, (float)PreviewGrid.SizeInMeters));
        }

        public abstract FillAreaBase<Composition> CreatePreviewGenerator();

        public virtual double GetPreviewWidth()
        {
            return PreviewGrid.SizeInMeters;
        }

        public Task GeneratePreview()
        {
            if (!IsGeneratingPreview)
            {
                GeneratePreviewStatus = "Generating preview...";
                IsGeneratingPreview = true;
                Task.Run(() => { var (obj, status) = GeneratePreviewItems(); SetPreview(obj, status); });
            }
            return Task.CompletedTask;
        }

        public Task GenerateFullPreview()
        {
            if (!IsGeneratingPreview)
            {
                GeneratePreviewStatus = "Generating preview...";
                IsGeneratingPreview = true;
                Task.Run(() => SetPreview(GenerateFullPreviewItems()));
            }
            return Task.CompletedTask;
        }

        protected virtual List<TerrainBuilderObject> GenerateFullPreviewItems()
        {
            var (obj, status) = GeneratePreviewItems();
            return obj;
        }
    }
}
