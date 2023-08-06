using System.Xml.Serialization;

namespace GameRealisticMap.Arma3.TerrainBuilder.TmlFiles
{
    [XmlRoot("Library")]
    public class TmlLibrary
    {
        [XmlElement("Template")]
        public List<TmlTemplate> Template { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("shape")]
        public string Shape { get; set; } = "rectangle";

        [XmlAttribute("default_fill")]
        public int DefaultFill { get; set; } = -16777216;

        [XmlAttribute("default_outline")]
        public int DefaultOutline { get; set; } = -65536;

        [XmlAttribute("tex")]
        public int Tex { get; set; } = 0;
    }
}
