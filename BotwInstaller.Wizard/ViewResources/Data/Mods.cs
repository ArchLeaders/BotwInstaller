using BotwInstaller.Lib;
using BotwScripts.Lib.Common.Computer;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Formatting;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BotwInstaller.Wizard.ViewResources.Data
{
    public class Mods
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static ComboBox GetPresets(string mode)
        {
            ComboBox comboBox = new();

            comboBox.ToolTip = null;

            foreach (var package in GameInfo.ModPresetData[mode.Replace("cemu", "wiiu")])
            {
                Grid stack = new();

                stack.ColumnDefinitions.Add(new());
                stack.ColumnDefinitions.Add(new() { Width = new(50) });

                string info = "";
                foreach (var mod in package.Value)
                    info += $"{mod["Name"]} | {mod["Download"]}\n";

                var _notifyIcon = new System.Windows.Forms.NotifyIcon();
                _notifyIcon.Icon = Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
                _notifyIcon.BalloonTipClosed += (s, e) => _notifyIcon.Visible = false;
                _notifyIcon.BalloonTipClicked += async (s, e) =>
                {
                    string random = $"{new Random().Next(1000, 9999)}-{new Random().Next(1000, 9999)}-{new Random().Next(1000, 9999)}-{new Random().Next(1000, 9999)}";

                    Directory.CreateDirectory($"{Config.AppData}\\Temp\\BOTW");
                    await File.WriteAllTextAsync($"{Config.AppData}\\Temp\\BOTW\\{random}.txt", $"[{package.Key}]\n{info}");
                    await HiddenProcess.Start("explorer.exe", $"{Config.AppData}\\Temp\\BOTW\\{random}.txt");
                };

                CheckBox cbItem = new()
                {
                    Style = (Style)Application.Current.Resources["MaterialDesignAccentCheckBox"],
                    Padding = new Thickness(1.5, 0, 0, 1.5),
                    Content = package.Key
                };

                Button button = new()
                {
                    Style = (Style)Application.Current.Resources["MaterialDesignIconButton"],
                    Content = new PackIcon() { Kind = PackIconKind.Information },
                    Margin = new Thickness(0, 0, 20, 0),
                    Height = 30,
                    Width = 30
                };

                button.Click += (s, e) =>
                {
                    _notifyIcon.Visible = true;
                    _notifyIcon.ShowBalloonTip(5000, package.Key, info, System.Windows.Forms.ToolTipIcon.Info);
                };

                Grid.SetColumn(button, 1);

                stack.Children.Add(cbItem);
                stack.Children.Add(button);

                comboBox.Items.Add(stack);
            }

            return comboBox;
        }

        public static List<Dictionary<string, string?>> GetPresetDictionary(ComboBox uiModPresets, string mode)
        {
            List<Dictionary<string, string?>> mods = new();

            foreach (Grid preset in uiModPresets.Items)
            {
                CheckBox presetData = (CheckBox)preset.Children[0];

                if (presetData.IsChecked == true)
                    foreach (var mod in GameInfo.ModPresetData[mode.Replace("cemu", "wiiu")][(string)presetData.Content])
                        if (mod != null)
                            mods.Add(mod);
            }

            return mods;
        }
    }
}
