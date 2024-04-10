using System.Linq;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Aerial
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var settings = await WorkspaceSettings.Load();
            var pdrive = settings.CreateProjectDrive();
            var library = new ModelInfoLibrary(pdrive);

            var assets = await Arma3Assets.LoadFromFile(library, "builtin:CentralEurope.grma3a");

            var models = assets.Buildings.SelectMany(b => b.Value.SelectMany(b => b.Composition.Objects.Select(o => o.Model)))
                .Concat(assets.BasicCollections.SelectMany(b => b.Value.SelectMany(b => b.Models.SelectMany(m => m.Model.Objects.Select(o => o.Model)))))
                .Concat(assets.ClusterCollections.SelectMany(b => b.Value.SelectMany(b => b.Clusters.SelectMany(b => b.Models.SelectMany(m => m.Model.Objects.Select(o => o.Model))))))
                .Concat(assets.Objects.SelectMany(o => o.Value.SelectMany(b => b.Composition.Objects.Select(o => o.Model))))
                .Distinct()
                .ToList();

            var references = models.Select(mi => AerialModelRefence.FromODOL(mi.Path, library.ReadModelInfoOnly(mi.Path)!)).ToList();

            var progress = new ConsoleProgressSystem();

            var worker = new AerialPhotoWorker(progress, references, string.Empty);

            await worker.TakePhotos();

            progress.DisplayReport();
        }
    }
}
