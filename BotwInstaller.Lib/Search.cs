#pragma warning disable CS0252
#pragma warning disable CS8600
#pragma warning disable CS8601
#pragma warning disable CS8602
#pragma warning disable CS8603
#pragma warning disable CS8604

using BotwScripts.Lib.Common;
using BotwScripts.Lib.Common.IO;
using BotwScripts.Lib.Common.IO.FileSystems;

namespace BotwInstaller.Lib
{
    /// <summary>
    /// Class used for search for game files and related applications
    /// </summary>
    public class Search
    {
        /// <summary>
        /// Search for Cemu and returns a Dictionary with the path and install details.
        /// </summary>
        /// <param name="path">Supposed path to Cemu</param>
        /// <returns></returns>
        public static void Cemu(ref Dictionary<string, object> paths, Interface.Notify print, string path = "::")
        {
            var func = "[SEARCH.CEMU]";

            // Check given path
            if (File.Exists($"{path}\\Cemu.exe"))
            {
                // Add cemu path
                paths["Cemu"] = path;

                Verify.CheckMlc(path, ref paths, print, $"{func}[CHECKMLC]");

                // Return dict
                return;
            }

            // Check root folder
            if (File.Exists($"{Directory.GetCurrentDirectory()}\\Cemu.exe"))
            {
                paths["Cemu"] = Directory.GetCurrentDirectory();

                Verify.CheckMlc(Directory.GetCurrentDirectory(), ref paths, print, $"{func}[CHECKMLC]");

                return;
            }

            // Check PC
            try
            {
                print($"{func} Start Unsafe");

                foreach (var dv in DriveInfo.GetDrives().Reverse())
                {
                    print($"{func}[UNSAFE] Searching '{dv.Name}'");

                    foreach (var file in Files.GetUnsafe(dv.Name, "Cemu.exe"))
                    {
                        var dir = new FileInfo(file).DirectoryName;
                        print($"{func}[UNSAFE] Cemu found in '{dir}'");
                        paths["Cemu"] = dir;

                        Verify.CheckMlc(dir, ref paths, print, $"{func}[UNSAFE][CHECKMLC]");

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                print($"{func} Warning - {ex.Message}", ConsoleColor.DarkYellow);
                print($"{func} Start Safe");

                if (paths["Cemu"] != "NOT FOUND") return;

                foreach (var dv in DriveInfo.GetDrives().Reverse())
                {
                    print($"{func}[SAFE] Searching '{dv.Name}'");

                    foreach (var file in Files.GetSafe(dv.Name, "Cemu.exe"))
                    {
                        var dir = new FileInfo(file).DirectoryName;
                        print($"{func}[SAFE] Cemu found in '{dir}'");
                        paths["Cemu"] = dir;

                        Verify.CheckMlc(dir, ref paths, print, $"{func}[SAFE][CHECKMLC]");

                        return;
                    }
                }
            }

            return;
        }

        /// <summary>
        /// Searches for BotW and returns a Dictionary with the paths.
        /// </summary>
        /// <returns></returns>
        public static void Botw(ref Dictionary<string, object> paths, Interface.Notify print)
        {
            var func = "[SEARCH.BOTW]";

            try
            {
                // Get SafeFiles [UNSTABLE]
                print($"{func} Start Unsafe");

                foreach (var dv in DriveInfo.GetDrives().Reverse())
                {
                    print($"{func}[UNSAFE] Searching '{dv.Name}'");

                    bool _break = false;

                    foreach (var file in Files.GetUnsafe(dv.Name, "U-King.rpx"))
                    {
                        while (!Verify.UKing(print, file, ref paths))
                            break;

                        if (paths["Game"] != "NOT FOUND" && paths["Update"] != "NOT FOUND")
                        {
                            _break = true;
                            break;
                        }
                    }

                    if (_break) break;
                }
            }
            catch (Exception ex)
            {
                // Get SafeFiles [STABLE]
                print($"{func} Warning - {ex.Message}", ConsoleColor.DarkYellow);
                print($"{func} Start Safe");

                foreach (var dv in DriveInfo.GetDrives().Reverse())
                {
                    print($"{func}[SAFE] Searching '{dv.Name}'");

                    bool _break = false;

                    foreach (var file in Files.GetSafe(dv.Name, "U-King.rpx"))
                    {
                        while (!Verify.UKing(print, file, ref paths))
                            break;

                        if (paths["Game"] != "NOT FOUND" && paths["Update"] != "NOT FOUND")
                        {
                            _break = true;
                            break;
                        }
                    }

                    if (_break) break;
                }
            }

            func = $"{func}[DLC]";

            if (paths["DLC"] == "NOT FOUND")
            {
                try
                {
                    // Get SafeFiles [UNSAFE]
                    print($"{func} Start Unsafe");

                    foreach (var dv in DriveInfo.GetDrives().Reverse())
                    {
                        print($"{func}[UNSAFE] Searching '{dv.Name}'");

                        foreach (var file in Files.GetUnsafe(dv.Name, "RollpictDLC001.sbstftex"))
                        {
                            while (!Verify.RollPictDLC(print, file, ref paths, "[SEARCH.BOTW][DLC][VERIFY]"))
                                break;

                            if (paths["DLC"] != "NOT FOUND")
                                return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Get SafeFiles [SAFE]
                    print($"{func} Warning - {ex.Message}", ConsoleColor.DarkYellow);
                    print($"{func} Start Safe");

                    foreach (var dv in DriveInfo.GetDrives().Reverse())
                    {
                        print($"{func}[SAFE] Searching '{dv.Name}'");

                        foreach (var file in Files.GetSafe(dv.Name, "RollpictDLC001.sbstftex"))
                        {
                            while (!Verify.RollPictDLC(print, file, ref paths, "[SEARCH.BOTW][DLC]"))
                                break;

                            if (paths["DLC"] != "NOT FOUND")
                                return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Searches for Python and returns a string dictating the path.
        /// </summary>
        /// <param name="path">Supposed path to python</param>
        /// <returns></returns>
        public static string Python(Interface.Notify print, string path = "::")
        {
            var func = "[SEARCH.PYTHON]";

            // Check given path
            if (File.Exists($"{path}\\python38.dll") || File.Exists($"{path}\\python37.dll"))
            {
                // Add path to PATH
                PATH.AddEntry(path);
                PATH.AddEntry($"{path}\\Scripts");

                print($"{func} Python found in '{path}'");

                return path;
            }

            // Check environment variables (PATH)
            string[] env = Environment.GetEnvironmentVariable("PATH").Split(';');
            foreach (string env_path in env)
            {
                if (File.Exists($"{env_path}\\python38.dll") || File.Exists($"{env_path}\\python37.dll"))
                {
                    print($"{func} Python found on PATH in '{env_path}'");

                    return env_path;
                }
            }

            // Check PC
            try
            {
                print($"{func} Start Unsafe");

                foreach (var dv in DriveInfo.GetDrives())
                {
                    print($"{func}[UNSAFE] Searching {dv.Name}");

                    foreach (var file in Files.GetUnsafe(dv.Name, "python.exe"))
                    {
                        string dir = new FileInfo(file).DirectoryName;

                        if (File.Exists($"{dir}\\python38.dll") || File.Exists($"{dir}\\python37.dll"))
                        {
                            print($"{func}[UNSAFE] Python found in '{dir}'");

                            // Add Python to PATH
                            print($"{func}[UNSAFE] Adding '{dir};{dir}\\Scripts' to the Environment Variables");
                            PATH.AddEntry($"{dir};{dir}\\Scripts");

                            return dir;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                print($"{func} Warning - {ex.Message}", ConsoleColor.DarkYellow);
                print($"{func} Start Safe");

                foreach (var dv in DriveInfo.GetDrives())
                {
                    print($"{func}[SAFE] Searching {dv.Name}");

                    foreach (var file in Files.GetSafe(dv.Name, "python.exe"))
                    {
                        string dir = new FileInfo(file).DirectoryName;

                        if (File.Exists($"{dir}\\python38.dll") || File.Exists($"{dir}\\python37.dll"))
                        {
                            print($"{func}[SAFE] Python found in '{dir}'");

                            // Add Python to PATH
                            print($"{func}[SAFE] Adding '{dir};{dir}\\Scripts' to the Environment Variables");
                            PATH.AddEntry($"{dir};{dir}\\Scripts");

                            return dir;
                        }
                    }
                }
            }

            return "NOT FOUND";
        }
    }
}
