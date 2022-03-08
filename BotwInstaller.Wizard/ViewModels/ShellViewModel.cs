#pragma warning disable CA1822
#pragma warning disable CS0108
#pragma warning disable CS8602
#pragma warning disable CS8612
#pragma warning disable CS8618
#pragma warning disable CS8629

#pragma warning disable IDE0044
#pragma warning disable IDE0060

using BotwInstaller.Lib;
using BotwInstaller.Lib.Configurations.Cemu;
using BotwInstaller.Lib.Remote;
using BotwScripts.Lib.Common.Computer;
using BotwScripts.Lib.Common.Computer.Software.Resources;
using BotwScripts.Lib.Common.IO.FileSystems;
using BotwScripts.Lib.Common.Web;
using Stylet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Media;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BotwInstaller.Wizard.ViewModels
{
    public class ShellViewModel : Screen, INotifyPropertyChanged
    {
        [DllImport("winmm.dll")]
        private static extern long mciSendString(string strCommand,
            StringBuilder? strReturn, int iReturnLength, IntPtr hwndCallback);

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
                    ControllerApi = "XBox Controller";
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
                GenericPath = $"{Config.AppData.EditPath()}\\Roaming\\yuzu\\sdmc\\atmosphere\\contents";
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

                        Conf.Shortcuts.BCML.Desktop = DesktopShortcuts;
                        Conf.Shortcuts.BotW.Desktop = DesktopShortcuts;
                        Conf.Shortcuts.Cemu.Desktop = DesktopShortcuts;
                        Conf.Shortcuts.DS4Windows.Desktop = DesktopShortcuts;
                        Conf.Dirs.Dynamic = GenericPath;
                        Conf.ModPacks = ModPresetData;
                        Conf.ModPack = ModPreset;

                        if (GameMode == "cemu")
                        {
                            Conf.UseCemu = true;
                            Conf.Install.Base = CopyBaseGame;
                            Conf.ControllerApi = ControllerApiTranslate[ControllerApi];
                        }

                        if (GameMode == "switch")
                        {
                            Conf.IsNX = true;
                        }

                        #endregion
                        Conf = await Installer.RunInstallerAsync(LogMessage, Update, Conf);

                        UpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
                    });
                    if (Conf.Dirs.Base == "NOT FOUND")
                        ShowDialog("The BOTW Game files could not be found and/or verified.\nPlease dump BOTW from your WiiU console.\n\nhttps://wiiu.hacks.guide/#/");

                    else if (Conf.Dirs.Update == "NOT FOUND")
                        ShowDialog($"The BOTW Update files could not be found and/or verified.\nPlease dump BOTW from your WiiU console.\n\nhttps://wiiu.hacks.guide/#/");

                    else if (Conf.Dirs.Base == "PIRATED")
                    {
                        string audio = $"{Config.AppData}\\Temp\\BOTW\\audio.wav";
                        await Download.FromUrl(HttpLinks.Audio, audio);

                        ShowDialog($"Hmm, you seem to have some... interesting files in your game dump.", "What's this??");

                        string command = $"open \"{audio}\" type mpegvideo alias MediaFile";
                        string play = $"play MediaFile";

                        mciSendString(command, null, 0, IntPtr.Zero);
                        mciSendString(play, null, 0, IntPtr.Zero);

                        ShowDialog($"You have collected your game files in a less than legal manner.\n" +
                            $"I can't stop you from pirating, but you should know you can't use this tool with illigal files.", "Piracy Notice");
                    }
                }
                catch (Exception ex)
                {
                    if (Directory.Exists($"{Config.AppData}\\Temp\\BOTW"))
                        Directory.Delete($"{Config.AppData}\\Temp\\BOTW", true);

                    ShowError(ex.Message, $"{ex.Message}\n{ex.StackTrace}", "Exception", false, "#E84639");
                }
                finally
                {
                    if (Directory.Exists($"{Config.AppData}\\Temp\\BOTW"))
                        Directory.Delete($"{Config.AppData}\\Temp\\BOTW", true);

                    watch.Stop();
                    string time = watch.ElapsedMilliseconds / 1000 >= 60 ? $"{watch.ElapsedMilliseconds / 1000 / 60} Minute(s)" : $"{watch.ElapsedMilliseconds / 1000} Seconds";
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
            UpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 80);

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

            timer.Start();
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
            else if (id == "tool")
            {
                if (UnboundToolProgressValue != 100)
                {
                    if (value - 100 > 0)
                        UnboundToolProgressValue = value - (value - 100);
                    else
                        UnboundToolProgressValue = value;
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

        DispatcherTimer UpdateTimer { get; } = new();
        public double UnboundGameInstallValue { get; set; } = 0.0;
        public double UnboundCemuInstallValue { get; set; } = 0.0;
        public double UnboundBcmlInstallValue { get; set; } = 0.0;
        public double UnboundToolProgressValue { get; set; } = 0.0;

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

        private double _toolProgressValue = 0;
        public double ToolProgressValue
        {
            get { return _toolProgressValue; }
            set
            {
                _toolProgressValue = value;
                StrBcmlInstallValue = $"{Math.Round(value)}%";
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Setup Tools (Actions)

        public async Task InstallHomebrew()
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

        public async Task PrepCemu()
        {
            if (File.Exists($"{GenericPath}\\Cemu.exe"))
            {
                if (ShowDialog("Cemu already exists in this directory.\nOpen Cemu?", isYesNo: true))
                    await HiddenProcess.Start($"{GenericPath}\\Cemu.exe");
                return;
            }

            // Create Temp Directory
            Directory.CreateDirectory($"{Config.AppData}\\Temp\\BOTW");
            UpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            Update(80, "tool");

            // Download Cemu
            await Download.FromUrl(DownloadLinks.Cemu, $"{Config.AppData}\\Temp\\BOTW\\CEMU.PACK.res");
            UpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            Update(85, "tool");

            // Extract Cemu
            await Task.Run(() => ZipFile.ExtractToDirectory($"{Config.AppData}\\Temp\\BOTW\\CEMU.PACK.res", $"{Config.AppData}\\Temp\\BOTW\\CEMU"));
            Update(90, "tool");

            // Install Cemu
            await Task.Run(() => {

                // Setup fonders and variables
                string cemuTemp = $"{Config.AppData}\\Temp\\BOTW\\CEMU".SubFolder();

                // Copy Cemu files
                foreach (var file in Directory.EnumerateFiles(cemuTemp, "*.*", SearchOption.AllDirectories))
                {
                    var dir = new FileInfo(file).DirectoryName;
                    Directory.CreateDirectory($"{GenericPath}\\{dir.Replace(cemuTemp, "")}");
                    File.Copy(file, $"{GenericPath}\\{file.Replace(cemuTemp, "")}", true);
                }

            });
            Update(95, "tool");

            // Create override tag
            await File.WriteAllTextAsync($"{GenericPath}\\installer.tag", "");

            // Write basic settings
            Conf.Dirs.Dynamic = GenericPath;
            Directory.CreateDirectory($"{GenericPath}\\mlc01");
            CemuSettings.Write(Conf, true);

            // Delete Temp Directory
            Directory.Delete($"{Config.AppData}\\Temp\\BOTW", true);

            Update(100, "tool");

            ShowDialog("Cemu Installed successfully.");
            await HiddenProcess.Start($"{GenericPath}\\Cemu.exe");
        }

        public void InstallDumpling()
        {

        }

        #endregion

        public Config Conf { get; set; } = new();
        public static Dictionary<string, Dictionary<string, List<string?>>> ModPresetData { get; set; } = new();
        private readonly IWindowManager windowManager;
        public ShellViewModel(IWindowManager windowManager)
        {
            this.windowManager = windowManager;

            // Start updater
            UpdateTimer.Start();
            UpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);

            UpdateTimer.Tick += (s, e) =>
            {
                if (UnboundGameInstallValue > GameInstallValue)
                    GameInstallValue++;

                if (UnboundCemuInstallValue > CemuInstallValue)
                    CemuInstallValue++;

                if (UnboundBcmlInstallValue > BcmlInstallValue)
                    BcmlInstallValue++;

                if (UnboundToolProgressValue > ToolProgressValue)
                    ToolProgressValue++;

                if (ToolProgressValue >= 100)
                {
                    UnboundToolProgressValue = 0;
                    ToolProgressValue = 0;
                }

                if (BcmlInstallValue >= 100)
                {
                    LaunchPageVisibility = Visibility.Visible;
                    SplashPageVisibility = Visibility.Hidden;
                    SetupPageVisibility = Visibility.Hidden;
                    InstallPageVisibility = Visibility.Hidden;
                }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
