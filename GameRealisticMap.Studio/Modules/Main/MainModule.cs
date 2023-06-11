using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using GameRealisticMap.Studio.Modules.Main.ViewModels;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Modules.Main
{
    [Export(typeof(IModule))]
    public class MainModule : ModuleBase
    {
        private readonly IMainWindow _mainWindow;

        [ImportingConstructor]
        public MainModule(IMainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public override void Initialize()
        {
            _mainWindow.Title = "GameRealisticMap Studio";
            _mainWindow.Icon = new BitmapImage(new("pack://application:,,,/GameRealisticMap.Studio;component/Resources/Icons/MainWindowIcon.png"));
            _mainWindow.Shell.ToolBars.Visible = true;
        }

        public override async Task PostInitializeAsync()
        {
            await _mainWindow.Shell.OpenDocumentAsync(IoC.Get<HomeViewModel>());

            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                var providers = IoC.GetAllInstances(typeof(IEditorProvider)).Cast<IEditorProvider>();

                foreach (var path in args.Skip(1).Where(str => File.Exists(str)))
                {
                    var provider = providers.FirstOrDefault(p => p.Handles(path));
                    if (provider!= null)
                    {
                        var editor = provider.Create();
                        await provider.Open(editor, path);
                        await _mainWindow.Shell.OpenDocumentAsync(editor);
                    }
                }
            }
        }
    }
}
