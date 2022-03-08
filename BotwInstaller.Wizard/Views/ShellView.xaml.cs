#pragma warning disable CS8601
#pragma warning disable CS8604
#pragma warning disable CS0649

using BotwInstaller.Lib.Remote;
using BotwInstaller.Wizard.ViewModels;
using BotwInstaller.Wizard.ViewThemes.App;
using BotwScripts.Lib.Common.Computer;
using Stylet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace BotwInstaller.Wizard.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : Window
    {
        private readonly IWindowManager WindowManager;
        public ShellView()
        {
            #region Template Setters

            InitializeComponent();
            DataContext = new ShellViewModel(WindowManager);

            if (File.Exists(ShellViewTheme.ThemeFile))
            {
                ShellViewTheme.ThemeStr = "Light";
                Animation.ThicknessAnim(footerChangeAppTheme_GridParent, nameof(footerChangeAppTheme_IsLight), Grid.MarginProperty, new Thickness(0), 100);
                ShellViewTheme.Change(true);
            }
            else
            {
                ShellViewTheme.ThemeStr = "Dark";
                Animation.ThicknessAnim(footerChangeAppTheme_GridParent, nameof(footerChangeAppTheme_IsDark), Grid.MarginProperty, new Thickness(0), 100);
                ShellViewTheme.Change();
            }

            // Load button close/minimize events
            btnExit.Click += (s, e) => { Hide(); Environment.Exit(1); };
            btnMin.Click += (s, e) => WindowState = WindowState.Minimized;
            btnReSize.Click += (s, e) =>
            {

                if (WindowState == WindowState.Normal)
                    WindowState = WindowState.Maximized;
                else
                    WindowState = WindowState.Normal;
            };

            // Load window fix
            SourceInitialized += async (s, e) =>
            {
                using HttpClient client = new();
                ShellViewModel.ModPresetData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<string?>>>>(await client.GetStringAsync(HttpLinks.ModPresets));
            };

            // Assign state changed events
            shellView.StateChanged += (s, e) =>
            {

                if (WindowState == WindowState.Normal)
                {
                    rectCascade.Opacity = 0;
                    rectMaximize.Opacity = 1;
                }
                else
                {
                    rectCascade.Opacity = 1;
                    rectMaximize.Opacity = 0;
                }
            };

            // Change app theme event
            footerChangeAppTheme.Click += async (s, e) =>
            {
                if (ShellViewTheme.ThemeStr == "Dark")
                {
                    ShellViewTheme.ThemeStr = "Light";
                    ShellViewTheme.Change(true);
                    Animation.ThicknessAnim(footerChangeAppTheme_GridParent, nameof(footerChangeAppTheme_IsDark), Grid.MarginProperty, new Thickness(0, 0, 0, 34), 250);
                    await Task.Run(() => Thread.Sleep(200));
                    Animation.ThicknessAnim(footerChangeAppTheme_GridParent, nameof(footerChangeAppTheme_IsLight), Grid.MarginProperty, new Thickness(0), 200);
                }
                else
                {
                    ShellViewTheme.ThemeStr = "Dark";
                    ShellViewTheme.Change();
                    Animation.ThicknessAnim(footerChangeAppTheme_GridParent, nameof(footerChangeAppTheme_IsLight), Grid.MarginProperty, new Thickness(0, 34, 0, 0), 250);
                    await Task.Run(() => Thread.Sleep(200));
                    Animation.ThicknessAnim(footerChangeAppTheme_GridParent, nameof(footerChangeAppTheme_IsDark), Grid.MarginProperty, new Thickness(0), 200);
                }
            };

            footerRequestHelp.Click += async (s, e) => await HiddenProcess.Start("explorer.exe", "https://github.com/ArchLeaders/BotwInstaller-RE#readme");

            #endregion

            // . . .
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Hyperlink link = (Hyperlink)sender;
            Process.Start("explorer.exe", link.NavigateUri.ToString());
        }
    }
}
