using GameRealisticMap.Algorithms;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Osm;
using OsmSharp.Geo;
using OsmSharp.Tags;
using Pmad.ProgressTracking;

namespace GameRealisticMap.ManMade.Objects
{
    internal class OrientedObjectBuilder : IDataBuilder<OrientedObjectData>
    {
        private readonly float maxDistance = 50f;

        public OrientedObjectData Build(IBuildContext context, IProgressScope scope)
        {
            var roads = context.GetData<RoadsData>().Roads;
            var facing = roads.Select(s => s.Path);

            var objects = new List<OrientedObject>();
            foreach (var node in context.OsmSource.Nodes.Where(s => s.Tags != null).WithProgress(scope, "Objects"))
            {
                var type = GetTypeId(node.Tags);
                if (type != null)
                {
                    var point = context.Area.LatLngToTerrainPoint(node.GetCoordinate());
                    if (context.Area.IsInside(point))
                    {
                        var angle = GetAngle(facing, node.Tags, point, out var facingPath);
                        var road = facingPath != null ? roads.FirstOrDefault(r => r.Path ==  facingPath) : null;
                        objects.Add(new OrientedObject(point, angle, type.Value, road));
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
            switch (tags.GetValue("man_made"))
            {
                case "water_well":
                    return ObjectTypeId.WaterWell;

                case "flagpole":
                    return ObjectTypeId.Flagpole;
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
            if (tags.GetValue("artwork_type") == "sculpture")
            {
                return ObjectTypeId.Sculpture;
            }
            if (tags.GetValue("highway") == "street_lamp")
            {
                return ObjectTypeId.StreetLamp;
            }
            return null;
        }

        private float GetAngle(IEnumerable<ITerrainGeo> facing, TagsCollectionBase tags, TerrainPoint point, out ITerrainGeo? closest)
        {
            var heading = tags.GetDirection();
            if (heading != null)
            {
                closest = null;
                return -heading.Value; // convert to trigonometric because Heading is clockwise
            }
            return GeometryHelper.GetFacing(point, facing, out closest, maxDistance) ?? GetRandomAngle(point);
        }

        internal static float GetRandomAngle(TerrainPoint point)
        {
            return (float)(RandomHelper.CreateRandom(point).NextDouble() * 360);
        }

    }
}
