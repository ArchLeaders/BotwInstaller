﻿using BotwInstaller.Lib.Configurations.Shortcuts;
using BotwInstaller.Lib.Remote;
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
        public static string Desktop { get; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public static string StartMenu { get; } = $"{Environment.GetFolderPath(Environment.SpecialFolder.StartMenu)}\\Programs";
        public static string Root { get; } = $"{AppData}\\botw";
        public static string User { get; } = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        public static string Drive { get; } = DriveInfo.GetDrives()[DriveInfo.GetDrives().Length - 1 - (DriveInfo.GetDrives().Length - 2)].Name.Replace("\\", "");
        public static string LastDrive { get; } = DriveInfo.GetDrives()[DriveInfo.GetDrives().Length - 1].Name.Replace("\\", "");

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
        /// Controller Api to be used in Cemu
        /// <code>
        /// Default: XInput
        /// Options: XInput, DSUController, SDLController
        /// </code>
        /// </summary>
        public string ControllerApi { get; set; } = "XInput";

        /// <summary>
        /// Use Cemu in BCML
        /// </summary>
        public bool UseCemu { get; set; } = false;

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
            /// Install Cemu
            /// </summary>
            public bool Cemu { get; set; } = false;

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
        }

        /// <summary>
        /// Shortcut meta data instances class
        /// </summary>
        public class GroupShortcutClass
        {
            /// <summary>
            /// Shortcut meta data for BCML
            /// </summary>
            public ShortcutClass BCML
            {
                get
                {
                    return new()
                    {
                        Name = "BCML",
                        Target = "$python\\python.exe".EvaluateVariables(),
                        Args = "-m bcml",
                        IconFile = "$root\\bcml.ico".EvaluateVariables(),
                        Description = "Breath of the Wild Cross-Platform Mod Loader developed by Caleb Smith and GingerAvalanche",
                        BatchFile = HttpLinks.BcmlBatchFile
                    };
                }
            }

            /// <summary>
            /// Shortcut meta data for BotW
            /// </summary>
            public ShortcutClass BotW
            {
                get
                {
                    return new()
                    {
                        Name = "BOTW",
                        Target = "$root\\botw.bat".EvaluateVariables(),
                        IconFile = "$root\\botw.ico".EvaluateVariables(),
                        Description = "Breath of the Wild developed by Nintendo",
                        BatchFile = HttpLinks.BotwBatchFile,
                        HasUninstaller = false
                    };
                }
            }

            /// <summary>
            /// Shortcut meta data for Cemu
            /// </summary>
            public ShortcutClass Cemu
            {
                get
                {
                    return new()
                    {
                        Name = "Cemu",
                        Target = "$cemu\\Cemu.exe".EvaluateVariables(),
                        IconFile = "$cemu\\Cemu.exe".EvaluateVariables(),
                        Description = "WiiU Emulator developed by Exzap and Petergov",
                        BatchFile = HttpLinks.CemuBatchFile
                    };
                }
            }

            /// <summary>
            /// Shortcut meta data for DS4Windows
            /// </summary>
            public ShortcutClass DS4Windows
            {
                get
                {
                    return new()
                    {
                        Name = "DS4Windows",
                        Target = "$ds4\\DS4Windows.exe".EvaluateVariables(),
                        IconFile = "$root\\bcml.ico".EvaluateVariables(),
                        Description = "DualShock 4 for Windows developed by Jays2Kings and modified by Ryochan7",
                        BatchFile = HttpLinks.DS4BatchFile
                    };
                }
            }
        }
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
        public bool Desktop { get; set; } = false;

        /// <summary>
        /// Name of the shortcut
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Target file the shortcut will open
        /// </summary>
        public string Target { get; set; } = "";

        /// <summary>
        /// Arguments to pass to the Target
        /// </summary>
        public string Args { get; set; } = "";

        /// <summary>
        /// Description of the shortcut
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Path to the icon (ico, dll, exe) of the shortcut.
        /// </summary>
        public string IconFile { get; set; } = "";

        /// <summary>
        /// Link to remote uninstaller or run script.
        /// </summary>
        public string BatchFile { get; set; } = "";

        /// <summary>
        /// Link to remote uninstaller or run script.
        /// </summary>
        public bool HasUninstaller { get; set; } = true;
    }
}
