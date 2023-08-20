using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Geometries;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Nature;
using GameRealisticMap.Reporting;
using OsmSharp.Tags;

namespace GameRealisticMap.ManMade.Farmlands
{
    internal class VineyardBuilder : BasicBuilderBase<VineyardData>
    {
        public VineyardBuilder(IProgressSystem progress)
            : base(progress)
        {

        }

        protected override VineyardData CreateWrapper(List<TerrainPolygon> polygons)
        {
            return new VineyardData(polygons);
        }

        protected override bool IsTargeted(TagsCollectionBase tags)
        {
            return tags.GetValue("landuse") == "vineyard";
        }

        protected override IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            return base.GetPriority(context)
                .Concat(context.GetData<ForestData>().Polygons);
        }

        protected override List<TerrainPolygon> MergeIfRequired(List<TerrainPolygon> polygons)
        {
            return polygons; // Do not merge, to be able to place objects at edges and to be able to post-process satellite image
        }
    }
}
