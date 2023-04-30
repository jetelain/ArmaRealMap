using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Osm;
using GameRealisticMap.Reporting;
using OsmSharp.Geo;
using OsmSharp.Tags;

namespace GameRealisticMap.ManMade.Objects
{
    internal class OrientedObjectBuilder : IDataBuilder<OrientedObjectData>
    {
        private readonly float maxDistance = 50f;

        private readonly IProgressSystem progress;

        public OrientedObjectBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public OrientedObjectData Build(IBuildContext context)
        {
            var facing = context.GetData<RoadsData>().Roads.Select(s => s.Path);

            var objects = new List<OrientedObject>();
            foreach (var node in context.OsmSource.Nodes.Where(s => s.Tags != null).ProgressStep(progress,"Objects"))
            {
                var type = GetTypeId(node.Tags);
                if (type != null)
                {
                    var point = context.Area.LatLngToTerrainPoint(node.GetCoordinate());
                    if (context.Area.IsInside(point))
                    {
                        objects.Add(new OrientedObject(point, GetAngle(facing, node.Tags, point), type.Value));
                    }
                }
            }
            return new OrientedObjectData(objects);
        }

        private ObjectTypeId? GetTypeId(TagsCollectionBase tags)
        {
            switch(tags.GetValue("amenity"))
            {
                case "bench":
                    return ObjectTypeId.Bench;

                case "waste_basket":
                    return ObjectTypeId.Basket;

                case "post_box":
                    return ObjectTypeId.PostBox;

                case "recycling":
                    if ( tags.GetValue("recycling_type") == "container")
                    {
                        return ObjectTypeId.RecyclingContainer;
                    }
                    return null;
            }
            if (tags.GetValue("man_made") == "water_well")
            {
                return ObjectTypeId.WaterWell;
            }
            if (tags.GetValue("leisure") == "picnic_table")
            {
                return ObjectTypeId.PicnicTable;
            }
            if (tags.GetValue("emergency") == "fire_hydrant")
            {
                var type = tags.GetValue("fire_hydrant:type");
                if (string.IsNullOrEmpty(type) || type == "pillar")
                {
                    return ObjectTypeId.FireHydrant;
                }
            }
            if (tags.GetValue("information") == "board")
            {
                return ObjectTypeId.InformationBoard;
            }
            switch (tags.GetValue("historic"))
            {
                case "wayside_cross":
                    return ObjectTypeId.WaysideCross;

                case "memorial":
                    switch (tags.GetValue("memorial"))
                    {
                        case "statue":
                            return ObjectTypeId.Statue;

                        case "war_memorial":
                            return ObjectTypeId.WarMemorial;
                    }
                    break;
            }
            return null;
        }

        private float GetAngle(IEnumerable<ITerrainGeo> facing, TagsCollectionBase tags, TerrainPoint point)
        {
            var heading = tags.GetDirection();
            if (heading != null)
            {
                return -heading.Value; // convert to trigonometric because Heading is clockwise
            }
            return GeometryHelper.GetFacing(point, facing, maxDistance) ?? GetRandomAngle(point);
        }

        private float GetRandomAngle(TerrainPoint point)
        {
            return (float)(new Random((int)(point.X + point.Y)).NextDouble() * 360);
        }

    }
}
