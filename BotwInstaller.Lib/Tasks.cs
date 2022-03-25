#pragma warning disable CS8602

using static BotwInstaller.Lib.Config;

using BotwScripts.Lib.Common;
using BotwScripts.Lib.Common.Computer.Software;
using BotwScripts.Lib.Common.Computer.Software.Resources;
using BotwScripts.Lib.Common.Web;
using BotwScripts.Lib.Common.Web.GitHub;
using System.IO.Compression;
using BotwInstaller.Lib.Configurations.Cemu;
using BotwScripts.Lib.Common.IO.FileSystems;
using BotwScripts.Lib.Common.Computer;
using BotwInstaller.Lib.Configurations;
using BotwScripts.Lib.Common.IO;
using System.Diagnostics;
using BotwInstaller.Lib.Remote;

namespace BotwInstaller.Lib
{
    /// <summary>
    /// Class used for installing various software and configurations
    /// </summary>
    public class Tasks
    {
        /// <summary>
        /// Installs and configures Cemu for BOTW.
        /// </summary>
        /// <param name="update"></param>
        /// <param name="print"></param>
        /// <param name="conf"></param>
        /// <returns></returns>
        public static async Task Cemu(Interface.Update update, Interface.Notify print, Config conf)
        {
            List<Task> download = new();
            List<Task> unpack = new();
            List<Task> install = new();

            Directory.CreateDirectory($"{AppData}\\Temp\\BOTW");
            Directory.CreateDirectory($"{conf.Dirs.Dynamic}\\graphicPacks");
            string func = "[INSTALL.CEMU]";
            update(5, "cemu");

            // Download Cemu
            print($"{func} Downloading Cemu . . .");
            download.Add(Download.FromUrl(DownloadLinks.Cemu, $"{AppData}\\Temp\\BOTW\\CEMU.PACK.res"));

            // Download GFX
            print($"{func} Downloading GFX . . .");
            download.Add(Download.FromUrl(await GitHub.GetLatestRelease("ActualMandM;cemu_graphic_packs"), $"{AppData}\\Temp\\BOTW\\GFX.PACK.res"));

            // Download DS4
            if (conf.ControllerApi == "DSUController")
            {
                print($"{func} Downloading DS4Windows . . .");
                download.Add(Download.FromUrl(await GitHub.GetLatestRelease("Ryochan7;DS4Windows", 2), $"{AppData}\\Temp\\BOTW\\DS4.PACK.res"));

                print($"{func} Downloading Virtual Gamepad Driver . . .");
                download.Add(Download.FromUrl(await GitHub.GetLatestRelease("ViGEm;ViGEmBus"), $"{AppData}\\Temp\\BOTW\\VIGEM.msi"));

                foreach (var proc in Process.GetProcesses())
                {
                    if (proc.ProcessName == "DS4Windows")
                    {
                        print($"{func} Found DS4Windows process {proc.Id}");
                        print($"{func} Killing process {proc.Id}");
                        proc.Kill();
                    }
                }
            }

            // Download CemuHook
            print($"{func} Downloading CemuHook . . .");
            download.Add(Download.FromUrl("https://files.sshnuke.net/cemuhook_1262d_0577.zip", $"{AppData}\\Temp\\BOTW\\CEMUHOOK.PACK.res"));

            // Wait for download
            await Task.WhenAll(download); update(45, "cemu");

            // Unpack cemu to the temp folder
            print($"{func} Extracting Cemu . . .");
            unpack.Add(Task.Run(() => ZipFile.ExtractToDirectory($"{AppData}\\Temp\\BOTW\\CEMU.PACK.res", $"{AppData}\\Temp\\BOTW\\CEMU")));

            // Unpack GFX to graphicPacks folder
            print($"{func} Extracting GFX . . .");
            unpack.Add(Task.Run(() => ZipFile.ExtractToDirectory($"{AppData}\\Temp\\BOTW\\GFX.PACK.res", $"{conf.Dirs.Dynamic}\\graphicPacks\\downloadedGraphicPacks", true)));

            // Unpack CemuHook to the temp folder
            print($"{func} Extracting CemuHook . . .");
            unpack.Add(Task.Run(() => ZipFile.ExtractToDirectory($"{AppData}\\Temp\\BOTW\\CEMUHOOK.PACK.res", $"{AppData}\\Temp\\BOTW\\CEMUHOOK")));

            // Download DS4
            if (conf.ControllerApi == "DSUController")
            {
                print($"{func} Extracting DS4Windows . . .");
                unpack.Add(Task.Run(() => ZipFile.ExtractToDirectory($"{AppData}\\Temp\\BOTW\\DS4.PACK.res", AppData, true)));

                print($"{func} Installing Virtual Gamepad Driver . . .");
                unpack.Add(HiddenProcess.Start("cmd.exe", $"/c \"{AppData}\\Temp\\BOTW\\VIGEM.msi\" /quiet & EXIT"));

                print($"{func} Installing Runtimes . . .");
                unpack.Add(RuntimeInstallers.Net5(print));
            }

            // Wait for unpack
            await Task.WhenAll(unpack); update(70, "cemu");

            // Install Cemu
            print($"{func} Installing Cemu . . .");
            install.Add(Task.Run(() => {
                // Setup fonders and variables
                Directory.CreateDirectory($"{conf.Dirs.Dynamic}\\sharedFonts");
                string cemuTemp = $"{AppData}\\Temp\\BOTW\\CEMU".SubFolder();
                string[] sharedFonts = new string[] { "CafeCn.ttf", "CafeTw.ttf", "CafeStd.ttf", "CafeKr.ttf" };

                // Copy sharedFonts
                foreach (string font in sharedFonts)
                    File.Copy($"{cemuTemp}\\resources\\sharedFonts\\{font}", $"{conf.Dirs.Dynamic}\\sharedFonts\\{font}", true);

                // Copy Cemu files
                foreach (var file in Directory.EnumerateFiles(cemuTemp, "*.*", SearchOption.AllDirectories))
                {
                    var dir = new FileInfo(file).DirectoryName;
                    Directory.CreateDirectory($"{conf.Dirs.Dynamic}\\{dir.Replace(cemuTemp, "")}");
                    File.Copy(file, $"{conf.Dirs.Dynamic}\\{file.Replace(cemuTemp, "")}", true);
                }
            }));

            // Install CemuHook
            print($"{func} Installing CemuHook . . .");
            install.Add(Task.Run(() => {
                string cemuHookTemp = $"{AppData}\\Temp\\BOTW\\CEMUHOOK";
                foreach (var file in Directory.EnumerateFiles(cemuHookTemp, "*.*", SearchOption.AllDirectories))
                    File.Copy(file, $"{conf.Dirs.Dynamic}\\{file.Replace(cemuHookTemp, "")}", true);
            }));

            // Configure settings, profiles, and controllers
            print($"{func} Configuring Cemu . . .");
            install.Add(Task.Run(() =>
            {
                // Create new controller profile
                ControllerProfile.Write(conf, conf.ControllerApi);

                // Create Cemu settings
                if (File.Exists($"{conf.Dirs.Dynamic}\\settings.xml") || File.Exists($"{AppData}\\Temp\\BOTW\\OVERRIDE"))
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

        /// <summary>
        /// Installs and configures BCML
        /// </summary>
        /// <param name="update"></param>
        /// <param name="print"></param>
        /// <param name="conf"></param>
        /// <returns></returns>
        public static async Task BCML(Interface.Update update, Interface.Notify print, Config conf)
        {
            string func = "[INSTALL.BCML]";

            update(5, "bcml");
            if (!File.Exists($"{conf.Dirs.Python}\\Scripts\\pip.exe"))
            {
                print($"{func} Installing PIP");
                await Download.FromUrl(HttpLinks.PipInstaller, $"{AppData}\\botw\\get-pip.py");
                await HiddenProcess.Start($"{conf.Dirs.Python}\\Scripts\\python.exe", $"\"{AppData}\\botw\\get-pip.py\"");
                await HiddenProcess.Start($"{conf.Dirs.Python}\\Scripts\\python.exe", "-m pip install --upgrade pip");
            }

            List<Task> pip = new();

            print($"{func} Installing BCML");
            update(8, "bcml");
            pip.Add(HiddenProcess.Start($"{conf.Dirs.Python}\\Scripts\\pip.exe", "install --force-reinstall bcml"));

            print($"{func} Installing CEF");
            update(14, "bcml");
            pip.Add(HiddenProcess.Start($"{conf.Dirs.Python}\\Scripts\\pip.exe", "install --force-reinstall cefpython3"));

            print($"{func} Installing TK");
            update(16, "bcml");
            pip.Add(HiddenProcess.Start($"{conf.Dirs.Python}\\Scripts\\pip.exe", "install --force-reinstall tk"));

            print($"{func} Installing Runtimes");
            update(18, "bcml");
            pip.Add(RuntimeInstallers.VisualCRuntime(print));

            print($"{func} Creating Settings");
            update(20, "bcml");
            pip.Add(BcmlSettings.Write(conf));

            await Task.WhenAll(pip);
            update(35, "bcml");
        }

        /// <summary>
        /// Installs the defined mod preset with BCML
        /// </summary>
        /// <param name="update"></param>
        /// <param name="print"></param>
        /// <param name="conf"></param>
        /// <returns></returns>
        public static async Task Mods(Interface.Update update, Interface.Notify print, Config conf)
        {
            List<Task> install = new();
            List<string?> mods = conf.ModPacks["wiiu"][conf.ModPack];
            string func = "[INSTALL.BCML.MODS]";

            if (conf.IsNX) mods = conf.ModPacks["switch"][conf.ModPack];

            await Download.FromUrl(HttpLinks.ModInstaller, $"{AppData}\\Temp\\BOTW\\python.py"); // skip this; run directly in the python interpreter
            update(50, "bcml");

            int i = 1;
            foreach (var mod in mods)
            {
                if (mod != null)
                {
                    install.Add(Task.Run(async() =>
                    {
                        string last = mods.Count == i ? "True" : "False";

                        print($"{func} Downloading {mod} . . .");
                        await Download.FromUrl(mod, $"{AppData}\\Temp\\BOTW\\MOD__{i}.bnp");

                        print($"{func} Installing {mod} . . .");
                        await HiddenProcess.Start( $"{conf.Dirs.Python}\\python.exe",
                            $"from bcml import install; from pathlib import Path; install.install_mod(Path('{AppData}\\Temp\\BOTW\\MOD__{i}.bnp'), merge_now={last})");

                        i++;
                    }));
                }
            }

            await Task.WhenAll(install);
            update(80, "bcml");

            if (Directory.Exists($"{conf.Dirs.BCML}\\mods\\9999_BCML"))
            {
                print($"{func} Merging mods . . .");

                await Task.Run(() =>
                    Batch.CopyDirectory($"{conf.Dirs.BCML}\\mods\\9999_BCML", conf.UseCemu ? $"{conf.Dirs.Dynamic}\\graphicPacks\\BreathOfTheWild_BCML" : conf.Dirs.Dynamic)
                );
            }

            update(95, "bcml");
        }

        public static async Task CopyFolderAsync(Interface.Notify print, Interface.Update update, int updateValue, int fileCount, string inputDir, string outputDir, string func = "[INSTALL.BOTW]")
        {
            List<Task> ops = new();

            print($"{func} Copying {fileCount} files . . .");

            double inc = updateValue/(fileCount/500.0);
            int index = 0;

            foreach (var file in Directory.EnumerateFiles(inputDir, "*.*", SearchOption.AllDirectories))
            {
                ops.Add(Task.Run(() =>
                {
                    // Collect the DirectoryInfo & FileInfo
                    FileInfo fileInfo = new(file);
                    DirectoryInfo directoryInfo = new(fileInfo.DirectoryName);

                    // Create the parent directory
                    Directory.CreateDirectory(directoryInfo.FullName.Replace(inputDir, outputDir));

                    // Copy the file
                    File.Copy(file, file.Replace(inputDir, outputDir), true);

                    index++;

                    if (index == 1000)
                    {
                        update(inc, "game");
                        index = 0;
                    }

                }));
            }

            await Task.WhenAll(ops);

            print($"{func} Copied {fileCount} files");
        }
    }
}
