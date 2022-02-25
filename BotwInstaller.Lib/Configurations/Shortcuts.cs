#pragma warning disable CA1416

using BotwScripts.Lib.Common.Computer;
using BotwScripts.Lib.Common.Computer.Software.Resources;
using BotwScripts.Lib.Common.Web;
using static BotwInstaller.Lib.Config;

namespace BotwInstaller.Lib.Configurations.Shortcuts
{
    public static class Shortcuts
    {
        public static async Task Write(ShortcutClass lnk)
        {
            string shell = $"{AppData}\\Temp\\BOTW\\lnk.shell";
            if (!File.Exists(shell))
                await Download.FromUrl(DownloadLinks.LnkWriter, shell);

            if (lnk.Start)
            {
                await HiddenProcess.Start(shell, $"/F:\"{StartMenu}\\{lnk.Name}.lnk\" /A:C /T:\"{lnk.Target}\" /P:\"{lnk.Args}\" /I:\"{lnk.IconFile}\" /D:\"{lnk.Description}\"");

                if (lnk.BatchFile.StartsWith("https:"))
                {
                    using (HttpClient client = new())
                        await File.WriteAllTextAsync($"{Root}\\{lnk.Name.ToLower()}.bat", (await client.GetStringAsync(lnk.BatchFile)).EvaluateVariables());
                }
                else
                {
                    await File.WriteAllTextAsync($"{Root}\\{lnk.Name.ToLower()}.bat", lnk.BatchFile.EvaluateVariables());
                }

                if (!lnk.HasUninstaller)
                {
                    await lnk.AddProgramEntry();
                }
            }

            if (lnk.Desktop)
            {
                await HiddenProcess.Start(shell, $"/F:\"{StartMenu}\\{lnk.Name}.lnk\" /A:C /T:\"{lnk.Target}\" /P:\"{lnk.Args}\" /I:\"{lnk.IconFile}\" /D:\"{lnk.Description}\"");
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
                { "$cemu", Conf.Dirs.Cemu },
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
        /// Adds a program key to the registry
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <param name="uninstall"></param>
        /// <param name="icon"></param>
        public static async Task AddProgramEntry(this ShortcutClass lnk)
        {
            await Task.Run(() =>
            {
                Microsoft.Win32.Registry.SetValue(
                    @$"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{lnk.Name}", "DisplayIcon", lnk.IconFile
                );
                Microsoft.Win32.Registry.SetValue(
                    @$"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{lnk.Name}", "DisplayName", lnk.Name
                );
                Microsoft.Win32.Registry.SetValue(
                    @$"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{lnk.Name}", "NoModify", 1
                );
                Microsoft.Win32.Registry.SetValue(
                    @$"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{lnk.Name}", "UninstallString", $"{Root}\\{lnk.Name.ToLower()}.bat"
                );
            });
        }

        public static Config Conf { get; set; } = new();

        public static Dictionary<string, string> Variables { get; set; } = new();
    }
}
