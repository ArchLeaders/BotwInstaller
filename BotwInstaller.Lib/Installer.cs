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
        public static async Task RunInstallerAsync(Interface.Notify print, Interface.Update update, Interface.Error error, Config c)
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
            conf.Install.Python = !File.Exists($"{gameInfo["Python"]}\\python38.dll") || !File.Exists($"{gameInfo["Python"]}\\python37.dll");

            /// Get system information and update config -/-

            /// END BLOCK ONE

            /// Install -\-

            List<Task> t1 = new();
            List<Task> t2 = new();

            // Install Python
            if (conf.Install.Python) t1.Add(RuntimeInstallers.Python(Interface.WriteLine, conf.Dirs.Python, conf.Install.PythonVersion, conf.Install.PythonDocs));

            /// Install -/-

        }
    }
}
