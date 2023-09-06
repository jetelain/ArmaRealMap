using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Roads;

namespace GameRealisticMap.Conditions
{
    public class ConditionOnPointEvaluation
    {
        private readonly ConditionEvaluator evaluator;
        private readonly TerrainPoint point;
        private readonly Road? road;

        public ConditionOnPointEvaluation(ConditionEvaluator evaluator, TerrainPoint point, Road? associatedRoad = null) 
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

        public bool IsAgricultural => evaluator.IsArea(point, BuildingTypeId.Agricultural);

        public bool IsNonUrban => evaluator.GetAreas(point).Count() == 0;

        public bool IsUrban => evaluator.GetAreas(point).Any();

        public string Area => string.Join(",", evaluator.GetAreas(point));

        public float Elevation => evaluator.GetElevation(point);

        public float Slope => evaluator.GetSlope(point);

        public string NearbyRoad => (road ?? evaluator.GetRoad(point))?.RoadType.ToString() ?? string.Empty;

        public bool IsOcean => evaluator.IsOcean(point);

        public float DistanceToOcean => evaluator.DistanceToOcean(point);

        public string CityType => evaluator.GetCity(point)?.Type.ToString() ?? string.Empty;

        public float DistanceToCityCenter => evaluator.DistanceToCityCenter(point);
    }
}
