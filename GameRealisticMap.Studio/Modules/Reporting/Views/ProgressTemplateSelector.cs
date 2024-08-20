using System.Windows;
using System.Windows.Controls;
using GameRealisticMap.Studio.Modules.Reporting.ViewModels;
using Pmad.ProgressTracking.Wpf;

namespace GameRealisticMap.Studio.Modules.Reporting.Views
{
    public class ProgressTemplateSelector : DataTemplateSelector
    {
        public HierarchicalDataTemplate Root { get; set; }

        public HierarchicalDataTemplate Normal { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if ( item is ProgressTask)
            {
                return Root;
            }
            return Normal;
        }
    }
}
