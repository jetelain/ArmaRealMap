using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ArmaRealMap.TerrainBuilder
{
    [XmlRoot("Library")]
    public class TBLibrary
    {
        [XmlElement("Template")]
        public List<TBTemplate> Template { get; set; }

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

        [XmlIgnore]
        public string FullPath { get; set; }

        public string GetAllFilesAsSqfArray()
        {
            return $"['{string.Join("',"+Environment.NewLine+"'", Template.Select(t => t.File))}']";
        }
    }
}
