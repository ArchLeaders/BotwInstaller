using BotwInstaller.Lib;
using BotwScripts.Lib.Common.Computer;
using Stylet;

namespace BotwInstaller.Wizard.ViewModels
{
    public class LaunchPageViewModel : Screen
    {
        public void LaunchBotw()
        {
            IsEnabled = false;
            Content = "Loading . . .";
            _ = HiddenProcess.Start("cmd.exe", $"/c \"{Config.Root}\\botw.bat\"");
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetAndNotify(ref _isEnabled, value);
        }

        private string _content = "Launch BOTW";
        public string Content
        {
            get => _content;
            set => SetAndNotify(ref _content, value);
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
