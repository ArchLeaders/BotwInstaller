using BotwInstaller.Lib;
using BotwScripts.Lib.Common.Computer;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotwInstaller.Wizard.ViewModels
{
    public class LaunchPageViewModel : Screen
    {
        public void LaunchBotw()
        {
            _ = HiddenProcess.Start("cmd.exe", $"/c \"{Config.Root}\\botw.bat\"");
        }

        private string _time = "0 Seconds";
        public string Time
        {
            get => _time;
            set => SetAndNotify(ref _time, value);
        }

        public SetupViewModel? SetupViewModel { get; set; } = null;

        public LaunchPageViewModel(SetupViewModel setupViewModel)
        {
            SetupViewModel = setupViewModel;
        }
    }
}
