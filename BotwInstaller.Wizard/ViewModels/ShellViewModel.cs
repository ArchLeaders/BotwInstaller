﻿#pragma warning disable CS0108
#pragma warning disable CS8612
#pragma warning disable CS8618
#pragma warning disable CS8629

using BotwInstaller.Lib;
using BotwScripts.Lib.Common.Computer;
using BotwScripts.Lib.Common.Web;
using Stylet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BotwInstaller.Wizard.ViewModels
{
    public class ShellViewModel : Screen, INotifyPropertyChanged
    {
        #region Actions

        /// <summary>
        /// Browse logic for opening system folders.
        /// </summary>
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

        /// <summary>
        /// Loads the mod presets.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public BindableCollection<string> GetModPresets(string mode)
        {
            BindableCollection<string> modPresets = new();

            if (mode == "cemu" || mode == "wiiu" || mode == "switch")
                foreach (var key in ModPresetData[mode.Replace("cemu", "wiiu")].Keys)
                    modPresets.Add(key);

            return modPresets;
        }

        /// <summary>
        /// Shows the next page and executes any related functions.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public async Task NextPage(string mode)
        {
            // Check Contoller
            if (ControllerApi == "Controller -" && mode == "install" && GameMode == "cemu")
            {
                if (ShowDialog("No controller is selected.\nWould you like to use the default API? (XInput)", "Warning", true) == true)
                    ControllerApi = "XBox | XBox Emulated DS4";
                else
                    return;
            }

            // Set prefs
            if (mode == "wiiu")
            {
                GameMode = mode;
                GenericPathLabel = "Merged Mods Directory";
                GenericPath = $"{Config.LastDrive}\\sdcafiine\\00050000101C9400\\MergedMods";
                WiiuPrefs_Visibility = Visibility.Visible;
                CemuPrefs_Visibility = Visibility.Collapsed;
                SwitchPrefs_Visibility = Visibility.Collapsed;
            }
            else if (mode == "cemu")
            {
                GameMode = mode;
                GenericPathLabel = "Cemu Installation Directory";
                GenericPath = new Config().Dirs.Dynamic;
                CemuPrefs_Visibility = Visibility.Visible;
                WiiuPrefs_Visibility = Visibility.Collapsed;
                SwitchPrefs_Visibility = Visibility.Collapsed;
            }
            else if (mode == "switch")
            {
                GameMode = mode;
                GenericPathLabel = "Merged Mods Directory";
                GenericPath = $"{Config.LastDrive}\\atmosphere\\contents";
                SwitchPrefs_Visibility = Visibility.Visible;
                WiiuPrefs_Visibility = Visibility.Collapsed;
                CemuPrefs_Visibility = Visibility.Collapsed;
            }

            // Setup
            if (SetupPageVisibility == Visibility.Hidden)
            {
                ModPresets = GetModPresets(mode);
                SetupPageVisibility = Visibility.Visible;

                SplashPageVisibility = Visibility.Hidden;
                InstallPageVisibility = Visibility.Hidden;
            }

            // Installing
            else if (InstallPageVisibility == Visibility.Hidden && !mode.Contains(';'))
            {
                InstallPageVisibility = Visibility.Visible;

                SplashPageVisibility = Visibility.Collapsed;
                SetupPageVisibility = Visibility.Hidden;
            }

            // Splash
            else if (SplashPageVisibility == Visibility.Hidden)
            {
                SplashPageVisibility = Visibility.Visible;

                SetupPageVisibility = Visibility.Hidden;
                InstallPageVisibility = Visibility.Hidden;

                WiiuPrefs_Visibility = Visibility.Collapsed;
                CemuPrefs_Visibility = Visibility.Collapsed;
                SwitchPrefs_Visibility = Visibility.Collapsed;
            }

            // Install
            if (mode == "install")
            {
                Stopwatch watch = new();

                try
                {
                    StartAnimation();
                    watch.Start();

                    await Task.Run(async () =>
                    {
                        #region Configure

                        conf.Shortcuts.BCML.Desktop = DesktopShortcuts;
                        conf.Shortcuts.BotW.Desktop = DesktopShortcuts;
                        conf.Shortcuts.Cemu.Desktop = DesktopShortcuts;
                        conf.Shortcuts.DS4Windows.Desktop = DesktopShortcuts;
                        conf.Dirs.Dynamic = GenericPath;
                        conf.ModPacks = ModPresetData;
                        conf.ModPack = ModPreset;

                        if (GameMode == "cemu")
                        {
                            conf.UseCemu = true;
                            conf.Install.Base = CopyBaseGame;
                            conf.ControllerApi = ControllerApiTranslate[ControllerApi];
                        }

                        if (GameMode == "switch")
                        {
                            conf.IsNX = true;
                        }

                        #endregion
                        await Installer.RunInstallerAsync(LogMessage, Update, conf);

                        updateTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
                    });
                }
                catch (Exception ex)
                {
                    ShowError(ex.Message, $"{ex.Message}\n{ex.StackTrace}", "Exception", false, "#E84639");
                }
                finally
                {
                    if (Directory.Exists($"{Config.AppData}\\Temp\\BOTW"))
                        Directory.Delete($"{Config.AppData}\\Temp\\BOTW", true);

                    watch.Stop();
                    string time = watch.ElapsedMilliseconds / 1000 >= 60 ? $"{watch.ElapsedMilliseconds / 1000 / 60} Minutes" : $"{watch.ElapsedMilliseconds / 1000} Seconds";
                    InstallTime = time;
                }
            }
        }

        /// <summary>
        /// Starts the UI install animations.
        /// </summary>
        public void StartAnimation()
        {
            // Make timer(s)
            DispatcherTimer timer = new();
            timer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            updateTimer.Interval = new TimeSpan(0, 0, 0, 0, 80);

            // Iteration variables
            Dictionary<string, string> installing = new() {
                { "Installing .", "Installing . ." },
                { "Installing . .", "Installing . . ." },
                { "Installing . . .", "Installing ." },
            };

            timer.Tick += (s, e) =>
            {
                InstallingText = installing[InstallingText];
            };

            updateTimer.Tick += (s, e) =>
            {
                if (UnboundGameInstallValue > GameInstallValue)
                    GameInstallValue++;

                if (UnboundCemuInstallValue > CemuInstallValue)
                    CemuInstallValue++;

                if (UnboundBcmlInstallValue > BcmlInstallValue)
                    BcmlInstallValue++;

                if (BcmlInstallValue >= 100)
                {
                    LaunchPageVisibility = Visibility.Visible;
                    SplashPageVisibility = Visibility.Hidden;
                    SetupPageVisibility = Visibility.Hidden;
                    InstallPageVisibility = Visibility.Hidden;
                }
            };

            timer.Start();
            updateTimer.Start();
        }

        /// <summary>
        /// Shows a new message box window.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="isYesNo"></param>
        /// <param name="exColor"></param>
        /// <returns></returns>
        public bool ShowDialog(string message, string title = "Notice", bool isYesNo = false, string? exColor = null)
        {
            MessageViewModel promptViewModel = new(message, title, isYesNo, exColor);
            return !(bool)windowManager.ShowDialog(promptViewModel);
        }

        /// <summary>
        /// Shows a new error box window.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exMessage"></param>
        /// <param name="title"></param>
        /// <param name="isYesNo"></param>
        /// <param name="exColor"></param>
        /// <returns></returns>
        public bool ShowError(string message, string? exMessage = null, string title = "Notice", bool isYesNo = false, string? exColor = null)
        {
            HandledErrorViewModel promptViewModel = new(message, title, isYesNo, exMessage, exColor);
            return !(bool)windowManager.ShowDialog(promptViewModel);
        }

        /// <summary>
        /// Writes a message to the app log.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        public void LogMessage(string text, ConsoleColor color = ConsoleColor.Gray)
        {
            InstallLog = $"{InstallLog}\n{text}";
            ScrollUpdater = !ScrollUpdater;
        }

        /// <summary>
        /// Updates the ui progress bar based on the passed id.
        /// <code>
        /// Valid ID: game, cemu, bcml
        /// </code>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="id"></param>
        public void Update(double value, string id = "")
        {
            if (id == "game")
            {
                if (UnboundGameInstallValue != 100)
                {
                    if (value - 100 > 0)
                        UnboundGameInstallValue = value - (value - 100);
                    else
                        UnboundGameInstallValue = value;
                }
                    
            }
            else if (id == "cemu")
            {
                if (UnboundCemuInstallValue != 100)
                {
                    if (value - 100 > 0)
                        UnboundCemuInstallValue = value - (value - 100);
                    else
                        UnboundCemuInstallValue = value;
                }
            }
            else if (id == "bcml")
            {
                if (UnboundBcmlInstallValue != 100)
                {
                    if (value - 100 > 0)
                        UnboundBcmlInstallValue = value - (value - 100);
                    else
                        UnboundBcmlInstallValue = value;
                }
            }
        }

        /// <summary>
        /// Launches the botw.bat file created when installing
        /// </summary>
        public void LaunchBotw()
        {
            _ = HiddenProcess.Start("cmd.exe", $"/c \"{Config.Root}\\botw.bat\"");
        }

        #endregion

        #region Props

        private bool _desktopShortcuts = true;
        public bool DesktopShortcuts
        {
            get { return _desktopShortcuts; }
            set
            {
                _desktopShortcuts = value;
                NotifyPropertyChanged();
            }
        }

        private bool _copyBaseGame = false;
        public bool CopyBaseGame
        {
            get { return _copyBaseGame; }
            set
            {
                _copyBaseGame = value;
                NotifyPropertyChanged();
            }
        }

        private string _genericPath = new Config().Dirs.Dynamic;
        public string GenericPath
        {
            get { return _genericPath; }
            set
            {
                _genericPath = value;
                NotifyPropertyChanged();
            }
        }

        private string _gameMode = "cemu";
        public string GameMode
        {
            get { return _gameMode; }
            set
            {
                _gameMode = value;
                NotifyPropertyChanged();
            }
        }

        private dynamic _modPreset = "None";
        public dynamic ModPreset
        {
            get => _modPreset;
            set => SetAndNotify(ref _modPreset, value);
        }

        private BindableCollection<string> _modPresets = new();
        public BindableCollection<string> ModPresets
        {
            get { return _modPresets; }
            set
            {
                _modPresets = value;
                NotifyPropertyChanged();
            }
        }

        private string _controllerApi = "Controller -";
        public Dictionary<string, string> ControllerApiTranslate { get; } = new()
        {
            { "Controller -", "XInput" },
            { "XBox Controller", "XInput" },
            { "Nintendo Switch Joycons", "SDLController-Joycon" },
            { "Nintendo Switch Pro Controller", "SDLController" },
            { "DualShock 4", "DSUController" },
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

        public void ScrollViewerSizeChanged(ScrollViewer sender, DependencyPropertyChangedEventArgs e)
        {
            sender.ScrollToBottom();
        }

        private bool _scrollUpdater = true;
        public bool ScrollUpdater
        {
            get { return _scrollUpdater; }
            set
            {
                _scrollUpdater = value;
                NotifyPropertyChanged();
            }
        }

        private string _installTime = "00:00";
        public string InstallTime
        {
            get { return _installTime; }
            set
            {
                _installTime = value;
                NotifyPropertyChanged();
            }
        }

        private string _installLog = "Loading Install Log...";
        public string InstallLog
        {
            get { return _installLog; }
            set
            {
                _installLog = value;
                NotifyPropertyChanged();
            }
        }

        private string _genericPathLabel = "Merged Mods Directory";
        public string GenericPathLabel
        {
            get { return _genericPathLabel; }
            set
            {
                _genericPathLabel = value;
                NotifyPropertyChanged();
            }
        }

        private Visibility _wiiuPrefs_Visibility = Visibility.Collapsed;
        public Visibility WiiuPrefs_Visibility
        {
            get { return _wiiuPrefs_Visibility; }
            set
            {
                _wiiuPrefs_Visibility = value;
                NotifyPropertyChanged();
            }
        }

        private Visibility _switchPrefs_Visibility = Visibility.Collapsed;
        public Visibility SwitchPrefs_Visibility
        {
            get { return _switchPrefs_Visibility; }
            set
            {
                _switchPrefs_Visibility = value;
                NotifyPropertyChanged();
            }
        }

        private Visibility _cemuPrefs_Visibility = Visibility.Collapsed;
        public Visibility CemuPrefs_Visibility
        {
            get { return _cemuPrefs_Visibility; }
            set
            {
                _cemuPrefs_Visibility = value;
                NotifyPropertyChanged();
            }
        }

        private Visibility _splashPageVisibility = Visibility.Visible;
        public Visibility SplashPageVisibility
        {
            get { return _splashPageVisibility; }
            set
            {
                _splashPageVisibility = value;
                NotifyPropertyChanged();
            }
        }

        private Visibility _setupPageVisibility = Visibility.Hidden;
        public Visibility SetupPageVisibility
        {
            get { return _setupPageVisibility; }
            set
            {
                _setupPageVisibility = value;
                NotifyPropertyChanged();
            }
        }

        private Visibility _installPageVisibility = Visibility.Hidden;
        public Visibility InstallPageVisibility
        {
            get { return _installPageVisibility; }
            set
            {
                _installPageVisibility = value;
                NotifyPropertyChanged();
            }
        }

        private Visibility _launchPageVisibility = Visibility.Hidden;
        public Visibility LaunchPageVisibility
        {
            get { return _launchPageVisibility; }
            set
            {
                _launchPageVisibility = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region ToolTips

        public string CopyBaseGameFiles_ToolTip { get; } = "Copies the base game files into Cemu's mlc01 directory.\n(Recomended if your files are on an SDCard)";

        #endregion

        #region Animations

        DispatcherTimer updateTimer = new();

        public double UnboundGameInstallValue { get; set; } = 0.0;
        public double UnboundCemuInstallValue { get; set; } = 0.0;
        public double UnboundBcmlInstallValue { get; set; } = 0.0;

        private string _installingText = "Installing . . .";
        public string InstallingText
        {
            get { return _installingText; }
            set
            {
                _installingText = value;
                NotifyPropertyChanged();
            }
        }

        private string _strGameInstallValue = "0%";
        public string StrGameInstallValue
        {
            get { return _strGameInstallValue; }
            set
            {
                _strGameInstallValue = value;
                NotifyPropertyChanged();
            }
        }

        private string _strCemuInstallValue = "0%";
        public string StrCemuInstallValue
        {
            get { return _strCemuInstallValue; }
            set
            {
                _strCemuInstallValue = value;
                NotifyPropertyChanged();
            }
        }

        private string _strBcmlInstallValue = "0%";
        public string StrBcmlInstallValue
        {
            get { return _strBcmlInstallValue; }
            set
            {
                _strBcmlInstallValue = value;
                NotifyPropertyChanged();
            }
        }

        private double _gameInstallValue = 0;
        public double GameInstallValue
        {
            get { return _gameInstallValue; }
            set
            {
                _gameInstallValue = value;
                StrGameInstallValue = $"{Math.Round(value)}%";
                NotifyPropertyChanged();
            }
        }

        private double _cemuInstallValue = 0;
        public double CemuInstallValue
        {
            get { return _cemuInstallValue; }
            set
            {
                _cemuInstallValue = value;
                StrCemuInstallValue = $"{Math.Round(value)}%";
                NotifyPropertyChanged();
            }
        }

        private double _bcmlInstallValue = 0;
        public double BcmlInstallValue
        {
            get { return _bcmlInstallValue; }
            set
            {
                _bcmlInstallValue = value;
                StrBcmlInstallValue = $"{Math.Round(value)}%";
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Setup Tools (Actions)

        public async Task InstallHomebrewWiiU()
        {
            ShowDialog("Select an empty folder or SDCard to install homebrew in.");

            System.Windows.Forms.FolderBrowserDialog browse = new();

            browse.InitialDirectory = Config.LastDrive;
            browse.Description = "Browse for your SDCard";
            browse.UseDescriptionForTitle = true;

            if (browse.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (var dir in Directory.EnumerateDirectories(browse.SelectedPath))
                {
                    if (dir != null && !dir.EndsWith("System Volume Information"))
                    {
                        ShowDialog($"The selected folder was not empty.\n{dir}", "Warning");
                        return;
                    }
                }

                foreach (var file in Directory.EnumerateFiles(browse.SelectedPath))
                {
                    if (file != null && !file.EndsWith("System Volume Information"))
                    {
                        ShowDialog($"The selected folder was not empty.\n{file}", "Warning");
                        return;
                    }
                }

                await Task.Run(async() =>
                {
                    string tiramisu = "https://tiramisu.foryour.cafe/api/download?packages=environmentloader,wiiu-nanddumper-payload,payloadloaderinstaller,tiramisu";

                    // Download tiramisu
                    await Download.FromUrl(tiramisu, $"{Config.AppData}\\Temp\\TIRAMISU_2022__DATA__UNPACK");

                    // Extract tiramisu
                    ZipFile.ExtractToDirectory($"{Config.AppData}\\Temp\\TIRAMISU_2022__DATA__UNPACK", browse.SelectedPath);

                    // Create setup directory
                    Directory.CreateDirectory($"{browse.SelectedPath}\\wiiu\\environments\\tiramisu\\modules\\setup");

                    // Download patches
                    await Download.FromUrl("https://wiiu.hacks.guide/docs/files/01_sigpatches.rpx", $"{browse.SelectedPath}\\wiiu\\environments\\tiramisu\\modules\\setup\\01_sigpatches.rpx");

                    // Delete tiramisu zip
                    File.Delete($"{Config.AppData}\\Temp\\TIRAMISU_2022__DATA__UNPACK");

                });

                ShowDialog($"Tiramisu installed successfully");
            }
        }

        public void InstallHomebrewSwitch()
        {

        }

        public void PrepCemu()
        {

        }

        public void InstallDumpling()
        {

        }

        #endregion

        public Config conf { get; set; } = new();

        public static Dictionary<string, Dictionary<string, List<string?>>> ModPresetData { get; set; } = new();

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
