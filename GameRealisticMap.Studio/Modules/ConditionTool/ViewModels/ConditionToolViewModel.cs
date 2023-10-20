using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Conditions;
using GameRealisticMap.Geometries;
using GameRealisticMap.Studio.Modules.MapConfigEditor;
using GameRealisticMap.Studio.Modules.MapConfigEditor.ViewModels;
using Gemini.Framework;
using Gemini.Framework.Services;
using Microsoft.Win32;

namespace GameRealisticMap.Studio.Modules.ConditionTool.ViewModels
{
    [Export(typeof(IConditionTool))]
    internal class ConditionToolViewModel : Tool, IConditionTool
    {
        private readonly IShell shell;
        private IConditionToolWorker? worker;

        [ImportingConstructor]
        public ConditionToolViewModel(IShell shell)
        {
            this.shell = shell;
            DisplayName = Labels.TagsEditor;
            Criterias = new List<CriteriaItem>();
            SetTarget(new PointConditionVM());
        }

        public override PaneLocation PreferredLocation => PaneLocation.Right;

        public string ConditionText { get; set; } = string.Empty;

        public string ErrorMessage => worker?.ErrorMessage ?? string.Empty;

        public List<ConditionToken> Tokens => worker?.Tokens ?? new List<ConditionToken>();

        public bool HasError => worker?.HasError ?? false;

        public List<CriteriaItem> Criterias { get; private set; }

        public Task Apply()
        {
            worker?.SetConditionText(ConditionText);
            NotifyOfPropertyChange(nameof(ErrorMessage));
            NotifyOfPropertyChange(nameof(Tokens));
            NotifyOfPropertyChange(nameof(HasError));
            return Task.CompletedTask;
        }

        public Task AddCriteria(CriteriaItem criteria)
        {
            var text = ConditionText;
            if (!string.IsNullOrEmpty(text) )
            {
                if(!text.EndsWith("!"))
                {
                    text += " && ";
                }
            }
            text += criteria.Name + criteria.InitText;
            ConditionText = text;
            NotifyOfPropertyChange(nameof(ConditionText));
            return Apply();
        }

        public List<MapConfigEditorViewModel> GetAvailableMaps()
        {
            return shell.Documents.OfType<MapConfigEditorViewModel>().ToList();
        }

        public async Task OpenAndTestOnMap()
        {
            var provider = IoC.Get<MapConfigEditorProvider>();
            var dialog = new OpenFileDialog();
            dialog.Filter = string.Join("|", provider.FileTypes.Select(x => x.Name + "|*" + x.FileExtension));
            if (dialog.ShowDialog() != true)
            {
                return;
            }
            var map = (MapConfigEditorViewModel)provider.Create();
            await provider.Open(map, dialog.FileName);
            await shell.OpenDocumentAsync(map);
            await TestOnMap(map);
        }

        public async Task TestOnFirstMap()
        {
            var map = shell.Documents.OfType<MapConfigEditorViewModel>().FirstOrDefault();
            if (map == null)
            {
                await OpenAndTestOnMap();
            }
            else
            {
                await TestOnMap(map);
            }
        }

        public async Task TestOnMap(MapConfigEditorViewModel map)
        {
            await Apply();
            if (worker == null || map == null)
            {
                return;
            }
            var tester = shell.Documents.OfType<ConditionTestMapViewModel>().Where(t => t.Map == map).FirstOrDefault();
            if (tester == null)
            {
                tester = new ConditionTestMapViewModel(map);
            }
            await shell.OpenDocumentAsync(tester);
            await worker.TestOnMap(tester);
        }

        public void SetTarget<TCondition, TContext, TGeometry>(ConditionVMBase<TCondition, TContext, TGeometry> target)
            where TCondition : class, ICondition<TContext>
            where TContext : IConditionContext<TGeometry>
            where TGeometry : ITerrainEnvelope
        {
            var prevWorker = worker;

            ConditionText = target.Condition;
            worker = new ConditionToolWorker<TCondition, TContext, TGeometry>(target);

            NotifyOfPropertyChange(nameof(ErrorMessage));
            NotifyOfPropertyChange(nameof(Tokens));
            NotifyOfPropertyChange(nameof(HasError));
            NotifyOfPropertyChange(nameof(ConditionText));

            if (prevWorker == null || worker.ContextType != prevWorker.ContextType)
            {
                Criterias = worker.GenerateCriterias();
                NotifyOfPropertyChange(nameof(Criterias));
            }
        }
    }
}
