using BotwScripts.Lib.Common;

namespace BotwInstaller.Lib
{
    public interface TitleIDFormat
    {
        public static string CemuFolder { get; } = "CF";
        public static string Region { get; } = "RE";
        public static string DecimalFull { get; } = "DF";
        public static string HexFull { get; } = "HF";
        public static string DecimalStart { get; } = "DS";
        public static string HexStart { get; } = "HS";
        public static string DecimalEnd { get; } = "DE";
        public static string HexEnd { get; } = "HE";
    }

    public static class GameInfo
    {
        /// <summary>
        /// Returns the title ID of BotW in the defined format.
        /// </summary>
        /// <param name="gameFiles"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string GetTitleID(this string gameFiles, string format = "HF")
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
                "CF" => $"{start}\\{end}",
                "RE" => region,
                "DF" => Convert.ToInt64(results, 16).ToString(),
                "HF" => results,
                "DS" => Convert.ToInt64(start, 16).ToString(),
                "HS" => start,
                "DE" => Convert.ToInt64(end, 16).ToString(),
                "HE" => end,
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
            Dictionary<string, object> paths = new();

            List<Task> tasks = new();

            if (!nx)
            {
                tasks.Add(Task.Run(() =>
                {
                    var botw = Search.Botw(print);

                    foreach (var item in botw)
                        paths.Add(item.Key, item.Value);
                }));

                if (cemu != "!ignore")
                {
                    tasks.Add(Task.Run(() =>
                    {
                        var cemuRt = Search.Cemu(print, cemu);

                        foreach (var item in cemuRt)
                            paths.Add(item.Key, item.Value);
                    }));
                }
            }

            tasks.Add(Task.Run(() =>
            {
                paths.Add("Python", Search.Python(print, python));
            }));

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
                "0005000c101C9500" => 22690,
                "0005000e101C9500" => 15927,
                // US
                "00050000101C9400" => 15996,
                "0005000e101C9400" => 22647,
                "0005000c101C9400" => 15219,
                // JP
                "00050000101C9300" => 14191,
                "0005000c101C9300" => 22617,
                "0005000e101C9300" => 14747,
                _ => 0,
            };
        }
    }
}
