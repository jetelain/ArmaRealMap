﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
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
            //_mainWindow.Icon = new BitmapImage(new("pack://application:,,,/GameRealisticMap.Studio;component/Resources/Icons/WindowIcon.png"));

            _mainWindow.Shell.ToolBars.Visible = true;
        }
    }
}