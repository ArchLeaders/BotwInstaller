using BotwInstaller.Lib;
using BotwInstaller.Wizard.Views;
using Stylet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace BotwInstaller.Wizard.ViewModels
{
    public class SetupViewModel : Screen
    {
        public void BrowseGenericPath()
        {
            System.Windows.Forms.FolderBrowserDialog browse = new();
            browse.Description = GenericPathLabel;
            browse.UseDescriptionForTitle = true;
            browse.AutoUpgradeEnabled = true;
            browse.InitialDirectory = GenericPath;

            if (browse.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                GenericPath = browse.SelectedPath;
        }

        public static Dictionary<string, string> ControllerApiTranslate = new()
        {
            { "Controller -", "XInput" },
            { "XBox Controller", "XInput" },
            { "Nintendo Switch Joycons", "SDLController-Joycon" },
            { "Nintendo Switch Pro Controller", "SDLController" },
            { "DualShock 4", "DSUController" },
            { "Keyboard (Not Recomended)", "Keyboard" },
        };

        private string _controllerApi = "Controller -";
        public string ControllerApi
        {
            get => _controllerApi;
            set => SetAndNotify(ref _controllerApi, value);
        }

        private Visibility _controllerApiVisibility;
        public Visibility ControllerApiVisibility
        {
            get => _controllerApiVisibility;
            set => SetAndNotify(ref _controllerApiVisibility, value);
        }

        private bool _copyGameFiles = false;
        public bool CopyGameFiles
        {
            get => _copyGameFiles;
            set => SetAndNotify(ref _copyGameFiles, value);
        }

        private bool _desktopShortcuts = true;
        public bool DesktopShortcuts
        {
            get => _desktopShortcuts;
            set => SetAndNotify(ref _desktopShortcuts, value);
        }

        private string _genericPath = string.Empty;
        public string GenericPath
        {
            get => _genericPath;
            set => SetAndNotify(ref _genericPath, value);
        }

        private string _genericPathLabel = string.Empty;
        public string GenericPathLabel
        {
            get => _genericPathLabel;
            set => SetAndNotify(ref _genericPathLabel, value);
        }

        private string _modPreset = "None";
        public string ModPreset
        {
            get => _modPreset;
            set => SetAndNotify(ref _modPreset, value);
        }

        private List<string> _modPresets = new();
        public List<string> ModPresets
        {
            get => _modPresets;
            set => SetAndNotify(ref _modPresets, value);
        }
    }
}
