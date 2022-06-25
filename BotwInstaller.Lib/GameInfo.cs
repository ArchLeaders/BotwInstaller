using BotwScripts.Lib.Common;

namespace BotwInstaller.Lib
{
    /// <summary>
    /// TitleID format enum
    /// </summary>
    public enum TitleIDFormat
    {
        CemuFolder = 0,
        Region = 1,
        DecimalFull = 2,
        HexFull = 3,
        DecimalStart = 4,
        HexStart = 5,
        DecimalEnd = 6,
        HexEnd = 7
    }

    /// <summary>
    /// Static class holding logic to get/set BOTW information
    /// </summary>
    public static class GameInfo
    {
        /// <summary>
        /// Returns the title ID of BotW in the defined format.
        /// </summary>
        /// <param name="gameFiles"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string GetTitleID(this string gameFiles, TitleIDFormat format = TitleIDFormat.HexEnd)
        {
            string results = "";

            foreach (string line in File.ReadAllLines($"{gameFiles}\\meta\\meta.xml"))
                if (line.StartsWith("  <title_id type=\"hexBinary\" length=\"8\">"))
                    results = line.Split('>')[1].Replace("</title_id", "");

            string[] starts = new[]
            {
                "00050000",
                "0005000E",
                "0005000C"
            };

            string[] ends = new[]
            {
                "101C9500",
                "101C9400",
                "101C9300"
            };

            Dictionary<string, string> regions = new()
            {
                { "101C9500", "EU" },
                { "101C9400", "US" },
                { "101C9300", "JP" }
            };

            var start = results;
            var end = results;

            foreach (var item in starts)
                end = end.Replace(item, "");
            foreach (var item in ends)
                start = start.Replace(item, "");

            var region = end;

            foreach (var item in regions)
                region = region.Replace(item.Key, item.Value);

            return format switch
            {
                TitleIDFormat.CemuFolder => $"{start}\\{end}",
                TitleIDFormat.Region => region,
                TitleIDFormat.DecimalFull => Convert.ToInt64(results, 16).ToString(),
                TitleIDFormat.HexFull => results,
                TitleIDFormat.DecimalStart => Convert.ToInt64(start, 16).ToString(),
                TitleIDFormat.HexStart => start,
                TitleIDFormat.DecimalEnd => Convert.ToInt64(end, 16).ToString(),
                TitleIDFormat.HexEnd => end,
                _ => results,
            };
        }

        /// <summary>
        /// Searches for the files and software to install BotW and returns a dictionary with the found values.
        /// <para> </para>
        /// <code><b>
        /// (string) Python
        /// (string) Cemu
        /// 
        /// (string) DLC
        /// (string) Game
        /// (string) Update
        /// 
        /// (bool) Game_IsInstalled
        /// (bool) Update_IsInstalled
        /// (bool) DLC_IsInstalled
        /// </b></code>
        /// </summary>
        /// <param name="cemu">Supposed path to Cemu</param>
        /// <param name="python">Supposed path to Python</param>
        /// <returns></returns>
        public static async Task<Dictionary<string, object>> GetFiles(Interface.Notify print, string cemu = "::", string python = "::", bool nx = false)
        {
            Dictionary<string, object> paths = new()
            {
                { "Cemu", "NOT FOUND" },
                { "mlc", "NOT FOUND" },
                { "Game_IsInstalled", false },
                { "Update_IsInstalled", false },
                { "DLC_IsInstalled", false },
                { "Game", "NOT FOUND" },
                { "Update", "NOT FOUND" },
                { "DLC", "NOT FOUND" },
                { "Python", "NOT FOUND" }
            };

            List<Task> tasks = new();

            if (!nx)
            {
                tasks.Add(Task.Run(() => Search.Botw(ref paths, print)));

                if (cemu != "!ignore")
                    tasks.Add(Task.Run(() => Search.Cemu(ref paths, print, cemu)));
            }
            else
            {
                tasks.Add(Task.Run(() => Search.BotwNX(ref paths, print)));
            }

            tasks.Add(Task.Run(() => paths["Python"] = Search.Python(print, python)));

            await Task.WhenAll(tasks);

            return paths;
        }

        /// <summary>
        /// Returns the game file count (Base, Update, DLC)
        /// </summary>
        /// <param name="gameFiles"></param>
        /// <returns></returns>
        public static int FileCount(this string gameFiles)
        {
            return GetTitleID(gameFiles) switch
            {
                // EU
                "00050000101C9500" => 18717,
                "0005000E101C9500" => 22690,
                "0005000C101C9500" => 15927,
                // US
                "00050000101C9400" => 15996,
                "0005000E101C9400" => 22647,
                "0005000C101C9400" => 15219,
                // JP
                "00050000101C9300" => 14191,
                "0005000E101C9300" => 22617,
                "0005000C101C9300" => 14747,
                _ => 0,
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static List<string> GetModPresets(string mode)
        {
            if (ModPresetData == null)
                 return new();

            return new(ModPresetData[mode.Replace("cemu", "wiiu")].Keys);
        }
        public static Dictionary<string, Dictionary<string, Dictionary<string, string?>[]>>? ModPresetData { get; set; }
    }
}
