﻿using BotwInstaller.Lib.Configurations.Shortcuts;
using BotwScripts.Lib.Common;
using BotwScripts.Lib.Common.Computer.Software;
using BotwScripts.Lib.Common.Computer.Software.Resources;
using BotwScripts.Lib.Common.IO;
using BotwScripts.Lib.Common.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotwInstaller.Lib
{
    public class Installer
    {
        public static async Task<Config> RunInstallerAsync(Interface.Notify print, Interface.Update update, Config conf)
        {
            // Get system information
            Dictionary<string, object> gameInfo = await GameInfo.GetFiles(print, conf.UseCemu ? conf.Dirs.Dynamic : "!ignore", conf.Dirs.Python, conf.IsNX);

            if (conf.IsNX)
            {
                conf.Dirs.Python = (string)gameInfo["Python"] == "NOT FOUND" ? conf.Dirs.Python : (string)gameInfo["Python"];
                conf.Install.Python = !(File.Exists($"{gameInfo["Python"]}\\python38.dll") || File.Exists($"{gameInfo["Python"]}\\python37.dll"));

                conf.Install.Base = false;
                conf.Install.Update = false;
                conf.Install.DLC = false;
            }
            else
            {
                // Set dictionary values to static parameters
                conf.Dirs.Python = (string)gameInfo["Python"] == "NOT FOUND" ? conf.Dirs.Python : (string)gameInfo["Python"];

                conf.Dirs.Base = (string)gameInfo["Game"] == "NOT FOUND" ? "" : (string)gameInfo["Game"];
                conf.Dirs.Update = (string)gameInfo["Update"] == "NOT FOUND" ? "" : (string)gameInfo["Update"];
                conf.Dirs.DLC = (string)gameInfo["DLC"] == "NOT FOUND" ? "" : (string)gameInfo["DLC"];

                if (conf.UseCemu)
                {
                    if (conf.Install.Base)
                        conf.Install.Base = !(bool)gameInfo["Game_IsInstalled"];

                    conf.Dirs.Dynamic = (string)gameInfo["Cemu"] == "NOT FOUND" ? conf.Dirs.Dynamic : (string)gameInfo["Cemu"];
                    conf.Dirs.MLC01 = (string)gameInfo["mlc"] == "NOT FOUND" ? conf.Dirs.MLC01 : (string)gameInfo["mlc"];
                    conf.Install.Cemu = !File.Exists($"{gameInfo["Cemu"]}\\Cemu.exe");
                    conf.Install.Update = !(bool)gameInfo["Update_IsInstalled"];
                    conf.Install.DLC = !(bool)gameInfo["DLC_IsInstalled"];
                }

                conf.Install.Python = !(File.Exists($"{gameInfo["Python"]}\\python38.dll") || File.Exists($"{gameInfo["Python"]}\\python37.dll"));
            }

            // Update shortcut parameters
            Shortcuts.Conf = conf;

            // Create install tasks
            List<Task> t1 = new();
            List<Task> t2 = new();

            // Install Python
            if (conf.Install.Python)
                t1.Add(RuntimeInstallers.Python(print, conf.Dirs.Python));

            // Install Cemu
            if (conf.UseCemu)
            {
                // Define mlc01
                string mlc01 = conf.Dirs.MLC01 == "" ? $"{conf.Dirs.Dynamic}\\mlc01" : conf.Dirs.MLC01;
                Directory.CreateDirectory(mlc01);

                // Install Game
                if (conf.Install.Base)
                {
                    t2.Add(
                        Batch.CopyDirectoryWithUpdate(update, print, 30, conf.Dirs.Base.FileCount(), "./install_log.txt", conf.Dirs.Base,
                            $"{mlc01}\\usr\\title\\00050000\\{GameInfo.GetTitleID(conf.Dirs.Base, TitleIDFormat.HexEnd)}"
                        )
                    );
                }

                // Install Update
                if (conf.Install.Update)
                {
                    t2.Add(
                        Batch.CopyDirectoryWithUpdate(
                            update, print, 30, conf.Dirs.Update.FileCount(), "./install_log.txt", conf.Dirs.Update,
                            $"{mlc01}\\usr\\title\\0005000e\\{GameInfo.GetTitleID(conf.Dirs.Update, TitleIDFormat.HexEnd)}"
                        )
                    );
                }

                // Install DLC
                if (conf.Install.DLC && conf.Dirs.DLC != "")
                {
                    t2.Add(
                        Batch.CopyDirectoryWithUpdate(
                            update, print, 30, conf.Dirs.DLC.FileCount(), "./install_log.txt", conf.Dirs.DLC,
                            $"{mlc01}\\usr\\title\\0005000c\\{GameInfo.GetTitleID(conf.Dirs.DLC, TitleIDFormat.HexEnd)}"
                        )
                    );
                }

                // Install Cemu
                t2.Add(Tasks.Cemu(update, print, conf));
            }

            await Task.WhenAll(t1);

            t2.Add(Tasks.BCML(update, print, conf));

            await Task.WhenAll(t2);

            // Install mods
            await Tasks.Mods(update, print, conf);

            // Create BCML shortcut(s)
            print($"[INSTALL] Creating BCML.lnk . . .");
            await Shortcuts.Write(conf.Shortcuts.BCML);

            if (conf.UseCemu)
            {
                // Create Cemu shortcut(s)
                print($"[INSTALL] Creating Cemu.lnk . . .");
                await Shortcuts.Write(conf.Shortcuts.Cemu);

                // Create Botw shortcut(s)
                print($"[INSTALL] Creating BOTW.lnk . . .");
                await Shortcuts.Write(conf.Shortcuts.BotW);
            }

            if (conf.ControllerApi == "DSUController" && conf.Install.Cemu)
            {
                // Create DS4Windows shortcut(s)
                print($"[INSTALL] Creating DS4Windows.lnk . . .");
                await Shortcuts.Write(conf.Shortcuts.DS4Windows);
            }

            update(100, "bcml");
            return conf;
        }
    }
}
