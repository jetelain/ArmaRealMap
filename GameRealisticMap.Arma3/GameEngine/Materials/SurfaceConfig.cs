using System.Text.Json.Serialization;

namespace GameRealisticMap.Arma3.GameEngine.Materials
{
    public sealed class SurfaceConfig
    {
        [JsonConstructor]
        public SurfaceConfig(string name, bool aceCanDig, string files, string soundEnviron, string soundHit, double rough, double maxSpeedCoef, double dust, double lucidity, double grassCover, string impact, double surfaceFriction, double maxClutterColoringCoef, List<ClutterConfig> character)
        {
            Name = name;
            AceCanDig = aceCanDig;
            Files = files;
            SoundEnviron = soundEnviron;
            SoundHit = soundHit;
            Rough = rough;
            MaxSpeedCoef = maxSpeedCoef;
            Dust = dust;
            Lucidity = lucidity;
            GrassCover = grassCover;
            Impact = impact;
            SurfaceFriction = surfaceFriction;
            Character = character;
            MaxClutterColoringCoef = maxClutterColoringCoef;
        }

        public string Name { get; }

        public bool AceCanDig { get; }

		public string Files { get; }

        public string SoundEnviron { get; }

        public string SoundHit { get; }

        public double Rough { get; }

        public double MaxSpeedCoef { get; }

        public double Dust { get; }

        public double Lucidity { get; }

        public double GrassCover { get; }

        public string Impact { get; }

        public double SurfaceFriction { get; }

        public List<ClutterConfig> Character { get; }

        public double MaxClutterColoringCoef { get; }

        public bool Match(string fileName)
        {
            if (Files.EndsWith('*'))
            {
                return fileName.StartsWith(Files.Substring(0, Files.Length - 1), StringComparison.OrdinalIgnoreCase);
            }
            return string.Equals(Files, fileName, StringComparison.OrdinalIgnoreCase);
        }

        public SurfaceConfig WithNameAndFiles(string className, string files)
        {
            return new SurfaceConfig(
                    className,
                    AceCanDig,
                    files,
                    SoundEnviron,
                    SoundHit,
                    Rough,
                    MaxSpeedCoef,
                    Dust,
                    Lucidity,
                    GrassCover,
                    Impact,
                    SurfaceFriction,
                    MaxClutterColoringCoef,
                    Character.Select(c =>
                        new ClutterConfig(
                            className + c.Name,
                            c.Probability,
                            c.Model,
                            c.AffectedByWind,
                            c.IsSwLighting,
                            c.ScaleMin,
                            c.ScaleMax)).ToList()
                    );
        }
    }
}
