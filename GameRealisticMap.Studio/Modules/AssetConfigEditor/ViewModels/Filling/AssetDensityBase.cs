using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Algorithms.Filling;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Geometries;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.Controls;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling
{
    internal abstract class AssetDensityBase<TId, TDefinition, TItem> : AssetProbabilityBase<TId, TDefinition>, IWithEditableProbability
        where TId : struct, Enum
        where TDefinition : class, IWithDensity, IWithProbability
        where TItem : class, IWithEditableProbability
    {

        public PreviewViewModel PlacementPreview { get; } = new PreviewViewModel();

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
                    return $"{MaxDensity} {Labels.ObjectsPerM2}";
                }
                return string.Format(Labels.RangeObjectsPerM2, MinDensity, MaxDensity);
            }
        }

        public abstract ObservableCollection<TItem> Items { get; }

        public bool IsEmpty => Items.Count == 0;

        protected abstract double GetMaxDensity();

        public string Preview => $"pack://application:,,,/GameRealisticMap.Studio;component/Resources/Areas/{FillId}.png";

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
                generatePreviewInfo = string.Format(Labels.DensityResultsTooHigh, sw.ElapsedMilliseconds * 100d / area, effective / area);
            }
            else
            {
                generatePreviewInfo = string.Format(Labels.DensityResultsOK, sw.ElapsedMilliseconds * 100d / area);
            }
            return (layer.SelectMany(c => c.Model.ToTerrainBuilderObjects(c)).ToList(), generatePreviewInfo);
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
            PlacementPreview.GeneratePreview(GeneratePreviewItems);
            return Task.CompletedTask;
        }

        public Task GenerateFullPreview()
        {
            PlacementPreview.GeneratePreview(GenerateFullPreviewItems);
            return Task.CompletedTask;
        }

        protected virtual List<TerrainBuilderObject> GenerateFullPreviewItems()
        {
            var (obj, status) = GeneratePreviewItems();
            return obj;
        }

        public Task MakeItemsEquiprobable()
        {
            DefinitionHelper.Equiprobable(Items, UndoRedoManager);
            return Task.CompletedTask;
        }
    }
}
