using BotwScripts.Lib.Common.IO.FileSystems;
using System.Text.Json;

namespace BotwInstaller.Lib.Configurations
{
    public class BcmlSettings
    {
        public static async Task Write(Config conf)
        {
            Dictionary<string, object> jsonObject = new()
            {
                { "cemu_dir", conf.Install.Cemu ? conf.Dirs.Dynamic : "" },
                { "game_dir", conf.IsNX ? "" : $"{conf.Dirs.Base}\\content" },
                { "game_dir_nx", conf.IsNX ? $"{Config.AppData.EditPath()}\\Roaming\\yuzu\\dump\\01007EF00011E000\\romfs" : "" },
                { "update_dir", conf.IsNX ? "" : $"{conf.Dirs.Update}\\content" },
                { "dlc_dir", conf.IsNX ? "" : $"{conf.Dirs.DLC}\\content\\0010" },
                { "dlc_dir_nx", conf.IsNX ? $"{Config.AppData.EditPath()}\\Roaming\\yuzu\\dump\\01007EF00011F001\\romfs" : "" },
                { "store_dir", conf.Dirs.BCML },
                { "export_dir", conf.Install.Cemu ? $"{conf.Dirs.Dynamic}\\graphicPacks\\BreathOfTheWild_BCML" : conf.Dirs.Dynamic },
                { "export_dir_nx", conf.Dirs.Dynamic == "" ? $"{Config.AppData.EditPath()}\\Roaming\\yuzu\\load" : conf.Dirs.Dynamic },
                { "load_reverse", false },
                { "site_meta", "" },
                { "no_guess", false },
                { "lang", $"{GameInfo.GetTitleID(conf.Dirs.Base, TitleIDFormat.Region)}en" },
                { "no_cemu", !conf.Install.Cemu },
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
