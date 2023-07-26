﻿using System.Numerics;
using GameRealisticMap.Algorithms.Following;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Railways;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.ManMade
{
    internal class RailwayGenerator : ITerrainBuilderLayerGenerator
    {
        private readonly IProgressSystem progress;
        private readonly RailwaysDefinition? assets;

        public RailwayGenerator(IProgressSystem progress, IArma3RegionAssets assets)
        {
            this.progress = progress;
            this.assets = assets.Railways;
        }

        public IEnumerable<TerrainBuilderObject> Generate(IArma3MapConfig config, IContext context)
        {
            if (assets == null || assets.Straights.Count == 0)
            {
                return Enumerable.Empty<TerrainBuilderObject>();
            }
            var railways = context.GetData<RailwaysData>().Railways;
            var layer = new List<PlacedModel<Composition>>();
            foreach (var segment in railways.ProgressStep(progress, "Railways"))
            {
                if ( segment.SpecialSegment == GameRealisticMap.ManMade.WaySpecialSegment.Crossing)
                {
                    var length = segment.Path.Length;
                    var crossing = assets.Crossings.OrderBy(c => Math.Abs(length - c.Size)).FirstOrDefault();
                    if (crossing != null)
                    {
                        // By construction length and Size should be very close
                        var center = new TerrainPoint((segment.Path.FirstPoint.Vector + segment.Path.LastPoint.Vector) / 2);
                        var delta = Vector2.Normalize(segment.Path.FirstPoint.Vector - segment.Path.LastPoint.Vector);
                        var angle = (90f + (MathF.Atan2(delta.Y, delta.X) * 180 / MathF.PI)) % 360f;
                        layer.Add(new PlacedModel<Composition>(crossing.Model, center, angle));
                    }
                }
                else
                {
                    FollowPathWithObjects.PlaceOnPath(assets.Straights, layer, segment.Path.Points);
                }
            }
            return layer.SelectMany(o => o.Model.ToTerrainBuilderObjects(o));
        }
    }
}