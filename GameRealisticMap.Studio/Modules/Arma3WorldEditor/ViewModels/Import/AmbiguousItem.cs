using System;
using System.Threading.Tasks;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels.Import
{
    internal class AmbiguousItem
    {
        private readonly FileImporterViewModel parent;

        public AmbiguousItem(FileImporterViewModel parent, string name, string path, Uri preview)
        {
            this.parent = parent;
            Name = name;
            Path = path;
            Preview = preview;
        }

        public string Name { get; }

        public string Path { get; }

        public Uri Preview { get; }

        public Task Resolve()
        {
            return parent.Resolve(Name, Path);
        }
    }
}