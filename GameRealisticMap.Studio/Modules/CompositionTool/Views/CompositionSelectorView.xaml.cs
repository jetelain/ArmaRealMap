﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GameRealisticMap.Studio.Modules.CompositionTool.Views
{
    /// <summary>
    /// Logique d'interaction pour CompositionSelectorView.xaml
    /// </summary>
    public partial class CompositionSelectorView : Window
    {
        public CompositionSelectorView()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { UseShellExecute = true, FileName= e.Uri.OriginalString });
        }
    }
}