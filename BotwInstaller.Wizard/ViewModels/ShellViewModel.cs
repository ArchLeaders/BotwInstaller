#pragma warning disable CS0108
#pragma warning disable CS8612
#pragma warning disable CS8618

using Stylet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace BotwInstaller.Wizard.ViewModels
{
    public class ShellViewModel : Screen, INotifyPropertyChanged
    {
        #region Actions

        public async Task Install()
        {
            ShowDialog("Warning!", "MARIO SUCKS!");
        }

        public void ShowDialog(string message, string? exMessage = null, string title = "Notice", bool isYesNo = false, string? exColor = null)
        {
            PromptViewModel promptViewModel = new(message, title, isYesNo, exMessage, exColor);
            windowManager.ShowDialog(promptViewModel);
        }

        #endregion

        #region Props

        private string _controllerApi = "Controller -";
        public Dictionary<string, string> ControllerApiTranslate { get; } = new()
        {
            { "Controller -", "XInput\n\n\n\n\n\n\n\n\n\n\nTEXT" },
            { "XBox | XBox Emulated DS4", "XInput" },
            { "Nintendo Switch Joycons", "SDLController-Joycon" },
            { "Nintendo Switch Pro Controller", "SDLController" },
            { "Emulated DS4", "DSUController" },
            { "Keyboard (Not Recomended)", "Keyboard" },
        };
        public string ControllerApi
        {
            get { return _controllerApi; }
            set
            {
                _controllerApi = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Bindings

        private Visibility page = Visibility.Hidden;

        #endregion

        private IWindowManager windowManager;
        public ShellViewModel(IWindowManager windowManager)
        {
            this.windowManager = windowManager;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
