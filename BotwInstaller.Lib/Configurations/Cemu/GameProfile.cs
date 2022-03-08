using BotwScripts.Lib.Common.ClassObjects.Ini;

namespace BotwInstaller.Lib.Configurations.Cemu
{
    /// <summary>
    /// Cemu Game Profile class
    /// </summary>
    public class GameProfile
    {
        /// <summary>
        /// Writes a Cemu Game profile to "<paramref name="conf"/>.Dirs.Dynamic\gameProfiles\TITLE_ID.ini" overwiting any existing file with the same name.
        /// </summary>
        /// <param name="conf">BotwINstaller Config class</param>
        public static void Write(Config conf)
        {
            Directory.CreateDirectory($"{conf.Dirs.Dynamic}\\gameProfiles");
            IniFile ini = new($"{conf.Dirs.Dynamic}\\gameProfiles\\{GameInfo.GetTitleID(conf.Dirs.Base)}.ini");

            Api = conf.ControllerApi;
            GameProfile gameProfile = new();

            foreach (var category in gameProfile.Profile)
                foreach (var entry in category.Value)
                    ini.Write(entry.Key, entry.Value, category.Key);
        }

        private static string Api { get; set; } = "XInput";

        public Dictionary<string, Dictionary<string, string>> Profile = new()
        {
            {
                "General", new() {
                    { "loadSharedLibraries", "true" },
                    { "startWithPadView", "false" }
                }
            },
            {
                "CPU", new() {
                    { "cpuMode", "Multi-core recompiler" },
                    { "threadQuantum", "45000" }
                }
            },
            {
                "Graphics", new() {
                    { "accurateShaderMul", "true" },
                    { "precompileShaders", "auto" },
                    { "graphics_api", "1" }
                }
            },
            {
                "Controller", new() {
                    { "controller1", $"{Api}.xml" }
                }
            },
        };
    }
}
