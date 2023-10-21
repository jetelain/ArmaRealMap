using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Services;
using Gemini.Modules.MainMenu;
using Gemini.Modules.StatusBar;
using Gemini.Modules.ToolBars;

namespace GameRealisticMap.Studio.Test.Mocks
{
    internal class ShellMock : Conductor<IDocument>.Collection.OneActive, IShell
    {
        public bool ShowFloatingWindowsInTaskbar { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IMenu MainMenu => throw new NotImplementedException();

        public IToolBars ToolBars => throw new NotImplementedException();

        public IStatusBar StatusBar => throw new NotImplementedException();

        public ILayoutItem ActiveLayoutItem { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IObservableCollection<IDocument> Documents => throw new NotImplementedException();

        public IObservableCollection<ITool> Tools => throw new NotImplementedException();

        public event EventHandler? ActiveDocumentChanging;
        public event EventHandler? ActiveDocumentChanged;

        public void Close()
        {
            throw new NotImplementedException();
        }

        public Task CloseDocumentAsync(IDocument document)
        {
            throw new NotImplementedException();
        }

        public Task OpenDocumentAsync(IDocument model)
        {
            throw new NotImplementedException();
        }

        public bool RegisterTool(ITool tool)
        {
            throw new NotImplementedException();
        }

        public void ShowTool<TTool>() where TTool : ITool
        {
            throw new NotImplementedException();
        }

        public void ShowTool(ITool model)
        {
            throw new NotImplementedException();
        }
    }
}