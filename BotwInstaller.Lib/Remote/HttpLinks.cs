namespace BotwInstaller.Lib.Remote
{
    /// <summary>
    /// Static class holding links to used remote files
    /// </summary>
    public static class HttpLinks
    {
        public static string ModPresets { get; } = "https://raw.githubusercontent.com/ArchLeaders/BotwInstaller/master/BotwInstaller.Lib/Configurations/Mods/ModPresets.json";
        public static string BcmlIconFile { get; } = "https://github.com/ArchLeaders/BotwInstaller/raw/master/BotwInstaller.Lib/Configurations/Icons/bcml.ico";
        public static string BotwIconFile { get; } = "https://github.com/ArchLeaders/BotwInstaller/raw/master/BotwInstaller.Lib/Configurations/Icons/botw.ico";
        public static string BotwBatchFile { get; } = "https://raw.githubusercontent.com/ArchLeaders/BotwInstaller/master/BotwInstaller.Lib/Configurations/BatchFiles/botw.bat";
        public static string BcmlBatchFile { get; } = "https://raw.githubusercontent.com/ArchLeaders/BotwInstaller/master/BotwInstaller.Lib/Configurations/BatchFiles/bcml.bat";
        public static string CemuBatchFile { get; } = "https://raw.githubusercontent.com/ArchLeaders/BotwInstaller/master/BotwInstaller.Lib/Configurations/BatchFiles/cemu.bat";
        public static string DS4BatchFile { get; } = "https://raw.githubusercontent.com/ArchLeaders/BotwInstaller/master/BotwInstaller.Lib/Configurations/BatchFiles/ds4windows.bat";
        public static string PipInstaller { get; } = "https://bootstrap.pypa.io/get-pip.py";
        public static string ModInstaller { get; } = "https://raw.githubusercontent.com/ArchLeaders/BotwInstaller/master/BotwInstaller.Lib/Configurations/Mods/mod_install.py";
        public static string ModRemerger { get; } = "https://raw.githubusercontent.com/ArchLeaders/BotwInstaller/master/BotwInstaller.Lib/Configurations/Mods/mod_remerge.py";
        public static string Audio { get; } = "https://github.com/ArchLeaders/BotwInstaller/raw/master/BotwInstaller.Wizard/ViewResources/Sound/audio.mp3";
        public static string LibUsbK { get; } = "https://github.com/ArchLeaders/BotwInstaller/raw/master/BotwInstaller.Lib/Configurations/Homebrew/libusbK.exe";
        public static string TegraRCMSmash { get; } = "https://files.sshnuke.net/TegraRcmSmash1213.zip";
    }
}
