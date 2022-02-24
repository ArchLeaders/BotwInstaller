using static BotwInstaller.Lib.Config;

using BotwScripts.Lib.Common;
using BotwScripts.Lib.Common.Computer.Software;
using BotwScripts.Lib.Common.Computer.Software.Resources;
using BotwScripts.Lib.Common.IO.FileSystems;
using BotwScripts.Lib.Common.Web;
using BotwScripts.Lib.Common.Web.GitHub;
using System.IO.Compression;

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
        public static async Task Cemu(Interface.YesNoOption option, Interface.Update update, Interface.Notify notify, Config conf)
        {
            /*
            
            List<Task> download = new List<Task>();
            List<Task> unpack = new List<Task>();
            List<Task> install = new List<Task>();

            // Create random folder name to ensure nothing is overwritten. (Maybe a bit overkill)
            // string rand = $"{conf.Dirs.Cemu.EditPath()}{new Random().Next(1000, 9999)}-InstallCemuTemp";
            Directory.CreateDirectory($"{AppData}\\Temp\\BOTW");
            update(5, "Cemu");

            try
            {
                // Download Cemu
                notify("Downloading Cemu . . .");
                download.Add(Download.FromUrl(DownloadLinks.Cemu, $"{AppData}\\Temp\\CEMU.PACK.res"));

                // Download GFX
                notify("Downloading GFX . . .");
                download.Add(Download.FromUrl(await GitHub.GetLatestRelease("ActualMandM;cemu_graphic_packs"), $"{AppData}\\Temp\\GFX.PACK.res"));

                // Download CemuHook
                notify("Downloading CemuHook . . .");
                download.Add(Download.FromUrl("https://files.sshnuke.net/cemuhook_1262d_0577.zip", $"{AppData}\\Temp\\CEMUHOOK.PACK.res"));

                // Downlaod lnk icons
                if (conf.Shortcuts.BotW.Start || conf.Shortcuts.BotW.Desktop)
                    download.Add(Download.FromUrl("https://github.com/ArchLeaders/Botw-Installer/raw/master/Resources/Botw.ico.res", $"{Root}\\Botw.ico"));

                // Install runtimes
                download.Add(RuntimeInstallers.VisualCRuntime(notify));

                // Wait for download
                update(45, "Cemu"); await Task.WhenAll(download);

                // Unpack cemu to the temp folder
                notify("Extracting Cemu . . .");
                unpack.Add(Task.Run(() => ZipFile.ExtractToDirectory($"{AppData}\\Temp\\CEMU.PACK.res", $"{AppData}\\Temp\\BOTW\\CEMU")));

                // Unpack GFX to the temp folder
                notify("Extracting GFX . . .");
                unpack.Add(Task.Run(() => ZipFile.ExtractToDirectory($"{AppData}\\Temp\\GFX.PACK.res", $"{AppData}\\Temp\\BOTW\\GFX")));

                // Unpack CemuHook to the temp folder
                notify("Extracting CemuHook . . .");
                unpack.Add(Task.Run(() => ZipFile.ExtractToDirectory($"{AppData}\\Temp\\CEMUHOOK.PACK.res", $"{AppData}\\Temp\\BOTW\\CEMUHOOK")));

                // Wait for unpack
                update(65, "Cemu"); await Task.WhenAll(unpack);

                // Add GFX folder
                notify("Installing Cemu . . .");
                await Task.Run(() => Directory.CreateDirectory($"{conf.Dirs.Cemu}\\graphicPacks"));
                await Task.Run(() => Directory.CreateDirectory($"{conf.Dirs.Cemu}\\controllerProfiles"));

                // Copy Cemu folders
                install.Add(Task.Run(() => {
                    foreach (var dir in Directory.EnumerateDirectories($"{AppData}\\Temp\\CEMU"))
                        Directory.Move(dir, $"{conf.Dirs.Cemu}\\{new DirectoryInfo(dir).Name}");
                }));

                // Copy Cemu files
                install.Add(Task.Run(() => {
                    foreach (var file in Directory.EnumerateFiles($"{AppData}\\Temp\\CEMU"))
                        File.Move(file, $"{conf.Dirs.Cemu}\\{new FileInfo(file).Name}", true);
                }));

                // Move the GFX to the install directory
                notify("Installing GFX . . .");
                install.Add(Task.Run(() => Directory.Move($"{AppData}\\Temp\\BOTW\\GFX", $"{conf.Dirs.Cemu}\\graphicPacks\\downloadedGraphicPacks")));

                // Wait for install
                update(85, "Cemu"); await Task.WhenAll(install);

                // Configure settings, profiles, and controllers
                notify("Configuring Cemu . . .");
                await Task.Run(async () =>
                {
                    // Create new controller
                    CemuController ctrl = new();

                    // Write JP controller
                    CemuController.Write(ctrl, conf.Dirs.Cemu);

                    // Assign first 4 buttons
                    ctrl.Common.A = "button_1";
                    ctrl.Common.B = "button_2";
                    ctrl.Common.X = "button_8";
                    ctrl.Common.Y = "button_4";

                    // Write US
                    CemuController.Write(ctrl, conf.Dirs.Cemu, "Controller_US");

                    // Assign first 4 buttons
                    ctrl.Common.A = "button_1";
                    ctrl.Common.B = "button_2";
                    ctrl.Common.X = "button_4";
                    ctrl.Common.Y = "button_8";

                    // Write PE controller
                    CemuController.Write(ctrl, conf.Dirs.Cemu, "Controller_PE");

                    // Create new settings instance
                    CemuSettings settings = new();

                    // Configure settings
                    settings.MlcPath = conf.MlcDir;
                    settings.GamePaths.Entry = conf.BaseDir;
                    settings.GameCache.Entry = new()
                    {
                        TitleId = Convert.ToInt64(titleId, 16),
                        Name = "The Legend of Zelda: Breath of the Wild",
                        Version = 208,
                        DlcVersion = 80,
                        Path = $"{conf.BaseDir.EditPath()}code\\U-King.rpx"
                    };

                    settings.GraphicPack.Entry = new EntryElement[]
                    {
                         new()
                         {
                             Filename = @"graphicPacks\downloadedGraphicPacks\BreathOfTheWild\Mods\FPS++\rules.txt",
                             Preset = new Preset[]
                             {
                                 new() { Category = "Fence Type", PresetPreset = "Performance Fence (Default)" },
                                 new() { Category = "Mode", PresetPreset = "Advanced Settings" },
                                 new() { Category = "FPS Limit", PresetPreset = "60FPS Limit (Default)" },
                                 new() { Category = "Framerate Limit", PresetPreset = "30FPS (ideal for 240/120/60Hz displays)" },
                                 new() { Category = "Menu Cursor Fix (Experimental)", PresetPreset = "Enabled At 72FPS And Higher (Recommended)" },
                                 new() { Category = "Debug Options", PresetPreset = "Disabled (Default)" },
                                 new() { Category = "Static Mode", PresetPreset = "Disabled (Default, dynamically adjust game speed)" },
                                 new() { Category = "Frame Average", PresetPreset = "8 Frames Averaged (Default)" },
                             }
                         }
                    };

                    // Write settings.xml
                    using (FileStream stream = new($"{conf.Dirs.Cemu}\\settings.xml", FileMode.Create))
                    {
                        XmlSerializer xml = new(typeof(CemuSettings));
                        xml.Serialize(stream, settings);
                    }

                    // Write Botw game profile
                    CemuProfile.Write(new CemuProfile(), conf.Dirs.Cemu, titleId);

                    // Configure Cemu shortcuts
                    var lnkInfo = conf.Shortcuts.Cemu;

                    lnkInfo.Name = "Cemu";
                    lnkInfo.Target = $"{conf.Dirs.Cemu}\\Cemu.exe";
                    lnkInfo.Description = "WiiU Emulator made by Exzap and Petergov";
                    lnkInfo.IconFile = lnkInfo.Target;
                    lnkInfo.BatchFile = "@echo off\n" +
                        "echo Removing Cemu...\n" +
                        $"rmdir \"{conf.Dirs.Cemu}\" /s /q\n" +
                        $"reg delete HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Cemu /f\n" +
                        $"del /Q \"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\{lnkInfo.Name}.lnk\" /f\n" +
                        $"del /Q \"{Environment.GetFolderPath(Environment.SpecialFolder.StartMenu)}\\Programs\\{lnkInfo.Name}.lnk\" /f\n" +
                        $"del /Q \"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\BotW.lnk\" /f\n" +
                        $"del /Q \"{Environment.GetFolderPath(Environment.SpecialFolder.StartMenu)}\\Programs\\BotW.lnk\" /f\n" +
                        $"del /Q \"{Root}\\BotW.bat\" /f\n" +
                        "echo Done!\n" +
                        "pause\n" +
                        $"del /Q \"C:\\Users\\ArchLeaders\\AppData\\Local\\BotwData\\{lnkInfo.Name}Uninstaller.bat\" /f";

                    // Write Cemu Shortcuts
                    await LnkFile.Write(lnkInfo);

                    // Configure Botw shortcuts
                    lnkInfo = conf.Shortcuts.Botw;

                    var moduleStart = "";

                    if (!conf.Install.Ds4 && conf.Install.BetterJoy) moduleStart = $"\nstart /b \"BTJ\" \"{conf.BetterjoyDir}\\BetterjoyForCemu.exe\"";
                    else if (conf.Install.Ds4) moduleStart = $"\nstart /b \"DS4\" \"{conf.Ds4Dir}\\DS4Windows.exe\"";

                    lnkInfo.Name = "BotW";
                    lnkInfo.Target = $"cmd.exe";
                    lnkInfo.Description = "The Legend of Zelda: Breath of the Wild - Nintedo 2017";
                    lnkInfo.IconFile = $"{Root}\\Botw.ico";
                    lnkInfo.Args = $"/c \"{Root}\\BotW.bat\"";
                    lnkInfo.BatchFile = $"@echo off\n" +
                        $"start /b \"BOTW\" \"{conf.Dirs.Cemu}\\Cemu.exe\" -g \"{conf.BaseDir.EditPath()}code\\U-King.rpx\"" +
                        $"{moduleStart}\nEXIT";

                    // Write Botw Shortcuts
                    await LnkFile.Write(lnkInfo, true);
                });

                // Clear tasks
                download.Clear();
                unpack.Clear();
                install.Clear();
            }
            catch
            {
                throw;
            }
            finally
            {
                if (Directory.Exists(rand)) await Task.Run(() => Directory.Delete(rand, true));
                if (File.Exists($"{AppData}\\Temp\\CEMU.PACK.res")) File.Delete($"{AppData}\\Temp\\CEMU.PACK.res");
                if (File.Exists($"{AppData}\\Temp\\GFX.PACK.res")) File.Delete($"{AppData}\\Temp\\GFX.PACK.res");
                update(100, "Cemu");
            }

            */
        }
    }
}
