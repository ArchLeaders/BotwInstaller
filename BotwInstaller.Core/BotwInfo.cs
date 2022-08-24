using BotwInstaller.Core.Cryptohraphy;
using BotwInstaller.Core.Extensions;
using BotwInstaller.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BotwInstaller.Core
{
    public enum BotwFolderType
    {
        BaseGame,
        Update,
        Dlc,
        BaseGameNx,
        DlcNx,
    }
    public enum TitleIdType
    {
        CommonName,
        DecimalFull,
        DecimalStart,
        DecimalEnd,
        HexFull,
        HexStart,
        HexEnd,
        MlcFolder,
        Region,
    }

    public class BotwInfo
    {
        /// <summary>
        /// Compares a checksum with the collected files in <paramref name="root"/>.
        /// </summary>
        /// <param name="root">Root game folder containing content/romfs</param>
        /// <returns>
        /// A KeyValuePair with true/false result and a list of missing files.
        /// </returns>
        public static async Task<KeyValuePair<bool, List<string>>> CheckFiles(string root)
        {
            List<Task> load = new();
            List<string>? expHashTable = null;
            List<string>? genHashTable = new();

            root = Path.GetFullPath(root);

            // Load the excepted checksum
            string titleid = GetTitleId(root, TitleIdType.HexFull);
            load.Add(Task.Run(() => {
                expHashTable = new Resource($"Core.Data.CheckSum.{titleid}", true).ToJson<List<string>>();
            }));

            // Load the new checksum
            load.Add(Task.Run(() => {
                foreach (var file in Directory.GetFiles(root, "*.*", SearchOption.AllDirectories)) {
                    var debug = file.Replace(root, "").ToSystemPath();
                    genHashTable.Add(CRC32.ComputeHash(file.Replace(root, "").ToSystemPath()));
                }
            }));

            await Task.WhenAll(load);

            if (expHashTable != null) {
                var diff = expHashTable.Except(genHashTable!);
                return new(!diff.Any(), await GetMissingFileTable(diff.ToList(), titleid));
            }

            throw new FileNotFoundException($"Could not find a checksum for the specified path '{root}'.");
        }

        public static string GetTitleId(string root, TitleIdType format = TitleIdType.HexEnd)
        {
            string results = "";
            string meta = $"{root}\\code\\app.xml";

            // Parse hexadecimal TitleId
            if (File.Exists(meta)) {
                foreach (string line in File.ReadAllLines(meta))
                    if (line.StartsWith("  <title_id type=\"hexBinary\" length=\"8\">"))
                        results = line.Split('>')[1].Replace("</title_id", "");
            }
            else if (File.Exists($"{root}\\romfs\\Actor\\Pack\\AirWall.sbactorpack")) {
                results = "01007EF00011E000";
            }
            else if (File.Exists($"{root}\\romfs\\UI\\StaffRollDLC\\RollpictDLC001.sbstftex")) {
                results = "01007EF00011F001";
            }
            else {
                throw new FileNotFoundException($"Could not find a file in '{root}' to verify a TitleID.");
            }

            Dictionary<string, string> regions = new() {
                { "101C9500", "EU" },
                { "101C9400", "US" },
                { "101C9300", "JP" },
                { "01007EF0", "US" },
            };

            Dictionary<string, string> names = new() {
                { "00050000", "BaseGame" },
                { "0005000E", "Update" },
                { "0005000C", "Dlc" },
                { "0011E000", "BaseGameNx" },
                { "0011F001", "DlcNx" },
            };

            string start = results[0..8];
            string end = results[8..16];

            return format switch {
                TitleIdType.CommonName => IsNX(results) ? names[end] : names[start],
                TitleIdType.DecimalFull => Convert.ToInt64(results, 16).ToString(),
                TitleIdType.DecimalStart => Convert.ToInt64(start, 16).ToString(),
                TitleIdType.DecimalEnd => Convert.ToInt64(end, 16).ToString(),
                TitleIdType.HexFull => results,
                TitleIdType.HexStart => start,
                TitleIdType.HexEnd => end,
                TitleIdType.MlcFolder => $"{start}\\{end}",
                TitleIdType.Region => IsNX(results) ? regions[start] : regions[end],
                _ => results,
            };
        }

        public static bool IsNX(string titleId)
            => new List<string>() { "01007EF00011E000", "01007EF00011F001" }.Contains(titleId);

        public static Task<List<string>> GetMissingFileTable(List<string> missingHashes, string titleid)
        {
            List<string> missing = new();
            Dictionary<string, string> table = new Resource($"BotwInstaller.Core/Data/Tables/{titleid}.sjson", true).ToJson<Dictionary<string, string>>() ?? new();

            foreach (var item in missingHashes) {
                missing.Add(table[item]);
            }

            return Task.FromResult(missing);
        }
    }
}
