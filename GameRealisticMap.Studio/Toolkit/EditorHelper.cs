using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Toolkit
{
    internal static class EditorHelper
    {
        public static async Task OpenWithEditor(string defaultEditorContractName, string filePath)
        {
            var shell = IoC.Get<IShell>();

            var opened = shell.Documents.OfType<IPersistedDocument>().FirstOrDefault(d => string.Equals(d.FilePath, filePath));
            if (opened != null)
            {
                await shell.OpenDocumentAsync(opened);
            }
            else
            {
                var provider = IoC.Get<IEditorProvider>(defaultEditorContractName);
                var editor = provider.Create();
                await shell.OpenDocumentAsync(editor);
                await provider.Open(editor, filePath);
            }
        }
    }
}
