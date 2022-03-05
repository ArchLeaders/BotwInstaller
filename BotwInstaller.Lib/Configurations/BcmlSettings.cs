using System.Text.Json;

namespace BotwInstaller.Lib.Configurations
{
    public class BcmlSettings
    {
        public static async Task Write(Config conf)
        {
            Dictionary<string, object> jsonObject = new()
            {
                { "cemu_dir", conf.Dirs.Dynamic },
                { "game_dir", conf.Dirs.Base },
                { "game_dir_nx", "" },
                { "update_dir", conf.Dirs.Update },
                { "dlc_dir", conf.Dirs.DLC },
                { "dlc_dir_nx", "" },
                { "store_dir", conf.Dirs.BCML },
                { "export_dir", $"{conf.Dirs.Dynamic}\\graphicPacks\\BreathOfTheWild_BCML" },
                { "export_dir_nx", "" },
                { "load_reverse", false },
                { "site_meta", "" },
                { "no_guess", false },
                { "lang", $"{GameInfo.GetTitleID(conf.Dirs.Base, TitleIDFormat.Region)}en" },
                { "no_cemu", false },
                { "wiiu", true },
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
