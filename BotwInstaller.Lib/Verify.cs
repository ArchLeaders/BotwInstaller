#pragma warning disable CS8600
#pragma warning disable CS8604

using BotwScripts.Lib.CheckSum;
using BotwScripts.Lib.Common;
using BotwScripts.Lib.Common.IO.FileSystems;
using System.Xml.Linq;

namespace BotwInstaller.Lib
{
    public class XmlContent
    {
        public Dictionary<dynamic, dynamic> content = new();
    }
    public static class Verify
    {
        /// <summary>
        /// Set of BotW Check files
        /// </summary>
        public static Dictionary<string, string> Checks = new()
        {
            { "Game", "\\content\\Movie\\Demo101_0.mp4" },
            { "Update", "\\content\\Actor\\Pack\\TwnObj_TempleOfTime_A_01.sbactorpack" },
            { "DLC", "\\content\\0010\\UI\\StaffRollDLC\\RollpictDLC001.sbstftex" }
        };

        /// <summary>
        /// Set of BotW IDs
        /// </summary>
        public static Dictionary<string, string> IDs = new()
        {
            { "Game", "00050000" },
            { "Update", "0005000E" },
            { "DLC", "0005000C" }
        };

        /// <summary>
        /// Set of BotW Title IDs
        /// </summary>
        public static Dictionary<string, string> TitleIds = new()
        {
            { "EU", "101C9500" },
            { "US", "101C9400" },
            { "JP", "101C9300" }
        };

