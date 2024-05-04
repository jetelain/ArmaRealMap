using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GameRealisticMap.Studio.Modules.Main.Services;
using GameRealisticMap.Studio.Toolkit;
using Gemini.Framework.Commands;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Modules.Main.Commands
{
    [CommandHandler]
    public class RecentFilesCommandHandler : ICommandListHandler<RecentFilesCommandDefinition>
    {
        private const int DisplayMaxLength = 64;
        private const int DisplayMaxEntries = 20;

        private readonly IShell _shell;
        private readonly IRecentFilesService _recent;
        private readonly IEnumerable<IEditorProvider> _providers;

        [ImportingConstructor]
        public RecentFilesCommandHandler(IShell shell, IRecentFilesService recent, [ImportMany(typeof(IEditorProvider))] IEditorProvider[] providers)
        {
            _shell = shell;
            _recent = recent;
            _providers = providers;
        }

        public void Populate(Command command, List<Command> commands)
        {
            var entries = _recent.GetEntries().GetAwaiter().GetResult();

            foreach(var entry in entries.OrderByDescending(e => e.TimeStamp).Take(DisplayMaxEntries))
            {
                commands.Add(new Command(command.CommandDefinition) { 
                    Text =  GetDisplayPath(entry.FullPath),
                    Enabled = File.Exists(entry.FullPath),
                    Tag = entry,
                    IconSource = _providers.FirstOrDefault(p => p.Handles(entry.FullPath))?.FileTypes?.FirstOrDefault()?.IconSource
                });
            }
        }

        internal static string GetDisplayPath(string fullPath)
        {
            if (fullPath.Length < DisplayMaxLength)
            {
                return fullPath;
            }
            var filename = Path.GetFileName(fullPath);
            var root = Path.GetPathRoot(fullPath);
            var directoryWithoutRoot = Path.GetDirectoryName(fullPath)?.Substring(root?.Length ?? 0);
            var directoryWantedLength = Math.Min(DisplayMaxLength - filename.Length - (root?.Length ?? 0), directoryWithoutRoot?.Length ?? 0);
            if (directoryWantedLength > 0 && !string.IsNullOrEmpty(directoryWithoutRoot))
            {
                directoryWithoutRoot = directoryWithoutRoot.Substring(directoryWithoutRoot.Length - directoryWantedLength);
            }
            else
            {
                directoryWithoutRoot = string.Empty;
            }
            return $"{root}...{directoryWithoutRoot}{Path.DirectorySeparatorChar}{filename}";
        }

        public Task Run(Command command)
        {
            return EditorHelper.OpenDefaultEditor(((RecentFileEntry)command.Tag).FullPath);
        }
    }
}
