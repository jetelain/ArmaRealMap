using System.Windows;
using System.Windows.Controls;
using GameRealisticMap.Studio.Behaviors;
using GameRealisticMap.Studio.Modules.ConditionTool.ViewModels;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.ConditionTool.Views
{
    /// <summary>
    /// Logique d'interaction pour ConditionToolView.xaml
    /// </summary>
    public partial class ConditionToolView : UserControl
    {
        public ConditionToolView()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var dc = DataContext as ConditionToolViewModel;
            if ( dc != null)
            {
                var maps = dc.GetAvailableMaps();
                if ( maps.Count == 0 )
                {
                    await dc.OpenAndTestOnMap();
                }
                else if ( maps.Count == 1)
                {
                    await dc.TestOnMap(maps[0]);
                }
                else
                {
                    var c = new ContextMenu();
                    foreach(var item in maps)
                    {
                        c.Items.Add(new MenuItem()
                        {
                            Header = item.FileName,
                            Command = new AsyncCommand(() => dc.TestOnMap(item))
                        });
                    }
                    ButtonBehaviors.ShowButtonContextMenu((Button)e.Source, c);
                }
            }
        }
    }
}
