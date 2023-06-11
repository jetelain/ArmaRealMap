using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;

namespace GameRealisticMap.Studio.Modules.Explorer.Views
{
    internal class TreeItemDataTemplateSelector : DataTemplateSelector
    {
        public HierarchicalDataTemplate Basic { get; set; }

        public HierarchicalDataTemplate Counter { get; set; }


        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if ( item is IExplorerTreeItemCounter)
            {
                return Counter;
            }



            return Basic;
        }
    }
}
