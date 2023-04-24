using GameRealisticMap.Geometries;

namespace GameRealisticMap.ManMade.Objects
{
    public class OrientedObject
    {
        public OrientedObject(TerrainPoint point, float angle, ObjectTypeId objectTypeId)
        {
            Point = point;
            Angle = angle;
            TypeId = objectTypeId;
        }

        public TerrainPoint Point { get; }

        public float Angle { get; }

        public ObjectTypeId TypeId { get; }

        public float Heading => -Angle;
    }
}
