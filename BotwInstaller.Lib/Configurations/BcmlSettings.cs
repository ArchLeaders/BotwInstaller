using BotwScripts.Lib.Common.IO.FileSystems;
using System.Text.Json;

namespace BotwInstaller.Lib.Configurations
{
    /// <summary>
    /// BCML Settings class
    /// </summary>
    public class BcmlSettings
    {
        /// <summary>
        /// Write a BCML settings file to "%localappdata%\bcml\settings.json" overwriting any file with the same name.
        /// </summary>
        /// <param name="conf">BotwInstaller Config class</param>
        /// <returns></returns>
        public static async Task Write(Config conf)
        {
            Dictionary<string, object> jsonObject = new()
            {
                { "cemu_dir", conf.UseCemu ? conf.Dirs.Dynamic : "" },
                { "game_dir", conf.IsNX ? "" : $"{conf.Dirs.Base}\\content" },
                { "game_dir_nx", conf.IsNX ? $"{conf.Dirs.Base}\\romfs" : "" },
                { "update_dir", conf.IsNX ? "" : $"{conf.Dirs.Update}\\content" },
                { "dlc_dir", conf.IsNX ? "" : $"{conf.Dirs.DLC}\\content\\0010" },
                { "dlc_dir_nx", conf.IsNX ? $"{conf.Dirs.DLC}\\romfs" : "" },
                { "store_dir", conf.Dirs.BCML },
                { "export_dir", conf.UseCemu ? $"{conf.Dirs.Dynamic}\\graphicPacks\\BreathOfTheWild_BCML" : conf.Dirs.Dynamic },
                { "export_dir_nx", conf.Dirs.Dynamic == "" ? $"{Config.AppData.EditPath()}\\Roaming\\yuzu\\load" : conf.Dirs.Dynamic },
                { "load_reverse", false },
                { "site_meta", "" },
                { "no_guess", false },
                { "lang", conf.IsNX ? "USen" : $"{GameInfo.GetTitleID(conf.Dirs.Base, TitleIDFormat.Region)}en" },
                { "no_cemu", !conf.UseCemu },
                { "wiiu", !conf.IsNX },
                { "no_hardlinks", false },
                { "force_7z", false },
                { "suppress_update", false },
                { "loaded", true },
                { "nsfw", false },
                { "changelog", true },
                { "strip_gfx", false },
                { "auto_gb", true },
                { "show_gb", false },
                { "dark_theme", false },
                { "last_version", "3.8.0" }
            };

            Directory.CreateDirectory($"{Config.AppData}\\bcml");
            await File.WriteAllTextAsync($"{Config.AppData}\\bcml\\settings.json", JsonSerializer.Serialize(jsonObject, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
