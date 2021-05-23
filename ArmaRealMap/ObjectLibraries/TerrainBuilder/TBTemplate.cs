using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ArmaRealMap.TerrainBuilder
{
    public class TBTemplate
    {
        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public string File { get; set; }
        [XmlElement]
        public TBVector BoundingMin { get; set; }
        [XmlElement]
        public TBVector BoundingMax { get; set; }
        [XmlElement]
        public TBVector BoundingCenter { get; set; }
        [XmlElement]
        public float Height { get; set; }
    }
}
