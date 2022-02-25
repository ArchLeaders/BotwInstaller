using BotwScripts.Lib.Common;
using BotwScripts.Lib.Common.IO.FileSystems;

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
                { "101C9500", "EUen" },
                { "101C9400", "USen" },
                { "101C9300", "JPjp" }
            };

            var start = results;
            var end = results;
            var region = "";

            foreach (var item in starts)
                end = end.Replace(item, "");
            foreach (var item in ends)
                start = start.Replace(item, "");
            foreach (var item in regions)
                region = end.Replace(item.Key, item.Value);

            switch (format)
            {
                case "CF":
                    return $"{start}\\{end}";
                case "RE":
                    return region;
                case "DF":
                    return Convert.ToInt64(results, 16).ToString();
                case "HF":
                    return results;
                case "DS":
                    return Convert.ToInt64(start, 16).ToString();
                case "HS":
                    return start;
                case "DE":
                    return Convert.ToInt64(end, 16).ToString();
                case "HE":
                    return end;
            }

            return results;
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
        public static async Task<Dictionary<string, object>> GetFiles(string cemu = "::", string python = "::")
        {
            Dictionary<string, object> paths = new();

            List<Task> tasks = new();

            tasks.Add(Task.Run(() =>
            {
                var botw = Search.Botw(Interface.WriteLine);

                foreach (var item in botw)
                    paths.Add(item.Key, item.Value);
            }));

            tasks.Add(Task.Run(() =>
            {
                paths.Add("Python", Search.Python(Interface.WriteLine, python));
            }));

            tasks.Add(Task.Run(() =>
            {
                var cemuRt = Search.Cemu(Interface.WriteLine, cemu);

                foreach (var item in cemuRt)
                    paths.Add(item.Key, item.Value);
            }));

            await Task.WhenAll(tasks);

            return paths;
        }

        /// <summary>
        /// Returns the game file count (Base, Update, DLC)
        /// </summary>
        /// <param name="pathToBase"></param>
        /// <returns></returns>
        public static int[] FileCount(this string pathToBase)
        {
            if (GetTitleID(pathToBase) == "00050000101C9500")
                return new int[] { 18717, 22690, 15927 };
            else if (GetTitleID(pathToBase) == "00050000101C9400")
                return new int[] { 15996, 22647, 15219 };
            else if (GetTitleID(pathToBase) == "00050000101C9300")
                return new int[] { 14191, 22617, 14747 };
            else return new int[] { 0 };
        }
    }
}
