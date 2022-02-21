using BotwScripts.Lib.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotwInstaller.Lib
{
    public class Config
    {
        public static string AppData { get; } = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static string User { get; } = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        public static string Drive { get; } = DriveInfo.GetDrives()[DriveInfo.GetDrives().Length - 1 - (DriveInfo.GetDrives().Length - 2)].Name;

        /// <summary>
        /// Directory list
        /// </summary>
        public DirsClass Dirs { get; set; } = new();

        /// <summary>
        /// Install list
        /// </summary>
        public InstallClass Install { get; set; } = new();

        /// <summary>
        /// Shortcut instances
        /// </summary>
        public GroupShortcutClass Shortcuts { get; set; } = new();

        /// <summary>
        /// Directory list class
        /// </summary>
        public class DirsClass
        {
            /// <summary>
            /// Directory in which the BotW base game files are stored
            /// </summary>
            public string Base { get; set; } = "";

            /// <summary>
            /// BCML's data directory
            /// </summary>
            public string BCML { get; set; } = Drive == "C:" ? $"{AppData}\\bcml" : $"{Drive}\\Games\\BotW\\BCML Data";

            /// <summary>
            /// BetterJoy installation directory
            /// </summary>
            public string BetterJoy { get; set; } = Drive == "C:" ? $"{AppData}\\BetterJoy" : $"{Drive}\\Games\\BotW\\BetterJoy";

            /// <summary>
            /// Cemu installation directory
            /// </summary>
            public string Cemu { get; set; } = Drive == "C:" ? $"{User}\\Cemu" : $"{Drive}\\Games\\BotW\\Cemu";

            /// <summary>
            /// DS4Windows installation directory
            /// </summary>
            public string DS4Windows { get; set; } = Drive == "C:" ? $"{AppData}\\DS4Windows" : $"{Drive}\\Games\\BotW\\DS4Windows";

            /// <summary>
            /// Directory in which the BotW DLC files are stored
            /// </summary>
            public string DLC { get; set; } = "";

            /// <summary>
            /// Cemu's data directory
            /// </summary>
            public string MLC01 { get; set; } = "";

            /// <summary>
            /// Directory in which Python is installed in
            /// </summary>
            public string Python { get; set; } = "C:\\Python3";

            /// <summary>
            /// Directory in which the BotW update files are stored
            /// </summary>
            public string Update { get; set; } = "";
        }

        /// <summary>
        /// Install list class
        /// </summary>
        public class InstallClass
        {
            /// <summary>
            /// Install the Base game in Cemu
            /// </summary>
            public bool Base { get; set; } = true;

            /// <summary>
            /// Install BCML
            /// </summary>
            public bool BCML { get; set; } = true;

            /// <summary>
            /// Install BetterJoy
            /// </summary>
            public bool BetterJoy { get; set; } = false;

            /// <summary>
            /// Install Cemu
            /// </summary>
            public bool Cemu { get; set; } = true;

            /// <summary>
            /// Install the DLC in Cemu
            /// </summary>
            public bool DLC { get; set; } = false;

            /// <summary>
            /// Install DS4Windows
            /// </summary>
            public bool DS4Windows { get; set; } = false;

            /// <summary>
            /// Install the update in Cemu
            /// </summary>
            public bool Update { get; set; } = true;

            /// <summary>
            /// Install python
            /// </summary>
            public bool Python { get; set; } = true;

            /// <summary>
            /// Install the python documentaion
            /// </summary>
            public bool PythonDocs { get; set; } = true;
        }

        /// <summary>
        /// Shortcut meta data instances class
        /// </summary>
        public class GroupShortcutClass
        {
            /// <summary>
            /// Shortcut meta data for BCML
            /// </summary>
            public ShortcutClass BCML { get; set; } = new();

            /// <summary>
            /// Shortcut meta data for BetterJoy
            /// </summary>
            public ShortcutClass BetterJoy { get; set; } = new();

            /// <summary>
            /// Shortcut meta data for BotW
            /// </summary>
            public ShortcutClass BotW { get; set; } = new();

            /// <summary>
            /// Shortcut meta data for Cemu
            /// </summary>
            public ShortcutClass Cemu { get; set; } = new();

            /// <summary>
            /// Shortcut meta data for DS4Windows
            /// </summary>
            public ShortcutClass DS4Windows { get; set; } = new();
        }

        /// <summary>
        /// Shortcut meta data instance class
        /// </summary>
        public class ShortcutClass
        {
            /// <summary>
            /// Create a shortcut on the start menu and an uninstaller in the program files
            /// </summary>
            public bool Start { get; set; } = true;

            /// <summary>
            /// Create a shortut on the desktop
            /// </summary>
            public bool Desktop { get; set; } = true;

            /// <summary>
            /// Name of the shortcut
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// Target file the shortcut will open
            /// </summary>
            public string Target { get; set; } = "";

            /// <summary>
            /// Description of the shortcut
            /// </summary>
            public string Description { get; set; } = "";

            /// <summary>
            /// Path to the icon (ico, dll, exe) of the shortcut.
            /// </summary>
            public string IconFile { get; set; } = "";

            /// <summary>
            /// Path to uninstaller or run script.
            /// </summary>
            public string BatchFile { get; set; } = "";
        }

        public static async Task RunInstallerAsync(Config c)
        {
            Config conf = new();

            /// Get system information and update config -\-
            
            Dictionary<string, object> gameInfo = await GameInfo.GetFiles(conf.Dirs.Cemu, conf.Dirs.Python);

            conf.Dirs.Cemu = (string)gameInfo["Cemu"] == "NOT FOUND" ? conf.Dirs.Cemu : (string)gameInfo["Cemu"];
            conf.Dirs.Python = (string)gameInfo["Python"] == "NOT FOUND" ? conf.Dirs.Python : (string)gameInfo["Python"];

            conf.Dirs.Base = (string)gameInfo["Game"] == "NOT FOUND" ? "" : (string)gameInfo["Game"];
            conf.Dirs.Update = (string)gameInfo["Update"] == "NOT FOUND" ? "" : (string)gameInfo["Update"];
            conf.Dirs.DLC = (string)gameInfo["DLC"] == "NOT FOUND" ? "" : (string)gameInfo["DLC"];

            conf.Install.Base = !(bool)gameInfo["Game_IsInstalled"];
            conf.Install.Update = !(bool)gameInfo["Update_IsInstalled"];
            conf.Install.DLC = !(bool)gameInfo["DLC_IsInstalled"];
            conf.Install.Cemu = !File.Exists($"{gameInfo["Cemu"]}\\Cemu.exe");
            conf.Install.Python = !File.Exists($"{gameInfo["Python"]}\\python38.dll") || File.Exists($"{gameInfo["Python"]}\\python37.dll");

            /// Get system information and update config -/-

            /// END BLOCK ONE

            /// 
        }
    }
}
