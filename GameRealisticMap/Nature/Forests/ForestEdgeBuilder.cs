using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;
using GameRealisticMap.Roads;
using NetTopologySuite.Utilities;

namespace GameRealisticMap.Nature.Forests
{
    internal class ForestEdgeBuilder : IDataBuilder<ForestEdgeData>
    {
        private readonly IProgressSystem progress;

        public ForestEdgeBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public ForestEdgeData Build(IBuildContext context)
        {
            var roads = context.GetData<RoadsData>().Roads;
            var forests = context.GetData<ForestData>().Polygons;

            // Trails are ignored by BasicBuilderBase, but prevents forest edge effect
            var priority = roads
                .Where(r => r.RoadType == RoadTypeId.Trail)
                .SelectMany(r => r.ClearPolygons)
                .ToList();

            using (var step = progress.CreateStep("Merge", 1))
            {
                forests = TerrainPolygon.MergeAll(forests); // XXX: Merge in BasicBuilderBase to rely only on clusters ?
            }

            // XXX: Ignore really small "forest" areas ? 

            var edges = 
                forests.ProgressStep(progress, "Edges")
                    .SelectMany(f => f.InnerCrown(2)) // 2m width offset

                    .ProgressStep(progress, "Priority")
                    .SelectMany(l => l.SubstractAll(priority))

                    .ToList();

            return new ForestEdgeData(edges, forests);
        }
    }
}
