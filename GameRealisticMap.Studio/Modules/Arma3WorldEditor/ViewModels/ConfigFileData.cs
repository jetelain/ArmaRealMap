using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    internal class ConfigFileData
    {
        private static readonly Regex WorldNameRegex = new Regex(@"worldName\s*=\s*""([^""]+)\.wrp""", RegexOptions.CultureInvariant|RegexOptions.IgnoreCase);
        private static readonly Regex DescriptionRegex = new Regex(@"description\s*=\s*""([^""]+)""", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        private static readonly Regex RevisionRegex = new Regex(@"grma3_revision\s*=\s*([0-9]+)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        // grma3_revision

        public ConfigFileData(string configFile, string pboPrefix, string worldName, string description, int revision)
        {
            Path = configFile; 
            PboPrefix = pboPrefix;
            WorldName = worldName;
            Description = description;
            Revision = revision;
        }

        public string Path { get; }

        public string PboPrefix { get; }

        public string WorldName { get; }

        public string Description { get; }

        public int Revision { get; private set; }

        public static ConfigFileData ReadFromFile(string configFile, string worldName)
        {
            var config = File.ReadAllText(configFile);

            var pboPrefix = string.Empty;
            var description = string.Empty;
            var revision = 0;

            var match = WorldNameRegex.Match(config);
            if (match.Success)
            {
                var fullWorldName = match.Groups[1].Value;
                if (fullWorldName.EndsWith("\\" + worldName, StringComparison.OrdinalIgnoreCase))
                {
                    pboPrefix = fullWorldName.Substring(0, fullWorldName.Length - worldName.Length - 1);
                }
            }

            match = DescriptionRegex.Match(config);
            if (match.Success)
            {
                description = match.Groups[1].Value;
            }

            match = RevisionRegex.Match(config);
            if (match.Success)
            {
                revision = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            }

            return new ConfigFileData(configFile, pboPrefix, worldName, description, revision);
        }

        public void UpdateRevision(int newRevision)
        {
            var config = File.ReadAllText(Path);

            var match = RevisionRegex.Match(config);
            if (match.Success)
            {
                config = config.Substring(0, match.Groups[1].Index)
                    + FormattableString.Invariant($"{newRevision}")
                    + config.Substring(match.Groups[1].Index + match.Groups[1].Length);
            }
            else
            {
                match = WorldNameRegex.Match(config);
                if (match.Success)
                {
                    var index = match.Index + match.Length;
                    config = config.Substring(0, index)
                        + FormattableString.Invariant($";\r\n\t\tgrma3_revision = {newRevision}")
                        + config.Substring(index);
                }
            }
            File.WriteAllText(Path, config);
            Revision = newRevision;
        }
    }
}
