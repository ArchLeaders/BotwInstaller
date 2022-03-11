// This file is waaaay to clutered
// clean it up!

#pragma warning disable CA1822
#pragma warning disable CS0108
#pragma warning disable CS8602
#pragma warning disable CS8612
#pragma warning disable CS8618
#pragma warning disable CS8629

#pragma warning disable IDE0044

using BotwInstaller.Lib;
using BotwInstaller.Lib.Configurations.Cemu;
using BotwInstaller.Lib.Remote;
using BotwInstaller.Wizard.ViewResources;
using BotwInstaller.Wizard.ViewResources.Data;
using BotwScripts.Lib.Common.Computer;
using BotwScripts.Lib.Common.Computer.Software.Resources;
using BotwScripts.Lib.Common.IO.FileSystems;
using BotwScripts.Lib.Common.Web;
using Octokit;
using Stylet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BotwInstaller.Wizard.ViewModels
{
    public class ShellViewModel : Screen, INotifyPropertyChanged
    {
        [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
        private static extern long mciSendString(string strCommand,
            StringBuilder? strReturn, int iReturnLength, IntPtr hwndCallback);

        #region Actions

        /// <summary>
        /// Shows the next page and executes any related functions.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public async Task NextPage(string mode)
        {
            // Check Contoller
            if (SetupViewModel.ControllerApi == "Controller -" && mode == "install" && GameMode == "cemu")
            {
                if (ShowDialog("No controller is selected.\nWould you like to use the default API? (XInput)", "Warning", true) == true)
                    SetupViewModel.ControllerApi = "XBox Controller";
                else
                    return;
            }

            // Set prefs
            if (mode == "wiiu")
            {
                GameMode = mode;
                SetupViewModel.GenericPathLabel = "Merged Mods Directory";
                SetupViewModel.GenericPath = $"{Config.LastDrive}\\sdcafiine\\00050000101C9400\\MergedMods";
                WiiuPrefs_Visibility = Visibility.Visible;
                CemuPrefs_Visibility = Visibility.Collapsed;
                SwitchPrefs_Visibility = Visibility.Collapsed;
            }
            else if (mode == "cemu")
            {
                GameMode = mode;
                SetupViewModel.GenericPathLabel = "Cemu Installation Directory";
                SetupViewModel.GenericPath = new Config().Dirs.Dynamic;
                CemuPrefs_Visibility = Visibility.Visible;
                WiiuPrefs_Visibility = Visibility.Collapsed;
                SwitchPrefs_Visibility = Visibility.Collapsed;
            }
            else if (mode == "switch")
            {
                GameMode = mode;
                SetupViewModel.GenericPathLabel = "Merged Mods Directory";
                SetupViewModel.GenericPath = $"{Config.AppData.EditPath()}\\Roaming\\yuzu\\sdmc\\atmosphere\\contents";
                SwitchPrefs_Visibility = Visibility.Visible;
                WiiuPrefs_Visibility = Visibility.Collapsed;
                CemuPrefs_Visibility = Visibility.Collapsed;
            }

            // Setup
            if (SetupPageVisibility == Visibility.Hidden)
            {
                SetupViewModel.ModPresets = GameInfo.GetModPresets(mode);
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

                        Conf.Shortcuts.BCML.Desktop = SetupViewModel.DesktopShortcuts;
                        Conf.Shortcuts.BotW.Desktop = SetupViewModel.DesktopShortcuts;
                        Conf.Shortcuts.Cemu.Desktop = SetupViewModel.DesktopShortcuts;
                        Conf.Shortcuts.DS4Windows.Desktop = SetupViewModel.DesktopShortcuts;
                        Conf.Dirs.Dynamic = SetupViewModel.GenericPath;
                        Conf.ModPacks = GameInfo.ModPresetData;
                        Conf.ModPack = SetupViewModel.ModPreset;

                        if (GameMode == "cemu")
                        {
                            Conf.UseCemu = true;
                            Conf.Install.Base = SetupViewModel.CopyGameFiles;
                            Conf.ControllerApi = SetupViewModel.ControllerApiTranslate[SetupViewModel.ControllerApi];
                        }

                        if (GameMode == "switch")
                        {
                            Conf.IsNX = true;
                        }

                        #endregion

                        Conf = await Installer.RunInstallerAsync(LogMessage, Update, SetSpeed, Conf);

                        UpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
                        SetupViewModel.GenericPath = Conf.Dirs.Dynamic;
                    });

                    if (Conf.Dirs.Base == "NOT FOUND" || Conf.Dirs.Update == "NOT FOUND")
                    {
                        var name = Conf.Dirs.Update == "NOT FOUND" ? nameof(Conf.Dirs.Update) : nameof(Conf.Dirs.Base);
                        ShowDialog($"The BOTW Game {name} files could not be found and/or verified.\nPlease dump BOTW from your WiiU console.\n\nhttps://wiiu.hacks.guide/#/", width: 400);
                        ThrowOwnException("Game files not found", "Search Unsuccessful",
                            $"The BOTW Game {name} files could not be found and/or verified.\nPlease dump BOTW from your WiiU console.\n\nhttps://wiiu.hacks.guide/#/");
                        ReportIsEnabled = false;
                    }
                    else if (Conf.Dirs.Base.EndsWith("(PIRATED)"))
                    {
                        string audio = $"{Config.AppData}\\Temp\\BOTW\\audio.wav";
                        await Download.FromUrl(HttpLinks.Audio, audio);

                        ShowDialog($"Hmm, you seem to have some... interesting files in your game dump.", "What's this??", width: 260);

                        string command = $"open \"{audio}\" type mpegvideo alias MediaFile";
                        string play = $"play MediaFile";

                        mciSendString(command, null, 0, IntPtr.Zero);
                        mciSendString(play, null, 0, IntPtr.Zero);

                        ShowDialog($"You have collected your game files in a less than legal manner.\n" +
                            $"I can't stop you from pirating, but you should know you can't use this tool with illigal files.", "Piracy Notice", width: 420);

                        ReportIsEnabled = false;
                        Title = "Piracy Warning";
                        Exception = "Pirating the game is illegal and not supported.";
                        StackTrace =
                            "To legally obtain The Legend of Zelda: Breath of the Wild you must dump " +
                            "it from your WiiU. Alternatively you can dump your WiiU online files and download" +
                            "the game legally from Nintendo's server through Cemu.\n\n" +
                            "WiiU Homebrew Guide: https://wiiu.hacks.guide/#/ \n" +
                            "Dumping Video: https://www.youtube.com/watch?v=bFTgv5mzSg8&t=300s \n" +
                            "Discord Help Server: https://discord.gg/cbA3AWwfJj";
                        ExceptionPageVisibility = Visibility.Visible;
                    }
                }
                catch (Exception ex)
                {
                    if (Directory.Exists($"{Config.AppData}\\Temp\\BOTW"))
                        Directory.Delete($"{Config.AppData}\\Temp\\BOTW", true);

                    if (ex.Message.StartsWith("The request was canceled due to the configured HttpClient.Timeout of "))
                    {
                        ShowError("The server took too long to respond.", $"Check your network connection and retry.\nIf the issue persists report the error.", "Network Error");
                        ThrowOwnException("The server took too long to respond.", "Network Error", $"Check your network connection and retry.\n" +
                            $"If the issue persists report the error.\n\n[Error Details]\n{ex.Message}\n{ex.StackTrace}");
                    }
                    else
                    {
                        ShowError(ex.Message, $"{ex.Message}\n{ex.StackTrace}", "Unhandled Exception", false, "#E84639");
                        ThrowException(ex);
                    }
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
        public bool ShowDialog(string message, string title = "Notice", bool isYesNo = false, string? exColor = null, double width = 220)
        {
            MessageViewModel promptViewModel = new(message, title, isYesNo, exColor, width);
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
        /// Shows a new error box window.
        /// </summary>
        /// <returns></returns>
        public void ThrowException(Exception ex, string title = "Exception Thrown")
        {
            Title = title;
            Exception = ex.Message;
            StackTrace = ex.StackTrace;
            ExceptionPageVisibility = Visibility.Visible;
        }

        /// <summary>
        /// Shows a new error box window.
        /// </summary>
        /// <returns></returns>
        public void ThrowOwnException(string exception, string title = "Exception Thrown", string detailedMessage = "")
        {
            Title = title;
            Exception = exception;
            StackTrace = detailedMessage;
            ExceptionPageVisibility = Visibility.Visible;
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
                if (value == -1)
                    UnboundGameInstallValue = 100;

                if (value == -2)
                    GameInstallValue = 100;

                if (UnboundGameInstallValue != 100)
                {
                    if (UnboundGameInstallValue + value - 100 > 0)
                        UnboundGameInstallValue = UnboundGameInstallValue + value - (value - 100);
                    else
                        UnboundGameInstallValue = UnboundGameInstallValue + value;
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

        public void SetSpeed(double value, string id = "placeholder")
        {
            UpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)Math.Round(value));
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

        private Visibility _exceptionPageVisibility = Visibility.Hidden;
        public Visibility ExceptionPageVisibility
        {
            get { return _exceptionPageVisibility; }
            set
            {
                _exceptionPageVisibility = value;
                NotifyPropertyChanged();
            }
        }

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
            if (File.Exists($"{SetupViewModel.GenericPath}\\Cemu.exe"))
            {
                if (ShowDialog("Cemu already exists in this directory.\nOpen Cemu?", isYesNo: true))
                    await HiddenProcess.Start($"{SetupViewModel.GenericPath}\\Cemu.exe");
                return;
            }

            // Create Temp Directory
            Directory.CreateDirectory($"{Config.AppData}\\Temp\\CEMU");
            UpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            Update(80, "tool");

            // Download Cemu
            await Download.FromUrl(DownloadLinks.Cemu, $"{Config.AppData}\\Temp\\CEMU\\CEMU.PACK.res");
            UpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            Update(85, "tool");

            // Extract Cemu
            await Task.Run(() => ZipFile.ExtractToDirectory($"{Config.AppData}\\Temp\\CEMU\\CEMU.PACK.res", $"{Config.AppData}\\Temp\\CEMU\\CEMU"));
            Update(90, "tool");

            // Install Cemu
            await Task.Run(() => {

                // Setup fonders and variables
                string cemuTemp = $"{Config.AppData}\\Temp\\CEMU\\CEMU".SubFolder();

                // Copy Cemu files
                foreach (var file in Directory.EnumerateFiles(cemuTemp, "*.*", SearchOption.AllDirectories))
                {
                    var dir = new FileInfo(file).DirectoryName;
                    Directory.CreateDirectory($"{SetupViewModel.GenericPath}\\{dir.Replace(cemuTemp, "")}");
                    File.Copy(file, $"{SetupViewModel.GenericPath}\\{file.Replace(cemuTemp, "")}", true);
                }

            });
            Update(95, "tool");

            // Create override tag
            Directory.CreateDirectory($"{Config.AppData}\\Temp\\BOTW");
            await File.WriteAllTextAsync($"{Config.AppData}\\Temp\\BOTW\\OVERRIDE", "");

            // Write basic settings
            Conf.Dirs.Dynamic = SetupViewModel.GenericPath;
            Directory.CreateDirectory($"{SetupViewModel.GenericPath}\\mlc01");
            CemuSettings.Write(Conf, true);

            // Delete Temp Directory
            Directory.Delete($"{Config.AppData}\\Temp\\CEMU", true);

            Update(100, "tool");

            ShowDialog("Cemu Installed successfully.");
            await HiddenProcess.Start($"{SetupViewModel.GenericPath}\\Cemu.exe");
        }

        public void InstallDumpling()
        {

        }

        #endregion

        #region Exception Page Bindings

        public string FormattedError
        {
            get
            {
                return new(

                    $"# {Title}\n\n> {Exception}\n\n```\n{StackTrace}\n```\n\n" +
                    $"## Config Info:\n" +
                    $"```yml\n" +
                    $"{nameof(Conf.ControllerApi)}: {Conf.ControllerApi}\n" +
                    $"{nameof(Conf.IsNX)}: {Conf.IsNX}\n" +
                    $"{nameof(Conf.ModPack)}: {Conf.ModPack}\n" +
                    $"```\n\n" +
                    $"## Directory Info:\n" +
                    $"```yml\n" +
                    $"{nameof(Conf.Dirs.Dynamic)}: {Conf.Dirs.Dynamic}\n" +
                    $"{nameof(Conf.Dirs.Base)}: {Conf.Dirs.Base}\n" +
                    $"{nameof(Conf.Dirs.Update)}: {Conf.Dirs.Update}\n" +
                    $"{nameof(Conf.Dirs.BCML)}: {Conf.Dirs.BCML}\n" +
                    $"{nameof(Conf.Dirs.MLC01)}: {Conf.Dirs.MLC01}\n" +
                    $"{nameof(Conf.Dirs.Python)}: {Conf.Dirs.Python}\n" +
                    $"{nameof(Conf.Dirs.DS4Windows)}: {Conf.Dirs.DS4Windows}\n" +
                    $"```\n\n" +
                    $"## Install Info:\n" +
                    $"```yml\n" +
                    $"{nameof(Conf.Install.Cemu)}: {Conf.Install.Cemu}\n" +
                    $"{nameof(Conf.Install.Base)}: {Conf.Install.Base}\n" +
                    $"{nameof(Conf.Install.Update)}: {Conf.Install.Update}\n" +
                    $"{nameof(Conf.Install.DLC)}: {Conf.Install.DLC}\n" +
                    $"{nameof(Conf.Install.Python)}: {Conf.Install.Python}\n" +
                    $"```"

                );
            }
        }

        public void CopyError()
        {
            Clipboard.SetText(FormattedError.Replace(Config.User, "C:\\Users\\admin"));
        }

        public async Task ReportError()
        {
            if (!ShowDialog($"{ToolTips.ReportError}\n\nContinue anyway?", "Privacy Warning", true, width: 500))
                return;

            bool isPublic = ShowDialog("Would you like to upload to the public GitHub repository?\n\n" +
                "https://github.com/ArchLeaders/BotwInstaller", "", true, width: 300);

            string repo = isPublic ? "botwinstaller" : "botwinstaller-issues";

            ReportIsEnabled = false;

            // Create client
            var client = new GitHubClient(new ProductHeaderValue("botw-installer-v3"));
            client.Credentials = new Credentials(AuthKey.Get);

            // Get issues
            var issues = await client.Issue.GetAllForRepository("archleaders", repo);

            // Update issue if it exists
            foreach (var issue in issues)
            {
                if (issue.Title == Exception)
                {
                    IssueUpdate issueUpdate = new();
                    issueUpdate.Body = $"{issue.Body}\n\n---\n\n{FormattedError.Replace(Config.User, "C:\\Users\\admin")}";

                    await client.Issue.Update("archleaders", repo, issue.Number, issueUpdate);
                    ShowDialog($"Updated issue: {issue.Id}");
                    return;
                }
            }

            // Create new issue
            var issueNew = await client.Issue.Create("archleaders", repo, new NewIssue(Exception) {
                Body = FormattedError.Replace(Config.User, "C:\\Users\\admin")
            });

            ShowDialog($"Created issue: {issueNew.Id}");
        }

        private string _title = "Exception Thrown";
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                NotifyPropertyChanged();
            }
        }

        private string _exception = "";
        public string Exception
        {
            get { return _exception; }
            set
            {
                _exception = value;
                NotifyPropertyChanged();
            }
        }

        private string? _stackTrace = "";
        public string? StackTrace
        {
            get { return _stackTrace; }
            set
            {
                _stackTrace = value;
                NotifyPropertyChanged();
            }
        }

        private bool _reportIsEnabled = true;
        public bool ReportIsEnabled
        {
            get { return _reportIsEnabled; }
            set
            {
                _reportIsEnabled = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        public Config Conf { get; set; } = new();

        public SetupViewModel SetupViewModel { get; private set; } = new();

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
