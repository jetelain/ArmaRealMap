using GameRealisticMap.Algorithms;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace GameRealisticMap.Test.Algorithms.Filling
{
    public abstract class FillAreaTestBase
    {
        private readonly ITestOutputHelper output;

        public FillAreaTestBase(ITestOutputHelper output)
        {
            this.output = output;
        }

        protected void AssertResult(RadiusPlacedLayer<string> result, string ressourceName)
        {
            var csv = result
                .OrderBy(e => e.Center.X).ThenBy(e => e.Center.Y)
                .Select(e => FormattableString.Invariant($"{e.Model};{e.Center.X:0.00};{e.Center.Y:0.00};{e.Angle:0.00};{e.Scale:0.00};{e.RelativeElevation:0.00};{e.Radius:0.00};{e.FitRadius:0.00}"))
                .ToList();

            using var ressource = typeof(FillAreaBasicTest).Assembly.GetManifestResourceStream(ressourceName);
            if (ressource == null)
            {
                var temp = Path.Combine(Path.GetTempPath(), ressourceName);
                File.WriteAllLines(temp, csv);
                throw new XunitException($"Missing ressource, actual '{temp}'");
            }
            try
            {
                Assert.Equal(ReadAllLines(ressource), csv);
            }
            catch
            {
#if DEBUG
                var temp = Path.Combine(Path.GetTempPath(), ressourceName);
                File.WriteAllLines(temp, csv);
                output.WriteLine($"Actual '{temp}'");
#endif
                throw;
            }
        }

        private static IEnumerable<string> ReadAllLines(Stream? ressource)
        {
            var lines = new List<string>();
            if (ressource != null)
            {
                var sr = new StreamReader(ressource);

                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }
            return lines;
        }
    }
}
