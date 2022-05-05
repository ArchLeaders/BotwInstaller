using BotwScripts.Lib.Common.Web;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BotwInstaller.Wizard.ViewModels
{
    public class SplashViewModel : Screen
    {
        public void Open(string gameMode)
        {
            Mode = gameMode;
            Shell.SetupPageVisibility = Visibility.Visible;
            Shell.SplashPageVisibility = Visibility.Collapsed;
            Download.GlobalTimout = Shell.Timeout;
        }
        
        private string _mode = "wiiu";
        public string Mode
        {
            get => _mode;
            set => SetAndNotify(ref _mode, value);
        }

        public ShellViewModel Shell;
        public SplashViewModel(ShellViewModel shell)
        {
            Shell = shell;
        }
    }
}
