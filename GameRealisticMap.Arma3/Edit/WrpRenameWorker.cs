using System.Text.RegularExpressions;
using BIS.WRP;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Edit
{
    public sealed class WrpRenameWorker
    {
        private readonly IProgressSystem progress;
        private readonly IGameFileSystemWriter writer;
        private readonly Regex oldPboPrefixRegex;
        private readonly string oldPboPrefix;
        private readonly string newPboPrefix;
        private readonly Dictionary<string, string> filesToCopy;
        private readonly HashSet<string> includes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        internal static readonly Regex IncludeRegex = new Regex(@"#include\s*""([^""]+)""", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        public WrpRenameWorker(IProgressSystem progress, IGameFileSystemWriter writer, string oldPboPrefix, string newPboPrefix)
        {
            this.progress = progress;
            this.writer = writer;
            this.oldPboPrefixRegex = new Regex("\"(" + Regex.Escape(oldPboPrefix) + "\\\\([^\"]+))\"", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            this.oldPboPrefix = oldPboPrefix;
            this.newPboPrefix = newPboPrefix;
            this.filesToCopy = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public IEnumerable<KeyValuePair<string, string>> ReferencedFiles => filesToCopy;

        public async Task RenameAndCopyMaterials(EditableWrp world)
        {
            using var step = progress.CreateStep("RvMat", world.MatNames.Length);
            for (int i = 0; i < world.MatNames.Length; i++)
            {
                var oldName = world.MatNames[i];
                if (oldName != null && oldName.StartsWith(oldPboPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    var newName = newPboPrefix + world.MatNames[i].Substring(oldPboPrefix.Length);
                    await CopyConfigFile(oldName, newName);
                    world.MatNames[i] = newName;
                }
                step.Report(i);
            }
        }

        public async Task CopyReferencedFiles()
        {
            foreach (var file in filesToCopy.ProgressStep(progress, "ReferencedFiles"))
            {
                progress.WriteLine($"Copy '{file.Key}' to '{file.Value}'");
                await writer.CopyAsync(file.Key, file.Value);
            }
            filesToCopy.Clear();
        }

        public string UpdateConfigContent(string content)
        {
            foreach (Match entry in oldPboPrefixRegex.Matches(content))
            {
                var oldName = entry.Groups[1].Value;
                var newName = @$"{newPboPrefix}\{entry.Groups[2].Value}";
                filesToCopy[oldName] = newName;
                if (oldName.EndsWith(".paa", StringComparison.OrdinalIgnoreCase))
                {
                    var oldPng = Path.ChangeExtension(oldName, ".png");
                    if (writer.FileExists(oldPng))
                    {
                        var newPng = Path.ChangeExtension(newName, ".png");
                        filesToCopy[oldPng] = newPng;
                    }
                }
            }
            content = oldPboPrefixRegex.Replace(content, e => @$"""{newPboPrefix}\{e.Groups[2].Value}""");
            return content;
        }

        private async Task CopyIncludes(string content, string location)
        {
            foreach (Match entry in IncludeRegex.Matches(content))
            {
                var includePath = location + "\\" + entry.Groups[1].Value;
                if (includes.Add(includePath) && 
                    includePath.StartsWith(oldPboPrefix, StringComparison.OrdinalIgnoreCase) && 
                    writer.FileExists(includePath))
                {
                    await CopyConfigFile(includePath, newPboPrefix + includePath.Substring(oldPboPrefix.Length), true);
                }
            }
        }

        private async Task CopyConfigFile(string oldConfigFile, string newConfigFile, bool processIncludes = false)
        {
            progress.WriteLine($"Update '{oldConfigFile}' to '{newConfigFile}'");
            var content = await writer.ReadAllTextAsync(oldConfigFile); 
            if (processIncludes)
            {
                await CopyIncludes(content, Path.GetDirectoryName(oldConfigFile)!);
            }
            content = UpdateConfigContent(content);
            writer.CreateDirectory(Path.GetDirectoryName(newConfigFile)!);
            writer.WriteTextFile(newConfigFile, content);
        }

        public async Task<GameConfigTextData> UpdateConfig(GameConfigTextData oldConfig, string newWorldName)
        {
            using var report = progress.CreateStep("UpdateConfig", 1);
            var newContent = oldConfig.InitialContent;
            newContent = new Regex("class\\s+" + Regex.Escape(oldConfig.WorldName) + "([^a-zA-Z0-9_])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
                .Replace(newContent, e => @$"class {newWorldName}{e.Groups[1].Value}");
            newContent = GameConfigTextData.WorldNameRegex.Replace(newContent, @$"worldName = ""{newPboPrefix}\{newWorldName}.wrp""");
            newContent = GameConfigTextData.RoadsRegex.Replace(newContent, @$"newRoadsShape = ""{newPboPrefix}\data\roads\roads.shp""");
            newContent = UpdateConfigContent(newContent); 
            await CopyIncludes(newContent, oldPboPrefix);
            var newConfig = new GameConfigTextData(newContent, newPboPrefix, newWorldName, oldConfig.Description, @$"{newPboPrefix}\data\roads", oldConfig.Revision);
            writer.WriteTextFile(newPboPrefix + "\\" + GameConfigTextData.FileName, newConfig.ToUpdatedContent());
            return newConfig;
        }
    }
}
