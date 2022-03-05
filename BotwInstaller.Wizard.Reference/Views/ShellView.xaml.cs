#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8604
#pragma warning disable CS8605
#pragma warning disable CS8629

using BotwInstaller.Wizard.ViewResources.Controls;
using BotwInstaller.Wizard.ViewThemes.App;
using BotwScripts.Lib.Common.ClassObjects.Json;
using BotwScripts.Lib.Common.ClassObjects.Xml;
using BotwScripts.Lib.Common.IO.FileSystems;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml.Serialization;

namespace BotwInstaller.Wizard.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : Window
    {
        BotwInstallerConfig cc = new();
        Dictionary<string, string> dirs = new();
        List<Task> searcher = new();
        public ShellView()
        {
            #region Template Setters

            InitializeComponent();

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
                // searcher.Add(SetGamePaths());
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

            // Custom Install
            qbtnAdvanced.Click += (s, e) =>
            {
                panelControllerOptions.Visibility = Visibility.Hidden;
                panelStartupPage.Visibility = Visibility.Hidden;
                panelBasicOptions.Visibility = Visibility.Visible;
                Animation.ThicknessAnim(footer, nameof(panelNextBackBtns), MarginProperty, new Thickness(0, 0, 5, 0), 150);
            };

            // Navigation
            btnGoBack.Click += (s, e) =>
            {
                if (panelInstallPaths.Visibility == Visibility.Visible)
                {
                    panelInstallPaths.Visibility = Visibility.Hidden;
                    panelShortcutOptions.Visibility = Visibility.Visible;
                    btnGoForward.Content = "Next";
                }
                else if (panelShortcutOptions.Visibility == Visibility.Visible)
                {
                    panelShortcutOptions.Visibility = Visibility.Hidden;
                    panelControllerOptions.Visibility = Visibility.Visible;
                }
                else if (panelControllerOptions.Visibility == Visibility.Visible)
                {
                    panelControllerOptions.Visibility = Visibility.Hidden;
                    panelBasicOptions.Visibility = Visibility.Visible;
                }
                else if (panelBasicOptions.Visibility == Visibility.Visible)
                {
                    panelBasicOptions.Visibility = Visibility.Hidden;
                    panelStartupPage.Visibility = Visibility.Visible;
                    Animation.ThicknessAnim(footer, nameof(panelNextBackBtns), MarginProperty, new Thickness(0, 40, 5, 0), 150);
                }
            };

            btnGoForward.Click += async (s, e) =>
            {
                if (panelBasicOptions.Visibility == Visibility.Visible)
                {
                    panelBasicOptions.Visibility = Visibility.Hidden;
                    panelControllerOptions.Visibility = Visibility.Visible;
                }
                else if (panelControllerOptions.Visibility == Visibility.Visible)
                {
                    panelControllerOptions.Visibility = Visibility.Hidden;
                    panelShortcutOptions.Visibility = Visibility.Visible;
                }
                else if (panelShortcutOptions.Visibility == Visibility.Visible)
                {
                    panelShortcutOptions.Visibility = Visibility.Hidden;
                    panelInstallPaths.Visibility = Visibility.Visible;
                    btnGoForward.Content = "Install";
                }
                else if (panelInstallPaths.Visibility == Visibility.Visible)
                {
                    panelInstallPaths.Visibility = Visibility.Hidden;
                    btnGoForward.Content = "Cancel";
                    btnGoBack.Visibility = Visibility.Collapsed;
                    btnSaveJson.Visibility = Visibility.Collapsed;
                    btnConsoleMode.Visibility = Visibility.Collapsed;
                    panelInstallScreen.Visibility = Visibility.Visible;
                    await InitInstall();
                }
                else if (panelInstallScreen.Visibility == Visibility.Visible)
                {
                    // Cancel install
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

            // Set defaults
            if (Directory.Exists("D:\\")) tbPathToCemu.Text = $"D:\\Games\\Cemu";
            else tbPathToCemu.Text = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\Games\\Cemu";

            tbPathToPython.Text = $"C:\\Python38";

            #region Add Advanced Text Boxes & Browse Buttons

            btnPathToCemu.Click += (s, e) =>
            {
                System.Windows.Forms.FolderBrowserDialog browse = new();
                browse.Description = "Cemu Directory";
                browse.UseDescriptionForTitle = true;
                browse.InitialDirectory = tbPathToCemu.Text;

                if (browse.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    tbPathToCemu.Text = browse.SelectedPath;

                    tbPathToCemu.Focus();
                    tbPathToCemu.Select(tbPathToCemu.Text.Length, 0);
                }
            };

            btnPathToPython.Click += (s, e) =>
            {
                System.Windows.Forms.FolderBrowserDialog browse = new();
                browse.Description = "Python Directory";
                browse.UseDescriptionForTitle = true;
                browse.InitialDirectory = tbPathToPython.Text;

                if (browse.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    tbPathToPython.Text = browse.SelectedPath;

                    tbPathToPython.Focus();
                    tbPathToPython.Select(tbPathToPython.Text.Length, 0);
                }
            };

            AdvPaths.PathControl[] paths = new AdvPaths.PathControl[]
            {
                new() { Header = "Base Game", Text = "", Offset = new Thickness(15,30,0,15) },
                new() { Header = "Update", Text = "" },
                new() { Header = "DLC", Text = "" },
                new() { Header = "Mlc01", Text = $"{tbPathToCemu.Text}\\mlc01" },
                new() { Header = "MLc01 Temp", Text = $"{tbPathToCemu.Text.EditPath()}{new Random().Next(1000, 9999)}-MlcTemp" },
                new() { Header = "BCML Data", Text = $"%localappdata%\\bcml" },
                new() { Header = "DS4Windows Install", Text = $"%BotwData%\\DS4Windows" },
                new() { Header = "BetterJoy Install", Text = $"%BotwData%\\BetterJoy" }
            };

            foreach (AdvPaths.PathControl path in paths)
            {
                var tb = new TextBox() { Text = path.Text, Margin = new Thickness(0, 5, 0, 15), Name = $"tb{path.Header.Replace(" ", "")}" };
                var btn = new Button() { Margin = path.Offset, Content = "Browse" };

                dirs.Add(tb.Name, tb.Text);

                btn.Click += (s, e) =>
                {
                    System.Windows.Forms.FolderBrowserDialog browse = new();
                    browse.Description = $"{path.Header} Directory";
                    browse.UseDescriptionForTitle = true;
                    browse.InitialDirectory = path.Text;

                    if (browse.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        tb.Text = browse.SelectedPath;

                        tb.Focus();
                        tb.Select(tb.Text.Length, 0);
                    }
                };

                tb.TextChanged += (s, e) =>
                {
                    foreach (var item in dirs)
                        if (item.Key == tb.Name)
                        {
                            dirs.Remove(item.Key);
                            dirs.Add(item.Key, tb.Text);
                            break;
                        }
                };

                advTextBoxes.Children.Add(new TextBlock() { Text = path.Header, Height = 25 });
                advTextBoxes.Children.Add(tb);
                advBrowseButtons.Children.Add(btn);
            }

            #endregion

            // qInstall
            qbtnInstall.Click += async (s, e) =>
            {
                btnGoBack.Visibility = Visibility.Collapsed;
                btnGoForward.Content = "Cancel";
                Animation.ThicknessAnim(footer, nameof(panelNextBackBtns), MarginProperty, new Thickness(0, 0, 5, 0), 150);
                panelStartupPage.Visibility = Visibility.Hidden;
                btnSaveJson.Visibility = Visibility.Collapsed;
                btnConsoleMode.Visibility = Visibility.Collapsed;
                panelInstallScreen.Visibility = Visibility.Visible;
                await InitInstall();
            };

            #endregion

            // . . .
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Hyperlink link = (Hyperlink)sender;
            Process.Start("explorer.exe", link.NavigateUri.ToString());
        }

        private async Task InitInstall()
        {
            #region Configure

            await Task.WhenAll(searcher);

            cc.BaseDir = dirs.ElementAt(0).Value;
            cc.UpdateDir = dirs.ElementAt(1).Value;
            cc.DlcDir = dirs.ElementAt(2).Value;
            cc.MlcDir = dirs.ElementAt(3).Value;
            cc.MlcTemp = dirs.ElementAt(4).Value;
            cc.BcmlData = dirs.ElementAt(5).Value;
            cc.Ds4Dir = dirs.ElementAt(6).Value;
            cc.BetterjoyDir = dirs.ElementAt(7).Value;

            cc.CemuDir = tbPathToCemu.Text;
            cc.PythonDir = tbPathToPython.Text;

            cc.Install.Bcml = cbInstallBcml.IsChecked.Value;
            cc.Install.BaseGame = cbInstallBaseGame.IsChecked.Value;

            // Check for Cemu
            if (File.Exists($"{cc.CemuDir}\\Cemu.exe"))
            {
                cc.Install.Cemu = false;

                var mlcDirFromCemu = "";

                using (Stream reader = new FileStream($"{cc.CemuDir}\\settings.xml", FileMode.Open))
                {
                    CemuSettings cemuSettings = (CemuSettings)new XmlSerializer(typeof(CemuSettings)).Deserialize(reader);
                    mlcDirFromCemu = cemuSettings.MlcPath;
                }

                // Check installs
                // List<bool> installGames = await GameFiles.VerifyCemu(PromptActions.Warn, mlcDirFromCemu, cc.BaseDir);

                // cc.Install.BaseGame = installGames[0];
                // cc.Install.Update = installGames[1];
                // cc.Install.Dlc = installGames[2];
            }

            // Check for Python
            string variables = Environment.GetEnvironmentVariable("PATH");
            if (variables != null)
            {
                if (cc.Install.Bcml)
                    foreach (var env in variables.Split(';'))
                        if (File.Exists($"{env}\\python.exe"))
                            cc.Install.Python = false;
                else cc.Install.Python = false;
            }

            #endregion

            // Install -

        }
    }
}
