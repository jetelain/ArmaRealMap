﻿using System.Numerics;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Conditions;
using GameRealisticMap.Geometries;
using GameRealisticMap.Nature;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3.Nature
{
    public abstract class GeneratorBase<TData> : ITerrainBuilderLayerGenerator 
        where TData : class, IPolygonTerrainData
    {
        protected readonly IArma3RegionAssets assets;

        public GeneratorBase(IArma3RegionAssets assets)
        {
            this.assets = assets;
        }

        protected virtual bool ShouldGenerate => true;

        public async Task<IEnumerable<TerrainBuilderObject>> Generate(IArma3MapConfig config, IContext context, IProgressScope progress)
        {
            if (!ShouldGenerate)
            {
                return new List<TerrainBuilderObject>(0);
            }
            var evaluator = context.GetDataAsync<ConditionEvaluator>();

            var polygons = context.GetDataAsync<TData>();

            using var scope = progress.CreateScope(GetType().Name.Replace("Generator", ""));

            var layer = new RadiusPlacedLayer<Composition>(new Vector2(config.SizeInMeters));

            Generate(layer, (await polygons).Polygons, await evaluator, scope);

            return layer.SelectMany(item => item.Model.ToTerrainBuilderObjects(item));
        }

        protected abstract void Generate(RadiusPlacedLayer<Composition> layer, List<TerrainPolygon> polygons, IConditionEvaluator evaluator, IProgressScope scope);
    }
}
