﻿using GameRealisticMap.Geometries;
using GameRealisticMap.Nature;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Reporting;
using OsmSharp.Tags;

namespace GameRealisticMap.ManMade.Farmlands
{
    internal class FarmlandsBuilder : BasicBuilderBase<FarmlandsData>
    {
        public FarmlandsBuilder(IProgressSystem progress)
            : base(progress)
        {

        }

        protected override FarmlandsData CreateWrapper(List<TerrainPolygon> polygons)
        {
            return new FarmlandsData(polygons);
        }

        protected override bool IsTargeted(TagsCollectionBase tags)
        {
            return tags.GetValue("landuse") == "farmland" && tags.GetValue("crop") != "grass";
            // Grass is processed as a Meadow
        }

        protected override IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            return base.GetPriority(context)
                .Concat(context.GetData<VineyardData>().Polygons)
                .Concat(context.GetData<OrchardData>().Polygons)
                .Concat(context.GetData<ForestData>().Polygons);
        }

        protected override List<TerrainPolygon> MergeIfRequired(List<TerrainPolygon> polygons)
        {
            return polygons; // Do not merge, to be able to place objects at edges and to be able to post-process satellite image
        }
    }
}
