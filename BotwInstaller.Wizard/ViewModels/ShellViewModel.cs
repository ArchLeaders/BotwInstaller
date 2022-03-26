﻿#pragma warning disable CA1822
#pragma warning disable CS8601

using BotwInstaller.Lib;
using BotwInstaller.Lib.Remote;
using BotwInstaller.Wizard.Helpers;
using BotwInstaller.Wizard.ViewResources.Data;
using BotwScripts.Lib.Common.IO.FileSystems;
using BotwScripts.Lib.Common.Web;
using Stylet;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BotwInstaller.Wizard.ViewModels
{
    public class ShellViewModel : Screen, INotifyPropertyChanged
    {
        [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
        private static extern long mciSendString(string strCommand,
            StringBuilder? strReturn, int iReturnLength, IntPtr hwndCallback);

        #region Actions

        public void GoBack()
        {
            SetupPageVisibility = Visibility.Collapsed;
            SplashPageVisibility = Visibility.Visible;
        }

        public void Home()
        {
            ExceptionPageVisibility = Visibility.Collapsed;
            LaunchPageVisibility = Visibility.Collapsed;
            InstallPageVisibility = Visibility.Collapsed;
            SetupPageVisibility = Visibility.Collapsed;
            SplashPageVisibility = Visibility.Visible;
        }

        public async Task Install()
        {
            Stopwatch watch = new();
            watch.Start();

            Conf.Shortcuts.BCML.Desktop = SetupViewModel.DesktopShortcuts;
            Conf.Shortcuts.BotW.Desktop = SetupViewModel.DesktopShortcuts;
            Conf.Shortcuts.Cemu.Desktop = SetupViewModel.DesktopShortcuts;
            Conf.Shortcuts.DS4Windows.Desktop = SetupViewModel.DesktopShortcuts;

            Conf.Dirs.Dynamic = SetupViewModel.GenericPath;
            Conf.ModPack = SetupViewModel.ModPreset;

            if (SplashViewModel.Mode == "cemu")
            {
                // Check controller
                if (SplashViewModel.Mode == "cemu" &&
                    SetupViewModel.ControllerApi == "Controller -" &&
                    !WindowManager.Show("No controller API was selected.\nUse the default? (XBox/XInput)", isYesNo: true, width: 250)
                ) return;
                else SetupViewModel.ControllerApi = "XBox Controller";

                Conf.ControllerApi = SetupViewModel.ControllerApi;
                Conf.Install.Cemu = true; Conf.UseCemu = true;
                Conf.Install.Base = SetupViewModel.CopyGameFiles;
            }
            else if (SplashViewModel.Mode == "switch") Conf.IsNX = true;

            SetupPageVisibility = Visibility.Collapsed;
            InstallPageVisibility = Visibility.Visible;

            try
            {
                Conf = await Installer.RunInstallerAsync(InstallViewModel.LogMessage, InstallViewModel.Update, Conf);

                if (Conf.Dirs.Base == "NOT FOUND" || Conf.Dirs.Update == "NOT FOUND")
                {
                    var name = Conf.Dirs.Update == "NOT FOUND" ? nameof(Conf.Dirs.Update) : nameof(Conf.Dirs.Base);
                    WindowManager.Show($"The BOTW Game {name} files could not be found and/or verified.\nPlease dump BOTW from your WiiU console.\n\nhttps://wiiu.hacks.guide/#/", width: 400);
                    ReportError(new()
                    {
                        Message = "Game files not found",
                        Exception = "System Search Unsuccessful",
                        ExtendedMessage = "The BOTW Game {name} files could not be found and/or verified.\nPlease dump BOTW from your WiiU console.\n\nhttps://wiiu.hacks.guide/#/"
                    }, "Game files not found", false);
                }
                else if (Conf.Dirs.Base.EndsWith("(PIRATED)"))
                {
                    string audio = $"{Config.AppData}\\Temp\\BOTW\\audio.mp3";
                    await Download.FromUrl(HttpLinks.Audio, audio);

                    WindowManager.Show($"Hmm, you seem to have some... interesting files in your game dump.", "What's this??", width: 260);

                    string command = $"open \"{audio}\" type mpegvideo alias MediaFile";
                    string play = $"play MediaFile";

                    mciSendString(command, null, 0, IntPtr.Zero);
                    mciSendString(play, null, 0, IntPtr.Zero);

                    WindowManager.Show($"You have collected your game files in a less than legal manner.\n" +
                        $"I can't stop you from pirating, but you should know you can't use this tool with illigal files.", "Piracy Notice", width: 420);

                    ReportError(new()
                    {
                        Message = "Pirating the game is illegal and not supported.",
                        ExtendedMessage = Texts.PiracyWarning
                    }, "Piracy Warning", false, false);
                }
            }
            catch (Exception ex)
            {
                foreach (var handled in Texts.HandledExceptions)
                {
                    if (ex.Message.StartsWith(handled.Value.Exception))
                    {
                        ReportError(handled.Value, handled.Key);
                        return;
                    }
                }

                ReportError(new() { Message = ex.Message, ExtendedMessage = ex.StackTrace }, "Unhandled Exception");
            }
            finally
            {
                if (Directory.Exists($"{Config.AppData}\\Temp\\BOTW"))
                    Directory.Delete($"{Config.AppData}\\Temp\\BOTW", true);

                watch.Stop();

                LaunchPageViewModel = new(SetupViewModel);
                LaunchPageViewModel.Time = watch.ElapsedMilliseconds / 1000 >= 60
                    ? $"{watch.ElapsedMilliseconds / 1000 / 60} Minute(s)"
                    : $"{watch.ElapsedMilliseconds / 1000} Seconds";

                LaunchPageVisibility = Visibility.Visible;
            }
        }

        public void ReportError(HandledException ex, string title, bool isReportable = true, bool showDialog = true)
        {
            InstallViewModel.LogMessage($"---\nFailed - {ex.Message}\n---\n");

            if (showDialog) WindowManager.Error(ex.Message, ex.ExtendedMessage, title);

            ExceptionViewModel = new() {
                Title = title,
                Message = ex.Message,
                ExtendedMessage = ex.ExtendedMessage.RenderMarkdown(),
                ExtendedMessageStr = ex.ExtendedMessage,
                IsReportable = isReportable,
                ShellViewModel = this
            };
            LaunchPageVisibility = Visibility.Visible;
            ExceptionPageVisibility = Visibility.Visible;
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
            //if (id == "game")
            //{
            //    if (value == -1)
            //        UnboundGameInstallValue = 100;

            //    if (value == -2)
            //        GameInstallValue = 100;

            //    if (UnboundGameInstallValue != 100)
            //    {
            //        if (UnboundGameInstallValue + value - 100 > 0)
            //            UnboundGameInstallValue = UnboundGameInstallValue + value - (value - 100);
            //        else
            //            UnboundGameInstallValue = UnboundGameInstallValue + value;
            //    }
                    
            //}
            //else if (id == "cemu")
            //{
            //    if (UnboundCemuInstallValue != 100)
            //    {
            //        if (value - 100 > 0)
            //            UnboundCemuInstallValue = value - (value - 100);
            //        else
            //            UnboundCemuInstallValue = value;
            //    }
            //}
            //else if (id == "bcml")
            //{
            //    if (UnboundBcmlInstallValue != 100)
            //    {
            //        if (value - 100 > 0)
            //            UnboundBcmlInstallValue = value - (value - 100);
            //        else
            //            UnboundBcmlInstallValue = value;
            //    }
            //}
            //else if (id == "tool")
            //{
            //    if (UnboundToolProgressValue != 100)
            //    {
            //        if (value - 100 > 0)
            //            UnboundToolProgressValue = value - (value - 100);
            //        else
            //            UnboundToolProgressValue = value;
            //    }
            //}
        }

        // public void SetSpeed(double value, string id = "placeholder") => UpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)Math.Round(value));

        #endregion

        #region Visibility Bindings

        public Visibility SplashPageVisibility
        {
            get => _splashPageVisibility;
            set => SetAndNotify(ref _splashPageVisibility, value);
        }
        private Visibility _splashPageVisibility = Visibility.Visible;

        public Visibility SetupPageVisibility
        {
            get => _setupPageVisibility;
            set
            {
                SetupViewModel.ModPresets = GameInfo.GetModPresets(SplashViewModel.Mode);
                SetupViewModel.ModPreset = "None";

                if (SplashViewModel.Mode == "cemu")
                {
                    SetupViewModel.ControllerApiVisibility = value;
                    SetupViewModel.GenericPathLabel = "Cemu Install Directory";
                    SetupViewModel.GenericPath = Conf.Dirs.Dynamic;
                }
                else if (SplashViewModel.Mode == "switch")
                {
                    SetupViewModel.GenericPathLabel = "BCML Export Directory";
                    SetupViewModel.GenericPath = $"{Config.AppData.EditPath()}\\Roaming\\yuzu\\sdmc\\atmosphere\\contents";
                }
                else
                {
                    SetupViewModel.GenericPathLabel = "BCML Export Directory";
                    SetupViewModel.GenericPath = $"{Config.LastDrive}\\sdcafiine\\00050000101c9400\\BCML";
                }

                SetAndNotify(ref _setupPageVisibility, value);
            }
        }
        private Visibility _setupPageVisibility = Visibility.Collapsed;

        public Visibility InstallPageVisibility
        {
            get => _installPageVisibility;
            set
            {
                SetAndNotify(ref _installPageVisibility, value);
            }
        }
        private Visibility _installPageVisibility = Visibility.Hidden;

        public Visibility LaunchPageVisibility
        {
            get => _launchPageVisibility;
            set
            {
                SetAndNotify(ref _launchPageVisibility, value);
            }
        }
        private Visibility _launchPageVisibility = Visibility.Hidden;

        public Visibility ExceptionPageVisibility
        {
            get => _exceptionPageVisibility;
            set
            {
                SetAndNotify(ref _exceptionPageVisibility, value);
            }
        }
        private Visibility _exceptionPageVisibility = Visibility.Hidden;

        #endregion

        // Private backers
        public readonly IWindowManager WindowManager;
        private ExceptionViewModel _exceptionViewModel = new();
        private LaunchPageViewModel _launchPageViewModel = new(new());

        // View models
        public ExceptionViewModel ExceptionViewModel
        {
            get => _exceptionViewModel;
            set => SetAndNotify(ref _exceptionViewModel, value);
        }
        public LaunchPageViewModel LaunchPageViewModel
        {
            get => _launchPageViewModel;
            set => SetAndNotify(ref _launchPageViewModel, value);
        }
        public SplashViewModel SplashViewModel { get; private set; }
        public SetupViewModel SetupViewModel { get; private set; } = new();
        public InstallViewModel InstallViewModel { get; private set; } = new();
        public Config Conf { get; set; } = new();

        public ShellViewModel(IWindowManager windowManager)
        {
            WindowManager = windowManager;
            SplashViewModel = new(this);

            // Start updater
            //UpdateTimer.Start();
            //UpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);

            //UpdateTimer.Tick += (s, e) =>
            //{
            //    if (InstallViewModel.UValues["game"] > InstallViewModel.GameValue)
            //        InstallViewModel.GameValue++;

            //    if (InstallViewModel.UValues["cemu"] > InstallViewModel.CemuValue)
            //        InstallViewModel.CemuValue++;

            //    if (InstallViewModel.UValues["bcml"] > InstallViewModel.BcmlValue)
            //        InstallViewModel.BcmlValue++;

            //    //if (UnboundToolProgressValue > ToolValue)
            //    //    ToolValue++;

            //    //if (ToolProgressValue >= 100)
            //    //{
            //    //    UnboundToolProgressValue = 0;
            //    //    ToolProgressValue = 0;
            //    //}

            //    if (InstallViewModel.BcmlValue >= 100)
            //    {
            //        LaunchPageVisibility = Visibility.Visible;
            //        SplashPageVisibility = Visibility.Hidden;
            //        SetupPageVisibility = Visibility.Hidden;
            //        InstallPageVisibility = Visibility.Hidden;
            //    }
            //};
        }
    }
}