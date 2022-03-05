#pragma warning disable CS8604

using MaterialDesignThemes.Wpf;
using System;
using System.IO;
using System.Windows.Media;

namespace BotwInstaller.Wizard.ViewThemes.App
{
    public class ShellViewTheme
    {
        public static string ThemeFile { get; set; } = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\BotwData\\Apps\\BotwInstaller.Wizard.theme";
        public static string ThemeStr { get; set; } = $"Dark";
        public static void Change(bool toLight = false)
        {
            PaletteHelper helper = new();

            ITheme theme = helper.GetTheme();

            if (toLight)
            {
                ThemeStr = "Light";
                Directory.CreateDirectory(new FileInfo(ThemeFile).DirectoryName);
                File.WriteAllText($"{ThemeFile}", string.Empty);

                theme.SetBaseTheme(Theme.Light);

                theme.PrimaryDark = (Color)ColorConverter.ConvertFromString("#fff");
                theme.PrimaryMid = (Color)ColorConverter.ConvertFromString("#f7f7f7");
                theme.PrimaryLight = (Color)ColorConverter.ConvertFromString("#25000000");

                theme.SecondaryDark = (Color)ColorConverter.ConvertFromString("#657160E8");
                theme.SecondaryMid = (Color)ColorConverter.ConvertFromString("#357160E8");
                theme.SecondaryLight = (Color)ColorConverter.ConvertFromString("#7160E8");

                theme.Selection = (Color)ColorConverter.ConvertFromString("#7160E8");
                theme.Paper = (Color)ColorConverter.ConvertFromString("#D9D9D9");
                theme.Body = (Color)ColorConverter.ConvertFromString("#1f1f1f");
            }
            else
            {
                ThemeStr = "Dark";

                if (File.Exists(ThemeFile)) File.Delete($"{ThemeFile}");

                theme.SetBaseTheme(Theme.Dark);

                theme.PrimaryDark = (Color)ColorConverter.ConvertFromString("#1F1F1F");
                theme.PrimaryMid = (Color)ColorConverter.ConvertFromString("#414141");
                theme.PrimaryLight = (Color)ColorConverter.ConvertFromString("#15ffffff");

                theme.SecondaryDark = (Color)ColorConverter.ConvertFromString("#201B42");
                theme.SecondaryMid = (Color)ColorConverter.ConvertFromString("#423887");
                theme.SecondaryLight = (Color)ColorConverter.ConvertFromString("#7160E8");

                theme.Selection = (Color)ColorConverter.ConvertFromString("#7160E8");
                theme.Paper = (Color)ColorConverter.ConvertFromString("#121212");
                theme.Body = (Color)ColorConverter.ConvertFromString("#B0B0B0");
            }

            helper.SetTheme(theme);
        }
    }
}
