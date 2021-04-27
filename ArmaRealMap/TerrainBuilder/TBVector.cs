using System.Xml.Serialization;

namespace ArmaRealMap.TerrainBuilder
{
    public class TBVector
    {
        [XmlAttribute]
        public float X { get; set; }

        [XmlAttribute]
        public float Y { get; set; }

        [XmlAttribute]
        public float Z { get; set; }
    }
}