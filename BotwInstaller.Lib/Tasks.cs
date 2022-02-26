using static BotwInstaller.Lib.Config;

using BotwScripts.Lib.Common;
using BotwScripts.Lib.Common.Computer.Software;
using BotwScripts.Lib.Common.Computer.Software.Resources;
using BotwScripts.Lib.Common.Web;
using BotwScripts.Lib.Common.Web.GitHub;
using System.IO.Compression;
using BotwInstaller.Lib.Configurations.Cemu;
using BotwInstaller.Lib.Configurations.Shortcuts;
using BotwScripts.Lib.Common.IO.FileSystems;

namespace BotwInstaller.Lib
{
    public class Tasks
    {
        /// <summary>
        /// Installs and configures Cemu for Botw.
        /// </summary>
        /// <param name="option"></param>
        /// <param name="update"></param>
        /// <param name="notify"></param>
        /// <param name="installDir"></param>
        /// <param name="installGfx"></param>
        /// <param name="installRuntimes"></param>
        /// <returns></returns>
        public static async Task Cemu(Interface.Update update, Interface.Notify notify, Config conf)
        {
            List<Task> download = new List<Task>();
            List<Task> unpack = new List<Task>();
            List<Task> install = new List<Task>();

            Directory.CreateDirectory($"{AppData}\\Temp\\BOTW");
            Directory.CreateDirectory($"{conf.Dirs.Dynamic}\\graphicPacks");
            string func = "[INSTALL.CEMU]";
            update(5, "cemu");

            // Download Cemu
            notify($"{func} Downloading Cemu . . .");
            download.Add(Download.FromUrl(DownloadLinks.Cemu, $"{AppData}\\Temp\\BOTW\\CEMU.PACK.res"));

            // Download GFX
            notify($"{func} Downloading GFX . . .");
            download.Add(Download.FromUrl(await GitHub.GetLatestRelease("ActualMandM;cemu_graphic_packs"), $"{AppData}\\Temp\\BOTW\\GFX.PACK.res"));

            // Download CemuHook
            notify($"{func} Downloading CemuHook . . .");
            download.Add(Download.FromUrl("https://files.sshnuke.net/cemuhook_1262d_0577.zip", $"{AppData}\\Temp\\BOTW\\CEMUHOOK.PACK.res"));

            // Install runtimes
            notify($"{func} Installing runtimes . . .");
            install.Add(RuntimeInstallers.VisualCRuntime(notify));

            // Wait for download
            await Task.WhenAll(download); update(45, "cemu");

            // Unpack cemu to the temp folder
            notify($"{func} Extracting Cemu . . .");
            unpack.Add(Task.Run(() => ZipFile.ExtractToDirectory($"{AppData}\\Temp\\BOTW\\CEMU.PACK.res", $"{AppData}\\Temp\\BOTW\\CEMU")));

            // Unpack GFX to graphicPacks folder
            notify($"{func} Extracting GFX . . .");
            unpack.Add(Task.Run(() => ZipFile.ExtractToDirectory($"{AppData}\\Temp\\BOTW\\GFX.PACK.res", $"{conf.Dirs.Dynamic}\\graphicPacks\\downloadedGraphicPacks")));

            // Unpack CemuHook to the temp folder
            notify($"{func} Extracting CemuHook . . .");
            unpack.Add(Task.Run(() => ZipFile.ExtractToDirectory($"{AppData}\\Temp\\BOTW\\CEMUHOOK.PACK.res", $"{AppData}\\Temp\\BOTW\\CEMUHOOK")));

            // Wait for unpack
            await Task.WhenAll(unpack); update(70, "cemu");

            // Add GFX folder
            notify($"{func} Installing Cemu . . .");
            install.Add(Task.Run(() => {
                string cemuTemp = $"{AppData}\\Temp\\BOTW\\CEMU".SubFolder();
                foreach (var file in Directory.EnumerateFiles(cemuTemp, "*.*", SearchOption.AllDirectories))
                {
                    var dir = new FileInfo(file).DirectoryName;
                    Directory.CreateDirectory($"{conf.Dirs.Dynamic}\\{dir.Replace(cemuTemp, "")}");
                    File.Copy(file, $"{conf.Dirs.Dynamic}\\{file.Replace(cemuTemp, "")}");
                }
            }));

            // Group lnk creation
            install.Add(Task.Run(async () =>
            {
                // Create Cemu shortcuts
                notify($"{func} Creating Cemu.lnk . . .");
                await Shortcuts.Write(conf.Shortcuts.Cemu);

                // Configure Botw shortcuts
                notify($"{func} Creating BOTW.lnk . . .");
                await Shortcuts.Write(conf.Shortcuts.BotW);
            }));

            // Configure settings, profiles, and controllers
            notify($"{func} Configuring Cemu . . .");
            install.Add(Task.Run(() =>
            {
                // Create new controller profile
                ControllerProfile.Write(conf, conf.ControllerApi);

                // Create Cemu settings
                CemuSettings.Write(conf);

                // Write Botw game profile
                GameProfile.Write(conf);
            }));

            // Wait for install
            await Task.WhenAll(install);
            update(100, "cemu");

            // Clear tasks
            download.Clear();
            unpack.Clear();
            install.Clear();
        }
    }
}
