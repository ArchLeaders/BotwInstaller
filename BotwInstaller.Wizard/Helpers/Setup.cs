#pragma warning disable CS8602

using BotwInstaller.Lib;
using BotwInstaller.Lib.Configurations.Cemu;
using BotwScripts.Lib.Common;
using BotwScripts.Lib.Common.Computer;
using BotwScripts.Lib.Common.Computer.Software.Resources;
using BotwScripts.Lib.Common.IO.FileSystems;
using BotwScripts.Lib.Common.Web;
using Stylet;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace BotwInstaller.Wizard.Helpers
{
    public class Setup
    {
        public static async Task Tiramisu(IWindowManager win, Interface.Update update)
        {
            win.Show("Select an empty folder or SDCard to install Tiramisu onto.");

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
                        win.Show($"The folder '{dir}' was not empty", "Warning");
                        return;
                    }
                }

                foreach (var file in Directory.EnumerateFiles(browse.SelectedPath))
                {
                    if (file != null && !file.EndsWith("System Volume Information"))
                    {
                        win.Show($"The folder '{file.EditPath()}' was not empty", "Warning");
                        return;
                    }
                }

                await Task.Run(async () =>
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

                win.Show($"Tiramisu installed successfully");
            }
        }

        public static void Hekate(IWindowManager win, Interface.Update update) // make async Task
        {
            // Install the APX drivers for TegraRCMSmash?

            win.Show("Your Switch must be vulnerable to fusee-gelee for the installed homebrew apps to work correctly.\n" +
                "See [this guide](https://switch.homebrew.guide/gettingstarted/checkingrcm.html) to get your switch info.\n\nContinue?", isYesNo: true, width: 300);


        }

        public static async Task CemuOnline(IWindowManager win, Interface.Update update, string dir)
        {
            if (File.Exists($"{dir}\\Cemu.exe"))
            {
                if (win.Show("Cemu already exists in this directory.\nOpen Cemu?", isYesNo: true))
                    await HiddenProcess.Start($"{dir}\\Cemu.exe");
                return;
            }

            // Create Temp Directory
            Directory.CreateDirectory($"{Config.AppData}\\Temp\\CEMU");
            update(50, "!time");
            update(80, "tool");

            // Download Cemu
            await Download.FromUrl(DownloadLinks.Cemu, $"{Config.AppData}\\Temp\\CEMU\\CEMU.PACK.res");
            update(85, "tool");

            // Extract Cemu
            await Task.Run(() => ZipFile.ExtractToDirectory($"{Config.AppData}\\Temp\\CEMU\\CEMU.PACK.res", $"{Config.AppData}\\Temp\\CEMU\\CEMU"));
            update(90, "tool");

            // Install Cemu
            await Task.Run(() => {

                // Setup fonders and variables
                string cemuTemp = $"{Config.AppData}\\Temp\\CEMU\\CEMU".SubFolder();

                // Copy Cemu files
                foreach (var file in Directory.EnumerateFiles(cemuTemp, "*.*", SearchOption.AllDirectories))
                {
                    var dir = new FileInfo(file).DirectoryName;
                    Directory.CreateDirectory($"{dir}\\{dir.Replace(cemuTemp, "")}");
                    File.Copy(file, $"{dir}\\{file.Replace(cemuTemp, "")}", true);
                }
            });

            update(95, "tool");

            // Create override tag
            Directory.CreateDirectory($"{Config.AppData}\\Temp\\BOTW");
            await File.WriteAllTextAsync($"{Config.AppData}\\Temp\\BOTW\\OVERRIDE", "");

            // Write basic settings
            Directory.CreateDirectory($"{dir}\\mlc01");
            CemuSettings.Write(new() { Dirs = { Dynamic = dir } }, true);

            // Delete Temp Directory
            Directory.Delete($"{Config.AppData}\\Temp\\CEMU", true);

            update(100, "tool");

            win.Show("Cemu Installed successfully.");
            await HiddenProcess.Start($"{dir}\\Cemu.exe");
        }
    }
}
