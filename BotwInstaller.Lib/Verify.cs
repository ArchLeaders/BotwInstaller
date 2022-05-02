#pragma warning disable CS0252
#pragma warning disable CS8600
#pragma warning disable CS8604

using BotwScripts.Lib.CheckSum;
using BotwScripts.Lib.Common;
using BotwScripts.Lib.Common.IO.FileSystems;
using System.Xml.Linq;

namespace BotwInstaller.Lib
{
    /// <summary>
    /// Static class used for verifying game files and software data
    /// </summary>
    public static class Verify
    {
        /// <summary>
        /// Set of BotW Check files
        /// </summary>
        private static Dictionary<string, string> Checks { get; } = new()
        {
            { "Game", "\\content\\Movie\\Demo101_0.mp4" },
            { "Update", "\\content\\Actor\\Pack\\TwnObj_TempleOfTime_A_01.sbactorpack" },
            { "DLC", "\\content\\0010\\UI\\StaffRollDLC\\RollpictDLC001.sbstftex" }
        };

        /// <summary>
        /// Set of BotW IDs
        /// </summary>
        private static Dictionary<string, string> IDs { get; } = new()
        {
            { "Game", "00050000" },
            { "Update", "0005000E" },
            { "DLC", "0005000C" }
        };

        /// <summary>
        /// Set of BotW Title IDs
        /// </summary>
        private static Dictionary<string, string> TitleIds { get; } = new()
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
                    paths["mlc"] = mlc;
                }
            }

            foreach (var titleId in TitleIds.Values)
            {
                foreach (var set in Checks)
                {
                    var dir = $"{mlc}\\usr\\title\\{IDs[set.Key]}\\{titleId}";

                    if (!(bool)paths[$"{set.Key}_IsInstalled"])
                    {
                        if (File.Exists($"{dir}{set.Value}"))
                        {
                            if (BotwContents(set.Key, dir, print, func))
                            {
                                paths[$"{set.Key}_IsInstalled"] = true;
                                paths[set.Key] = dir;
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
        public static bool RollPictDLC(Interface.Notify print, string rollpict, ref Dictionary<string, object> paths, string func = "[VERIFY.ROLLPICTDLC]", string? nx = null)
        {
            int index = 5;

            if (nx != null)
                index = 2;

            if (BotwContents("DLC", rollpict.EditPath(index), print, func))
            {
                print($"{func} DLC found in '{rollpict.EditPath(nx == null ? index : index+1)}'");
                paths["DLC"] = rollpict.EditPath(index);
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Searches for the botw game paths relative to the passed RollPictDLC.sbstftex file.
        /// <para>Returns <c>true</c> when the Game and Update paths have been located.</para>
        /// </summary>
        /// <param name="uking"></param>
        /// <param name="paths"></param>
        /// <returns>Boolean</returns>
        public static bool AglResource(Interface.Notify print, string aglResource, ref Dictionary<string, object> paths, string func = "[VERIFY.AGLRESOURCE]")
        {
            if (BotwContents("GameNX", aglResource.EditPath(3), print, func, "01007EF00011E000"))
            {
                print($"{func} Game found in '{aglResource.EditPath(4)}'");
                paths["Game"] = aglResource.EditPath(4);
            }

            if (Directory.Exists($"{aglResource.EditPath(5)}\\01007EF00011F001\\romfs"))
            {
                if (BotwContents("DlcNX", $"{aglResource.EditPath(5)}\\01007EF00011F001\\romfs", print, func, "01007EF00011F001"))
                {
                    print($"{func} DLC found in '{aglResource.EditPath(5)}\\01007EF00011F001'");
                    paths["DLC"] = $"{aglResource.EditPath(5)}\\01007EF00011F001";
                }
            }

            if (paths["Game"] != "NOT FOUND")
                return true;

            return false;
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
            if (!Directory.Exists($"{code.EditPath()}\\content"))
            {
                print($"{func}[UKING] UKing '{new FileInfo(uking).Directory}\\U-King.rpx' was invalid.", ConsoleColor.DarkYellow);
                return false;
            }

            // Check for cemu structure
            var id = code.EditPath().GetTitleID(ITitleIDFormat.HexEnd);

            foreach (var set in Checks)
            {
                if (File.Exists($"{code.EditPath(3)}\\{IDs[set.Key]}\\{id}{set.Value}") && paths[set.Key] == "NOT FOUND")
                {
                    if (BotwContents(set.Key, $"{code.EditPath(3)}\\{IDs[set.Key]}\\{id}", print, func))
                    {
                        paths[set.Key] = $"{code.EditPath(3)}\\{IDs[set.Key]}\\{id}";
                        print($"{func}[UKING][CEMU] {set.Key} found in '{code.EditPath(3)}\\{IDs[set.Key]}\\{id}'");
                    }
                    else return false;
                }
            }

            // If the Game and Update have been found, return
            if (paths["Game"] != "NOT FOUND" && paths["Update"] != "NOT FOUND")
                return true;

            // Loop supposed game directory
            foreach (var folder in Directory.GetDirectories(code.EditPath(2)))
            {
                foreach (var set in Checks)
                {
                    if (File.Exists(folder + set.Value) && paths[set.Key] == "NOT FOUND")
                    {
                        if (BotwContents(set.Key, folder, print, func))
                        {
                            paths[set.Key] = folder;
                            print($"{func} {set.Key} found in '{folder}'");
                        }
                        else return false;

                        if (paths["Game"] != "NOT FOUND" && paths["Update"] != "NOT FOUND")
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
        public static bool BotwContents(this string key, string folder, Interface.Notify print, string func = "[VERIFY.BOTW]", string? nx = null)
        {
            var checkSum = folder.GetCheckSum(print, func, nx);
            var diff = checkSum.Except(Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories));

            if (diff.Any())
            {
                foreach (var item in diff)
                    Interface.Log($"[{key.ToUpper()} MISSING] '{item}'", ".\\Missing Files.txt");

                return false;
            }
            else return true;
        }

        /// <summary>
        /// Gets a BotW checksum based on the games region.
        /// </summary>
        /// <param name="gameFiles"></param>
        /// <returns></returns>
        public static List<string> GetCheckSum(this string gameFiles, Interface.Notify print, string func = "[VERIFY.CHECKSUM]", string? nx = null)
        {
            gameFiles = gameFiles.EndsWith("\\") ? gameFiles : $"{gameFiles}\\";

            if (nx != null)
            {
                print($"{func} Returning NX data check as '{nx}'");

                return nx switch
                {
                    "01007EF00011E000" => BaseNX.Set(gameFiles),
                    "01007EF00011F001" => DlcNX.Set(gameFiles),
                    _ => new()
                };
            }

            string ID = GameInfo.GetTitleID(gameFiles);
            string region = GameInfo.GetTitleID(gameFiles, ITitleIDFormat.Region);

            print($"{func} Returning {region} data check as '{ID}'");

            return ID switch
            {
                "00050000101C9500" => BaseEU.Set(gameFiles),
                "0005000E101C9500" => UpdateEU.Set(gameFiles),
                "0005000C101C9500" => DlcEU.Set(gameFiles),

                "00050000101C9400" => BaseUS.Set(gameFiles),
                "0005000E101C9400" => UpdateUS.Set(gameFiles),
                "0005000C101C9400" => DlcUS.Set(gameFiles),

                "00050000101C9300" => BaseJP.Set(gameFiles),
                "0005000E101C9300" => UpdateJP.Set(gameFiles),
                "0005000C101C9300" => DlcJP.Set(gameFiles),
                _ => new()
            };
        }
    }
}