        /// <summary>
        /// Looks for the Game, Update and DLC in the passed Cemu directory.
        /// </summary>
        /// <param name="cemu"></param>
        /// <returns></returns>
        public static void CheckMlc(this string cemu, ref Dictionary<string, string> paths)
        {
            var mlc = "";

            if (!File.Exists($"{cemu}\\settings.xml"))
                return;

            var xml = XDocument.Load(File.OpenRead($"{cemu}\\settings.xml")).Descendants("content").Descendants();

            foreach (var item in xml)
            {
                if (item.Name == "mlc_path")
                {
                    mlc = mlc == "" ? "mlc01" : item.Value;

                    if (!mlc.Contains(':'))
                        mlc = $"{cemu}\\{mlc}";
                }
            }

            foreach (var titleId in TitleIds.Values)
            {
                Console.WriteLine($"[{titleId}]");

                foreach (var set in Checks)
                {
                    var dir = $"{mlc}\\usr\\title\\{IDs[set.Key]}\\{titleId}";

                    if (File.Exists($"{dir}{set.Value}"))
                    {
                        Console.WriteLine($"[{titleId}:{set.Key}]");

                        if (BotwContents(set.Key, dir))
                        {
                            Interface.WriteLine($"[CEMU.ABSOLUTE] {set.Key} found in '{dir}'", ConsoleColor.DarkGray);
                            paths.Add($"{set.Key}_IsInstalled", "TRUE");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Searches for the botw game paths relative to the passed RollPictDLC.sbstftex file.
        /// <para>Returns <c>true</c> when the Game and Update paths have been located.</para>
        /// </summary>
        /// <param name="uking"></param>
        /// <param name="paths"></param>
        /// <returns>Boolean</returns>
        public static bool RollPictDLC(string rollpict, ref Dictionary<string, string> paths)
        {
            if (BotwContents("DLC", rollpict.EditPath(5)))
            {
                Interface.WriteLine($"[ROLLPICTDLC.RELATIVE] DLC found in '{rollpict.EditPath(5)}'", ConsoleColor.DarkGray);
                paths.Add("DLC", rollpict.EditPath(5));
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Searches for the botw game paths relative to the passed U-King.rpx file.
        /// <para>Returns <c>true</c> when the Game and Update paths have been located.</para>
        /// </summary>
        /// <param name="uking"></param>
        /// <param name="paths"></param>
        /// <returns>Boolean</returns>
        public static bool UKing(string uking, ref Dictionary<string, string> paths)
        {
            // Get DirectoryName
            string code = new FileInfo(uking).DirectoryName;

            // Check for content folder
            if (!Directory.Exists($"{code.EditPath()}content"))
                return false;

            // Check for cemu structure
            var id = $"{code.EditPath()}content".GetTitleID(TitleIDFormat.HexEnd);

            foreach (var set in Checks)
            {
                if (File.Exists($"{code.EditPath(3)}{IDs[set.Key]}\\{id}{set.Value}") && !paths.ContainsKey(set.Key))
                {
                    if (File.Exists($"{code.EditPath(6)}\\Cemu.exe") && !paths.ContainsKey("Cemu"))
                    {
                        Interface.WriteLine($"[UKING.CEMU.RELATIVE] Cemu found in '{code.EditPath(6)}'\n", ConsoleColor.DarkGray);
                        if (!paths.ContainsKey("Cemu_FromBotw")) paths.Add("Cemu_FromBotw", code.EditPath(6));
                    }

                    if (BotwContents(set.Key, $"{code.EditPath(3)}{IDs[set.Key]}\\{id}"))
                    {
                        Interface.WriteLine($"[UKING.CEMU.RELATIVE] {set.Key} found in '{code.EditPath(3)}{IDs[set.Key]}\\{id}'", ConsoleColor.DarkGray);
                        paths.Add(set.Key, $"{code.EditPath(3)}{IDs[set.Key]}\\{id}");
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            // If the Game and Update have been found, return
            if (paths.ContainsKey("Game") && paths.ContainsKey("Update"))
                return true;

            // Loop supposed game directory
            foreach (var folder in Directory.GetDirectories(code.EditPath(2)))
            {
                foreach (var set in Verify.Checks)
                {
                    if (File.Exists(folder + set.Value) && !paths.ContainsKey(set.Key))
                    {
                        if (BotwContents(set.Key, folder))
                        {
                            Interface.WriteLine($"[UKING.RELATIVE] {set.Key} found in '{folder}'", ConsoleColor.DarkGray);
                            paths.Add(set.Key, folder);
                        }
                        else
                        {
                            return false;
                        }

                        if (paths.ContainsKey("Game") && paths.ContainsKey("Update"))
                            return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Verifies the passed Game/Update/DLC folder.
        /// <para>Returns <c>false</c> if the Game/Update/DLC is missing file(s).</para>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="folder"></param>
        /// <returns>Boolean</returns>
        public static bool BotwContents(this string key, string folder)
        {
            var checkSum = folder.GetCheckSum();
            var diff = checkSum[key].Except(Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories));

            if (diff.Any())
            {
                foreach (var item in diff)
                {
                    Interface.WriteLine($"[{key.ToUpper()} MISSING] '{item}'", ConsoleColor.Red);
                }

                return false;
            }
            else return true;
        }

        /// <summary>
        /// Gets a BotW checksum based on the games region.
        /// </summary>
        /// <param name="gameFiles"></param>
        /// <returns></returns>
        public static Dictionary<string, List<string>> GetCheckSum(this string gameFiles)
        {
            var ID = GameInfo.GetTitleID($"{gameFiles}\\content");

            gameFiles = gameFiles.EndsWith("\\") ? gameFiles : $"{gameFiles}\\";

            if (ID == "00050000101C9500" || ID == "0005000C101C9500" || ID == "0005000E101C9500")
            {
                return new()
                {
                    { "Game", BaseEU.Set(gameFiles) },
                    { "Update", UpdateEU.Set(gameFiles) },
                    { "DLC", DlcEU.Set(gameFiles) }
                };
            }
            else if (ID == "00050000101C9400" || ID == "0005000C101C9400" || ID == "0005000E101C9400")
            {
                return new()
                {
                    { "Game", BaseUS.Set(gameFiles) },
                    { "Update", UpdateUS.Set(gameFiles) },
                    { "DLC", DlcUS.Set(gameFiles) }
                };
            }
            else if (ID == "00050000101C9300" || ID == "0005000C101C9300" || ID == "0005000E101C9300")
            {
                return new()
                {
                    { "Game", BaseJP.Set(gameFiles) },
                    { "Update", UpdateJP.Set(gameFiles) },
                    { "DLC", DlcJP.Set(gameFiles) }
                };
            }
            else
            {
                return new();
            }
        }
    }
}
