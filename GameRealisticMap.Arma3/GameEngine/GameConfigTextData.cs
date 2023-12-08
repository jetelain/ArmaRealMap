using System.Globalization;
using System.Text.RegularExpressions;

namespace GameRealisticMap.Arma3.GameEngine
{
    /// <summary>
    /// Simplified representation of a map game config
    /// </summary>
    public sealed class GameConfigTextData
    {
        public const string FileName = "config.cpp";
        public const string FileNameRecover = "config-initial.hpp";

        private static readonly Regex WorldNameRegex = new Regex(@"worldName\s*=\s*""([^""]+)\.wrp""", RegexOptions.CultureInvariant|RegexOptions.IgnoreCase);
        private static readonly Regex DescriptionRegex = new Regex(@"description\s*=\s*""([^""]+)""", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        private static readonly Regex RevisionRegex = new Regex(@"grma3_revision\s*=\s*([0-9]+)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        private static readonly Regex RoadsRegex = new Regex(@"newRoadsShape\s*=\s*""([^""]+)\\roads.shp""", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        public GameConfigTextData(string initialContent, string pboPrefix, string worldName, string description, string roads, int revision)
        {
            InitialContent = initialContent; 
            PboPrefix = pboPrefix;
            WorldName = worldName;
            Description = description;
            Roads = roads;
            Revision = revision;
        }

        /// <summary>
        /// Initial raw content of the file (updates of Description and Revision are not done)
        /// </summary>
        public string InitialContent { get; }

        /// <summary>
        /// PBO prefix where the wrp file is
        /// </summary>
        public string PboPrefix { get; }

        /// <summary>
        /// Game technical map name equals to wrp filename without extension
        /// </summary>
        public string WorldName { get; }

        /// <summary>
        /// Game displayed map name
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Path to roads shape file
        /// </summary>
        public string Roads { get; set; }

        /// <summary>
        /// GRM specific revision number
        /// </summary>
        public int Revision { get; set; }

        public static GameConfigTextData ReadFromFile(string configFile, string worldName)
        {
            return ReadFromContent(File.ReadAllText(configFile), worldName);
        }

        public static GameConfigTextData ReadFromContent(string content, string? worldName = null)
        {
            var pboPrefix = string.Empty;
            var description = string.Empty;
            var roads = string.Empty;
            var revision = 0;

            var match = WorldNameRegex.Match(content);
            if (match.Success)
            {
                var fullWorldName = match.Groups[1].Value;
                if (!string.IsNullOrEmpty(worldName))
                {
                    if (fullWorldName.EndsWith("\\" + worldName, StringComparison.OrdinalIgnoreCase))
                    {
                        pboPrefix = fullWorldName.Substring(0, fullWorldName.Length - worldName.Length - 1);
                    }
                }
                else
                {
                    var idx = fullWorldName.LastIndexOf('\\');
                    if (idx != -1)
                    {
                        pboPrefix = fullWorldName.Substring(0, idx);
                        worldName = fullWorldName.Substring(idx + 1);
                    }
                }
            }

            match = DescriptionRegex.Match(content);
            if (match.Success)
            {
                description = match.Groups[1].Value;
            }

            match = RevisionRegex.Match(content);
            if (match.Success)
            {
                revision = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            }

            match = RoadsRegex.Match(content);
            if (match.Success)
            {
                roads = match.Groups[1].Value;
            }

            return new GameConfigTextData(content, pboPrefix, worldName ?? string.Empty, description, roads, revision);
        }

        public void SaveIncrementalToFile(string filePath)
        {
            var content = InitialContent;
            if (File.Exists(filePath))
            {
                content = File.ReadAllText(filePath);
            }
            File.WriteAllText(filePath, UpdateDescriptionAndRevision(content));
        }

        public string ToUpdatedContent()
        {
            return UpdateDescriptionAndRevision(InitialContent);
        }

        private string UpdateDescriptionAndRevision(string content)
        {
            var match = RevisionRegex.Match(content);
            if (match.Success)
            {
                content = content.Substring(0, match.Groups[1].Index)
                    + FormattableString.Invariant($"{Revision}")
                    + content.Substring(match.Groups[1].Index + match.Groups[1].Length);
            }
            else
            {
                match = WorldNameRegex.Match(content);
                if (match.Success)
                {
                    var index = match.Index + match.Length;
                    content = content.Substring(0, index)
                        + FormattableString.Invariant($";{Environment.NewLine}\t\tgrma3_revision = {Revision}")
                        + content.Substring(index);
                }
            }

            match = DescriptionRegex.Match(content);
            if (match.Success)
            {
                content = content.Substring(0, match.Groups[1].Index)
                    + Description.Replace('"', ' ')
                    + content.Substring(match.Groups[1].Index + match.Groups[1].Length);
            }

            return content;
        }
    }
}
