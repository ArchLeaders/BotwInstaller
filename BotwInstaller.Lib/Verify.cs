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
        public static void CheckMlc(this string cemu, ref Dictionary<string, object> paths, Interface.Notify print, string func = "[VERIFY.MLC]")
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

                    print($"{func} Mlc found in '{mlc}'");
                }
            }

            foreach (var titleId in TitleIds.Values)
            {
                foreach (var set in Checks)
                {
                    var dir = $"{mlc}\\usr\\title\\{IDs[set.Key]}\\{titleId}";

                    if (!paths.ContainsKey($"{set.Key}_IsInstalled"))
                    {
                        paths.Add($"{set.Key}_IsInstalled", false);

                        if (File.Exists($"{dir}{set.Value}"))
                        {
                            if (BotwContents(set.Key, dir, print, func))
                            {
                                paths[$"{set.Key}_IsInstalled"] = true;
                            }
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
        public static bool RollPictDLC(Interface.Notify print, string rollpict, ref Dictionary<string, object> paths, string func = "[VERIFY.ROLLPICTDLC]")
        {
            if (BotwContents("DLC", rollpict.EditPath(5), print, func))
            {
                print($"{func} DLC found in '{rollpict.EditPath(5)}'");
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
        public static bool UKing(Interface.Notify print, string uking, ref Dictionary<string, object> paths, string func = "[VERIFY.UKING]")
        {
            // Get DirectoryName
            string code = new FileInfo(uking).DirectoryName;

            // Check for content folder
            if (!Directory.Exists($"{code.EditPath()}content"))
            {
                print($"{func}[UKING] UKing '{new FileInfo(uking).Directory}\\U-King.rpx' was invalid.", ConsoleColor.DarkYellow);
                return false;
            }

            // Check for cemu structure
            var id = code.EditPath().GetTitleID(TitleIDFormat.HexEnd);

            foreach (var set in Checks)
            {
                if (File.Exists($"{code.EditPath(3)}{IDs[set.Key]}\\{id}{set.Value}") && !paths.ContainsKey(set.Key))
                {
                    if (BotwContents(set.Key, $"{code.EditPath(3)}{IDs[set.Key]}\\{id}", print, func))
                    {
                        paths.Add(set.Key, $"{code.EditPath(3)}{IDs[set.Key]}\\{id}");
                        print($"{func}[UKING][CEMU] {set.Key} found in '{code.EditPath(3)}{IDs[set.Key]}\\{id}'");
                    }
                    else return false;
                }
            }

            // If the Game and Update have been found, return
            if (paths.ContainsKey("Game") && paths.ContainsKey("Update"))
                return true;

            // Loop supposed game directory
            foreach (var folder in Directory.GetDirectories(code.EditPath(2)))
            {
                foreach (var set in Checks)
                {
                    if (File.Exists(folder + set.Value) && !paths.ContainsKey(set.Key))
                    {
                        if (BotwContents(set.Key, folder, print, func))
                        {
                            paths.Add(set.Key, folder);
                            print($"{func} {set.Key} found in '{folder}'");
                        }
                        else return false;

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
        public static bool BotwContents(this string key, string folder, Interface.Notify print, string func = "[VERIFY.BOTW]")
        {
            var checkSum = folder.GetCheckSum(print, func);
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
        public static Dictionary<string, List<string>> GetCheckSum(this string gameFiles, Interface.Notify print, string func = "[VERIFY.CHECKSUM]")
        {
            var ID = GameInfo.GetTitleID(gameFiles);

            gameFiles = gameFiles.EndsWith("\\") ? gameFiles : $"{gameFiles}\\";

            if (ID == "00050000101C9500" || ID == "0005000C101C9500" || ID == "0005000E101C9500")
            {
                print($"{func} Returning EU data check as '{ID}'");
                return new()
                {
                    { "Game", BaseEU.Set(gameFiles) },
                    { "Update", UpdateEU.Set(gameFiles) },
                    { "DLC", DlcEU.Set(gameFiles) }
                };
            }
            else if (ID == "00050000101C9400" || ID == "0005000C101C9400" || ID == "0005000E101C9400")
            {
                print($"{func} Returning US data check as '{ID}'");
                return new()
                {
                    { "Game", BaseUS.Set(gameFiles) },
                    { "Update", UpdateUS.Set(gameFiles) },
                    { "DLC", DlcUS.Set(gameFiles) }
                };
            }
            else if (ID == "00050000101C9300" || ID == "0005000C101C9300" || ID == "0005000E101C9300")
            {
                print($"{func} Returning JP data check as '{ID}'");
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
