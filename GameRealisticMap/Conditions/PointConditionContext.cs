using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Roads;

namespace GameRealisticMap.Conditions
{
    internal sealed class PointConditionContext : IPointConditionContext
    {
        private readonly ConditionEvaluator evaluator;
        private readonly TerrainPoint point;
        private Road? road;

        public PointConditionContext(ConditionEvaluator evaluator, TerrainPoint point, Road? associatedRoad = null)
        {
            this.evaluator = evaluator;
            this.point = point;
            this.road = associatedRoad;
        }

        public bool IsResidential => evaluator.IsArea(point, BuildingTypeId.Residential);

        public bool IsCommercial => evaluator.IsArea(point, BuildingTypeId.Commercial);

        public bool IsIndustrial => evaluator.IsArea(point, BuildingTypeId.Industrial);

        public bool IsRetail => evaluator.IsArea(point, BuildingTypeId.Retail);

        public bool IsMilitary => evaluator.IsArea(point, BuildingTypeId.Military);

        public bool IsFarmyard => evaluator.IsArea(point, BuildingTypeId.Agricultural);

        public bool IsUrban => evaluator.GetAreas(point).Any();

        public float Elevation => evaluator.GetElevation(point);

        public float Slope => evaluator.GetSlope(point);

        private Road? NearbyRoad => road = road ?? evaluator.GetRoads(point).Where(r => r.RoadType < RoadTypeId.ConcreteFootway).FirstOrDefault();

        public bool IsRoadMotorway => NearbyRoad?.RoadType == RoadTypeId.TwoLanesMotorway;

        public bool IsRoadPrimary => NearbyRoad?.RoadType == RoadTypeId.TwoLanesPrimaryRoad;

        public bool IsRoadSecondary => NearbyRoad?.RoadType == RoadTypeId.TwoLanesSecondaryRoad;

        public bool IsRoadSimple => OneOf(NearbyRoad?.RoadType, RoadTypeId.TwoLanesConcreteRoad, RoadTypeId.SingleLaneConcreteRoad);

        public bool IsRoadPath => OneOf(NearbyRoad?.RoadType, RoadTypeId.SingleLaneDirtPath, RoadTypeId.SingleLaneDirtRoad);

        public float DistanceToRoad => NearbyRoad?.Path.Distance(point) ?? ConditionEvaluator.MaxRoadDistance;

        public bool IsOcean => evaluator.IsOcean(point);

        public float DistanceToOcean => evaluator.DistanceToOcean(point);

        //public string CityType => evaluator.GetCity(point)?.Type.ToString() ?? string.Empty;

        //public float DistanceToCityCenter => evaluator.DistanceToCityCenter(point);

        private static bool OneOf(RoadTypeId? value, RoadTypeId v1, RoadTypeId v2)
        {
            return value == v1 || value == v2;
        }
    }
}
