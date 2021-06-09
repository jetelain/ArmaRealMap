using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ArmaRealMap.TerrainBuilder
{
    public class TBTemplate
    {
        [XmlElement(Order =0)]
        public string Name { get; set; }

        [XmlElement(Order = 1)]
        public string File { get; set; }

		[XmlElement(Order = 2)]
		public string Date { get; set; } = "2000-01-01 01:01:01.000000";

		[XmlElement(Order = 3)]
		public string Archive { get; set; } = string.Empty;
		[XmlElement(Order = 4)]
		public int Fill { get; set; } = -16777216;
		[XmlElement(Order = 5)]
		public int Outline { get; set; } = -65536;
		[XmlElement(Order = 6)]
		public float Scale { get; set; } = 1;
		[XmlElement(Order = 7)]
		public int Hash { get; set; }

		[XmlElement(Order = 8)]
		public float ScaleRandMin { get; set; }
		[XmlElement(Order = 9)]
		public float ScaleRandMax { get; set; }
		[XmlElement(Order = 10)]
		public float YawRandMin { get; set; }
		[XmlElement(Order = 11)]
		public float YawRandMax { get; set; }
		[XmlElement(Order = 12)]
		public float RollRandMin { get; set; }
		[XmlElement(Order = 13)]
		public float RollRandMax { get; set; }

		[XmlElement(Order = 14)]
		public float PitchRandMin { get; set; }
		[XmlElement(Order = 15)]
		public float PitchRandMax { get; set; }
		[XmlElement(Order = 16)]
		public float TexLLU { get; set; }
		[XmlElement(Order = 17)]
		public float TexLLV { get; set; }
		[XmlElement(Order = 18)]
		public float TexURU { get; set; } = 1;
		[XmlElement(Order = 19)]
		public float TexURV { get; set; } = 1;
		[XmlElement(Order = 20)]
		public float BBRadius { get; set; } = -1;
		[XmlElement(Order = 21)]
		public float BBHScale { get; set; } = 1;
		[XmlElement(Order = 22)]
		public int AutoCenter { get; set; }
		[XmlElement(Order = 23)]
		public float XShift { get; set; }
		[XmlElement(Order = 24)]
		public float YShift { get; set; }
		[XmlElement(Order = 25)]
		public float ZShift { get; set; }
		[XmlElement(Order = 26)]
		public float Height { get; set; }
		[XmlElement(Order = 27)]
        public TBVector BoundingMin { get; set; }
        [XmlElement(Order = 28)]
        public TBVector BoundingMax { get; set; }
        [XmlElement(Order = 29)]
        public TBVector BoundingCenter { get; set; }
		[XmlElement(Order = 30)]
		public string Placement { get; set; } = string.Empty;
	}
}
