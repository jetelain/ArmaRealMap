using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Fences;
using GameRealisticMap.ManMade.Objects;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Individual;
using GameRealisticMap.Studio.Modules.CompositionTool;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal class AssetConfigEditorViewModel : PersistedDocument
    {
        private readonly Arma3DataModule _arma3Data;
        private readonly IShell _shell;
        private readonly ICompositionTool _compositionTool;

        public ObservableCollection<IDocument> Filling { get; } = new ObservableCollection<IDocument>();

        public ObservableCollection<IDocument> Individual { get; } = new ObservableCollection<IDocument>();

        public ObservableCollection<MaterialViewModel> Materials { get; } = new ObservableCollection<MaterialViewModel>();

        public ObservableCollection<RoadViewModel> Roads { get; } = new ObservableCollection<RoadViewModel>();

        public bool IsLoading { get; set; }

        public AssetConfigEditorViewModel(Arma3DataModule arma3Data, IShell shell, ICompositionTool compositionTool)
        {
            _arma3Data = arma3Data;
            _shell = shell;
            _compositionTool = compositionTool;
        }

        public Arma3DataModule Arma3DataModule => _arma3Data;

        protected override async Task DoLoad(string filePath)
        {
            IsLoading = true;
            NotifyOfPropertyChange(nameof(IsLoading));
            using var stream = File.OpenRead(filePath);
            FromJson(await JsonSerializer.DeserializeAsync<Arma3Assets>(stream, Arma3Assets.CreateJsonSerializerOptions(_arma3Data.Library)) ?? new Arma3Assets());
            IsLoading = false;
            NotifyOfPropertyChange(nameof(IsLoading));
        }

        private void FromJson(Arma3Assets arma3Assets)
        {
            Filling.Clear();
            Individual.Clear();
            foreach (var id in Enum.GetValues<BasicCollectionId>())
            {
                foreach (var entry in AtLeastOne(arma3Assets.GetBasicCollections(id)))
                {
                    Filling.Add(new FillingAssetBasicViewModel(id, entry, this));
                }
            }
            foreach (var id in Enum.GetValues<ClusterCollectionId>())
            {
                foreach (var entry in AtLeastOne(arma3Assets.GetClusterCollections(id)))
                {
                    Filling.Add(new FillingAssetClusterViewModel(id, entry, this));
                }
            }
            foreach (var id in Enum.GetValues<FenceTypeId>())
            {
                foreach (var entry in AtLeastOne(arma3Assets.GetFences(id)))
                {
                    Filling.Add(new FencesViewModel(id, entry, this));
                }
            }
            foreach (var id in Enum.GetValues<BuildingTypeId>())
            {
                Individual.Add(new BuildingsViewModel(id, arma3Assets.GetBuildings(id), this));
            }
            foreach (var id in Enum.GetValues<ObjectTypeId>())
            {
                Individual.Add(new ObjectsViewModel(id, arma3Assets.GetObjects(id), this));
            }
            foreach (var id in Enum.GetValues<TerrainMaterialUsage>().OrderByDescending(i => i))
            {
                Materials.Add(new MaterialViewModel(id, arma3Assets.Materials.GetMaterialByUsage(id), this));
            }

            foreach (var id in Enum.GetValues<RoadTypeId>().OrderByDescending(i => i))
            {
                Roads.Add(new RoadViewModel(id, arma3Assets.Roads.FirstOrDefault(m => m.Id == id), arma3Assets.GetBridge(id), this));
            }
            // TODO :
            // - Ponds
        }

        private IEnumerable<T?> AtLeastOne<T>(IReadOnlyCollection<T> collection, T? one = null) where T : class
        {
            if (collection.Count == 0)
            {
                return new T?[] { one };
            }
            return collection;
        }

        private Arma3Assets ToJson()
        {
            EquilibrateProbabilities();

            var json = new Arma3Assets();
            json.ClusterCollections = Filling.OfType<FillingAssetClusterViewModel>().GroupBy(c => c.FillId).ToDictionary(k => k.Key, k => k.Select(o => o.ToDefinition()).ToList());
            json.BasicCollections = Filling.OfType<FillingAssetBasicViewModel>().GroupBy(c => c.FillId).ToDictionary(k => k.Key, k => k.Select(o => o.ToDefinition()).ToList());
            json.Fences = Filling.OfType<FencesViewModel>().GroupBy(c => c.FillId).ToDictionary(k => k.Key, k => k.Select(o => o.ToDefinition()).ToList());
            json.Buildings = Individual.OfType<BuildingsViewModel>().ToDictionary(k => k.FillId, k => k.ToDefinition());
            json.Objects = Individual.OfType<ObjectsViewModel>().ToDictionary(k => k.FillId, k => k.ToDefinition());
            var materialDefintions = Materials
                .Where(m => m.SameAs == null)
                .Select(m => new TerrainMaterialDefinition(m.ToDefinition(), Materials.Where(o => o == m || o.SameAs == m).Select(o => o.FillId).ToArray()))
                .ToList();
            json.Materials = new TerrainMaterialLibrary(materialDefintions);
            json.Roads = Roads.Select(r => r.ToDefinition()).ToList();
            json.Bridges = Roads.Select(r => new { Key = r.FillId, Bridge = r.ToBridgeDefinition() })
                .Where(b => b.Bridge != null)
                .ToDictionary(k => k.Key, k => k.Bridge!);

            // TODO :
            // - Ponds
            return json;
        }

        private void EquilibrateProbabilities()
        {
            foreach(var list in Filling.OfType<FillingAssetClusterViewModel>().GroupBy(c => c.FillId))
            {
                DefinitionHelper.EquilibrateProbabilities(list.ToList());
            }
            foreach (var list in Filling.OfType<FillingAssetBasicViewModel>().GroupBy(c => c.FillId))
            {
                DefinitionHelper.EquilibrateProbabilities(list.ToList());
            }
            foreach (var list in Filling.OfType<FencesViewModel>().GroupBy(c => c.FillId))
            {
                DefinitionHelper.EquilibrateProbabilities(list.ToList());
            }
        }


        protected override Task DoNew()
        {
            FromJson(new Arma3Assets());
            return Task.CompletedTask;
        }

        protected override async Task DoSave(string filePath)
        {
            using var stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, ToJson(), Arma3Assets.CreateJsonSerializerOptions(_arma3Data.Library));
        }

        internal Task EditAssetCategory(IDocument document)
        {
            return _shell.OpenDocumentAsync(document);
        }

        internal void EditComposition(IWithComposition document)
        {
            _shell.ShowTool(_compositionTool);
            _compositionTool.Current = document;
        }
    }
}
