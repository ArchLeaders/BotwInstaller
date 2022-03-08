#pragma warning disable CA1416

using BotwScripts.Lib.Common.Computer;
using BotwScripts.Lib.Common.Computer.Software.Resources;
using BotwScripts.Lib.Common.Web;
using static BotwInstaller.Lib.Config;

namespace BotwInstaller.Lib.Configurations.Shortcuts
{
    /// <summary>
    /// Static class for LNK file logic
    /// </summary>
    public static class Shortcuts
    {
        /// <summary>
        /// Writes a set of lnk files using the <paramref name="shortcut"/> data
        /// </summary>
        /// <param name="shortcut">Shortcut data class</param>
        /// <returns></returns>
        public static async Task Write(ShortcutClass shortcut)
        {
            // Create root directory
            Directory.CreateDirectory(Root);

            // Download shell process
            string shell = $"{AppData}\\Temp\\BOTW\\lnk.sh";
            if (!File.Exists(shell))
                await Download.FromUrl(DownloadLinks.LnkWriter, shell);

            if (shortcut.Start)
            {
                if (shortcut.IconFile.StartsWith("https"))
                {
                    using (HttpClient client = new())
                        await File.WriteAllBytesAsync($"{Root}\\{shortcut.Name.ToLower()}.ico", await client.GetByteArrayAsync(shortcut.IconFile));

                    shortcut.IconFile = $"{Root}\\{shortcut.Name.ToLower()}.ico";
                }

                await HiddenProcess.Start(shell, $"/F:\"{StartMenu}\\{shortcut.Name}.lnk\" /A:C /T:\"{shortcut.Target}\" /P:\"{shortcut.Args}\" /I:\"{shortcut.IconFile}\" /D:\"{shortcut.Description}\"");

                if (shortcut.BatchFile.StartsWith("https:"))
                {
                    using HttpClient client = new();
                    await File.WriteAllTextAsync($"{Root}\\{shortcut.Name.ToLower()}.bat", (await client.GetStringAsync(shortcut.BatchFile)).EvaluateVariables());
                }
                else
                {
                    await File.WriteAllTextAsync($"{Root}\\{shortcut.Name.ToLower()}.bat", shortcut.BatchFile.EvaluateVariables());
                }

                if (shortcut.HasUninstaller)
                {
                    await shortcut.AddProgramEntry();
                }
            }

            if (shortcut.Desktop)
            {
                await HiddenProcess.Start(shell, $"/F:\"{Desktop}\\{shortcut.Name}.lnk\" /A:C /T:\"{shortcut.Target}\" /P:\"{shortcut.Args}\" /I:\"{shortcut.IconFile}\" /D:\"{shortcut.Description}\"");
            }
        }

        /// <summary>
        /// Replaces any found variables in a string with the corresponding data
        /// <code>
        /// Environment -
        /// $desktop: Desktop folder
        /// $drive: Optimal drive (Second to first)
        /// $last_drive: Last drive (A-Z)
        /// $python: Python install directory
        /// $root: BotwInstaller root directory
        /// $start: Programs start menu fodler
        /// $user: User profile folder
        /// 
        /// BotW -
        /// $base: Path to the BotW base game files
        /// $update: Path to the BotW update files
        /// $dlc: Path to the BotW DLC files
        /// 
        /// Apps -
        /// $cemu: Path to the Cemu install directory
        /// $ds4: Path to the DS4Windows install directory
        /// </code>
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string EvaluateVariables(this string file)
        {
            Dictionary<string, string> defaults = new()
            {
                { "$desktop", Desktop },
                { "$drive", Drive },
                { "$last_drive", LastDrive },
                { "$python", Conf.Dirs.Python },
                { "$root", Root },
                { "$start", StartMenu },
                { "$user", User },
                { "$base", Conf.Dirs.Base },
                { "$update", Conf.Dirs.Update },
                { "$dlc", Conf.Dirs.DLC },
                { "$cemu", Conf.Dirs.Dynamic },
                { "$ds4", Conf.Dirs.DS4Windows },
            };

            foreach (var entry in defaults)
                file = file.Replace(entry.Key, entry.Value);

            foreach (var entry in Variables)
                file = file.Replace(entry.Key, entry.Value);

            Variables.Clear();

            return file;
        }

        /// <summary>
        /// Adds a program key to the registry with data from the given <paramref name="shortcut"/>
        /// </summary>
        /// <param name="shortcut">Shortcut data class</param>
        public static async Task AddProgramEntry(this ShortcutClass shortcut)
        {
            await Task.Run(() =>
            {
                Microsoft.Win32.Registry.SetValue(
                    @$"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{shortcut.Name}", "DisplayIcon", shortcut.IconFile
                );
                Microsoft.Win32.Registry.SetValue(
                    @$"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{shortcut.Name}", "DisplayName", shortcut.Name
                );
                Microsoft.Win32.Registry.SetValue(
                    @$"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{shortcut.Name}", "NoModify", 1
                );
                Microsoft.Win32.Registry.SetValue(
                    @$"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{shortcut.Name}", "UninstallString", $"{Root}\\{shortcut.Name.ToLower()}.bat"
                );
            });
        }

        public static Config Conf { get; set; } = new();
        public static Dictionary<string, string> Variables { get; set; } = new();
    }
}
