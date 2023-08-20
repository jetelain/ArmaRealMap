using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Geometries;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Nature.Scrubs;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.Controls;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.Views.Filling;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling
{
    internal class FullPreviewGenerator
    {
        public static bool IsForest(FillingAssetBasicViewModel additional)
        {
            return additional.FillId == BasicCollectionId.ForestAdditional;
        }

        public static bool IsForest(FillingAssetClusterViewModel additional)
        {
            return additional.FillId == ClusterCollectionId.Forest || additional.FillId == ClusterCollectionId.ForestEdge || additional.FillId == ClusterCollectionId.ForestRadial;
        }

        public static List<TerrainBuilderObject> Forest(FillingAssetBasicViewModel additional)
        {
            var others = additional.ParentEditor.Filling.OfType<FillingAssetClusterViewModel>();
            return Forest(0, additional,
                others.First(c => c.FillId == ClusterCollectionId.Forest),
                others.First(c => c.FillId == ClusterCollectionId.ForestEdge),
                others.First(c => c.FillId == ClusterCollectionId.ForestRadial));
        }

        public static List<TerrainBuilderObject> Forest(FillingAssetClusterViewModel forestOrEdgeOrRadial)
        {
            var others = forestOrEdgeOrRadial.ParentEditor.Filling.OfType<FillingAssetClusterViewModel>();
            var additional = forestOrEdgeOrRadial.ParentEditor.Filling.OfType<FillingAssetBasicViewModel>().First(c => c.FillId == BasicCollectionId.ForestAdditional);
            var shift = 0f;
            if (forestOrEdgeOrRadial.FillId == ClusterCollectionId.ForestEdge)
            {
                shift = -ForestEdgeData.Width / 2;
            }
            else if (forestOrEdgeOrRadial.FillId == ClusterCollectionId.ForestRadial)
            {
                shift = ForestRadialData.Width / 2;
            }
            return Forest(shift, additional,
                GetOther(forestOrEdgeOrRadial, others, ClusterCollectionId.Forest),
                GetOther(forestOrEdgeOrRadial, others, ClusterCollectionId.ForestEdge),
                GetOther(forestOrEdgeOrRadial, others, ClusterCollectionId.ForestRadial));
        }

        private static FillingAssetClusterViewModel GetOther(FillingAssetClusterViewModel item, IEnumerable<FillingAssetClusterViewModel> others, ClusterCollectionId fillId)
        {
            if (item.FillId == fillId)
            {
                return item;
            }
            return others.First(c => c.FillId == fillId);
        }

        private static FillingAssetBasicViewModel GetOther(FillingAssetBasicViewModel item, IEnumerable<FillingAssetBasicViewModel> others, BasicCollectionId fillId)
        {
            if (item.FillId == fillId)
            {
                return item;
            }
            return others.First(c => c.FillId == fillId);
        }

        public static List<TerrainBuilderObject> Forest(
            float shift,
            FillingAssetBasicViewModel additional,
            FillingAssetClusterViewModel forest,
            FillingAssetClusterViewModel edge,
            FillingAssetClusterViewModel radial)
        {
            var center = (float)PreviewGrid.SizeInMeters / 2 + shift;
            var radialPolygon = TerrainPolygon.FromRectangle(new TerrainPoint(center - ForestRadialData.Width, 0), new TerrainPoint(center, (float)PreviewGrid.SizeInMeters));
            var edgePolygon = TerrainPolygon.FromRectangle(new TerrainPoint(center, 0), new TerrainPoint(center + ForestEdgeData.Width, (float)PreviewGrid.SizeInMeters));
            var forestPolygon = TerrainPolygon.FromRectangle(new TerrainPoint(center, 0), new TerrainPoint((float)PreviewGrid.SizeInMeters, (float)PreviewGrid.SizeInMeters));

            var forestLayer = new RadiusPlacedLayer<Composition>(new Vector2((float)PreviewGrid.SizeInMeters, (float)PreviewGrid.SizeInMeters));
            forest.CreatePreviewGenerator().FillPolygons(forestLayer, new List<TerrainPolygon>() { forestPolygon });
            additional.CreatePreviewGenerator().FillPolygons(forestLayer, new List<TerrainPolygon>() { forestPolygon });

            var edgeLayer = new RadiusPlacedLayer<Composition>(new Vector2((float)PreviewGrid.SizeInMeters, (float)PreviewGrid.SizeInMeters));
            edge.CreatePreviewGenerator().FillPolygons(edgeLayer, new List<TerrainPolygon>() { edgePolygon });

            var radialLayer = new RadiusPlacedLayer<Composition>(new Vector2((float)PreviewGrid.SizeInMeters, (float)PreviewGrid.SizeInMeters));
            radial.CreatePreviewGenerator().FillPolygons(radialLayer, new List<TerrainPolygon>() { radialPolygon });

            return forestLayer.Concat(radialLayer).Concat(edgeLayer).SelectMany(c => c.Model.ToTerrainBuilderObjects(c)).ToList();
        }

        public static bool IsRocks(FillingAssetBasicViewModel additional)
        {
            return additional.FillId == BasicCollectionId.Rocks || additional.FillId == BasicCollectionId.RocksAdditional;
        }

        public static List<TerrainBuilderObject> Rocks(FillingAssetBasicViewModel rocksOrAdditional)
        {
            var others = rocksOrAdditional.ParentEditor.Filling.OfType<FillingAssetBasicViewModel>();
            return Rocks(
                GetOther(rocksOrAdditional, others, BasicCollectionId.RocksAdditional),
                GetOther(rocksOrAdditional, others, BasicCollectionId.Rocks));
        }

        public static List<TerrainBuilderObject> Rocks(
           FillingAssetBasicViewModel additional,
           FillingAssetBasicViewModel rocks)
        {
            var polygon = TerrainPolygon.FromRectangle(new TerrainPoint(0, 0), new TerrainPoint((float)PreviewGrid.SizeInMeters, (float)PreviewGrid.SizeInMeters));

            var layer = new RadiusPlacedLayer<Composition>(new Vector2((float)PreviewGrid.SizeInMeters, (float)PreviewGrid.SizeInMeters));
            rocks.CreatePreviewGenerator().FillPolygons(layer, new List<TerrainPolygon>() { polygon });
            additional.CreatePreviewGenerator().FillPolygons(layer, new List<TerrainPolygon>() { polygon });

            return layer.SelectMany(c => c.Model.ToTerrainBuilderObjects(c)).ToList();
        }

        public static bool IsScrub(FillingAssetBasicViewModel additional)
        {
            return additional.FillId == BasicCollectionId.ScrubAdditional;
        }

        public static bool IsScrub(FillingAssetClusterViewModel additional)
        {
            return additional.FillId == ClusterCollectionId.Scrub || additional.FillId == ClusterCollectionId.ScrubRadial;
        }

        public static List<TerrainBuilderObject> Scrub(FillingAssetClusterViewModel scrubsOrRadial)
        {
            var others = scrubsOrRadial.ParentEditor.Filling.OfType<FillingAssetClusterViewModel>();
            var additional = scrubsOrRadial.ParentEditor.Filling.OfType<FillingAssetBasicViewModel>().First(c => c.FillId == BasicCollectionId.ScrubAdditional);
            var shift = 0f;
            if (scrubsOrRadial.FillId == ClusterCollectionId.ScrubRadial)
            {
                shift = ScrubRadialData.Width / 2;
            }
            return Scrub(shift, additional,
                GetOther(scrubsOrRadial, others, ClusterCollectionId.Scrub),
                GetOther(scrubsOrRadial, others, ClusterCollectionId.ScrubRadial));
        }
        public static List<TerrainBuilderObject> Scrub(FillingAssetBasicViewModel additional)
        {
            var others = additional.ParentEditor.Filling.OfType<FillingAssetClusterViewModel>();
            return Scrub(0, additional,
                others.First(c => c.FillId == ClusterCollectionId.Scrub),
                others.First(c => c.FillId == ClusterCollectionId.ScrubRadial));
        }

        public static List<TerrainBuilderObject> Scrub(
            float shift,
            FillingAssetBasicViewModel additional,
            FillingAssetClusterViewModel scrubs,
            FillingAssetClusterViewModel radial)
        {
            var center = (float)PreviewGrid.SizeInMeters / 2 + shift;
            var radialPolygon = TerrainPolygon.FromRectangle(new TerrainPoint(center - ScrubRadialData.Width, 0), new TerrainPoint(center, (float)PreviewGrid.SizeInMeters));
            var scrubsPolygon = TerrainPolygon.FromRectangle(new TerrainPoint(center, 0), new TerrainPoint((float)PreviewGrid.SizeInMeters, (float)PreviewGrid.SizeInMeters));

            var forestLayer = new RadiusPlacedLayer<Composition>(new Vector2((float)PreviewGrid.SizeInMeters, (float)PreviewGrid.SizeInMeters));
            scrubs.CreatePreviewGenerator().FillPolygons(forestLayer, new List<TerrainPolygon>() { scrubsPolygon });
            additional.CreatePreviewGenerator().FillPolygons(forestLayer, new List<TerrainPolygon>() { scrubsPolygon });

            var radialLayer = new RadiusPlacedLayer<Composition>(new Vector2((float)PreviewGrid.SizeInMeters, (float)PreviewGrid.SizeInMeters));
            radial.CreatePreviewGenerator().FillPolygons(radialLayer, new List<TerrainPolygon>() { radialPolygon });

            return forestLayer.Concat(radialLayer).SelectMany(c => c.Model.ToTerrainBuilderObjects(c)).ToList();
        }
    }
}
