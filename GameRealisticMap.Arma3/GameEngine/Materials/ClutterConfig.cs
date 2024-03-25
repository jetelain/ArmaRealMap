using System.Text.Json.Serialization;
using GameRealisticMap.Arma3.TerrainBuilder;

namespace GameRealisticMap.Arma3.GameEngine.Materials
{
    public sealed class ClutterConfig
    {
        [JsonConstructor]
        public ClutterConfig(string name, double probability, ModelInfo model, double affectedByWind, bool isSwLighting, double scaleMin, double scaleMax)
        {
            Name = name;
            Probability = probability;
            Model = model;
            AffectedByWind = affectedByWind;
            IsSwLighting = isSwLighting;
            ScaleMin = scaleMin;
            ScaleMax = scaleMax;
        }

        public string Name { get; }

        public double Probability { get; }

        public ModelInfo Model { get; }

        public double AffectedByWind { get; }

        public bool IsSwLighting { get; }

        public double ScaleMin { get; }

        public double ScaleMax { get; }

        internal void WriteTo(StringWriter sw)
        {
            sw.WriteLine(FormattableString.Invariant($@"class {Name} : DefaultClutter
{{
	model=""{Model}"";
	affectedByWind={AffectedByWind};
	swLighting={(IsSwLighting?1:0)};
	scaleMin={ScaleMin};
	scaleMax={ScaleMax};
}};"));

        }
    }
}
