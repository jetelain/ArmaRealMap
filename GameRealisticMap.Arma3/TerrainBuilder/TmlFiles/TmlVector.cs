using System.Xml.Serialization;

namespace GameRealisticMap.Arma3.TerrainBuilder.TmlFiles
{
    public class TmlVector
    {
        [XmlAttribute]
        public float X { get; set; }

        [XmlAttribute]
        public float Y { get; set; }

        [XmlAttribute]
        public float Z { get; set; }
    }
}