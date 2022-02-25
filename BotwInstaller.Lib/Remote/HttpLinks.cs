using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotwInstaller.Lib.Remote
{
    public class HttpLinks
    {
        public static string ModPresetsJson { get; } = "https://raw.githubusercontent.com/ArchLeaders/Botw-Installer/master/RE/TempDataFromRE.json";
        public static string BotwBatchFile { get; } = "";
        public static string BcmlBatchFile { get; } = "";
        public static string CemuBatchFile { get; } = "";
        public static string DS4BatchFile { get; } = "";

    }
}
