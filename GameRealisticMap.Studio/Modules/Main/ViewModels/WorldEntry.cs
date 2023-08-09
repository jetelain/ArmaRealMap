using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Studio.Modules.Arma3WorldEditor;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Modules.Main.ViewModels
{
    public class WorldEntry
    {
        public WorldEntry(IArma3WorldEntry world, ProjectDrive drive)
        {
            if (!string.IsNullOrEmpty(world.PboPrefix) && !string.IsNullOrEmpty(world.WorldName))
            {
                WorldName = world.WorldName;
                Description = world.Description ?? world.WorldName;
                FilePath = drive.GetFullPath(world.PboPrefix + "\\" + world.WorldName + ".wrp");
                PreviewPath = drive.GetFullPath(world.PboPrefix + "\\data\\picturemap_ca.png");
                Exists = File.Exists(FilePath);
                TimeStamp = world.TimeStamp;
            }
            else
            {
                WorldName = string.Empty;
                Description = string.Empty;
                FilePath = string.Empty;
                PreviewPath = string.Empty;
                Exists = false;
            }
        }

        public string WorldName { get; }

        public string Description { get; }

        public string FilePath { get; }

        public string PreviewPath { get; }

        public bool Exists { get; }

        public DateTime TimeStamp { get; }

        public async Task OpenFile()
        {
            var provider = IoC.Get<IEditorProvider>("Arma3WorldEditorProvider");
            var editor = provider.Create();
            await provider.Open(editor, FilePath);
            await IoC.Get<IShell>().OpenDocumentAsync(editor);
        }

        public string Tooltip => string.Format("WorldName: {0}\r\nLast generated on {1}", WorldName, TimeStamp.ToLocalTime());

        public ImageSource ImageSource
        {
            get
            {
                var imgTemp = new BitmapImage();
                imgTemp.BeginInit();
                imgTemp.CacheOption = BitmapCacheOption.OnLoad;
                imgTemp.UriSource = new Uri(PreviewPath);
                imgTemp.EndInit();
                return imgTemp;
            }
        }
    }
}
