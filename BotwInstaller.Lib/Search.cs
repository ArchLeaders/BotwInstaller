#pragma warning disable CS8602
#pragma warning disable CS8604

using BotwScripts.Lib.Common;
using BotwScripts.Lib.Common.IO;
using BotwScripts.Lib.Common.IO.FileSystems;

namespace BotwInstaller.Lib
{
    public class Search
    {
        /// <summary>
        /// Search for Cemu and returns a Dictionary with the path and install details.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Dictionary<string, string> Cemu(string path = "A:")
        {
            Interface.WriteLine("Searching for Cemu.exe . . .");

            Dictionary<string, string> paths = new();

            // Check given path
            Interface.WriteLine($"[CEMU.SEARCH] Checking '{path}'");

            if (File.Exists($"{path}\\Cemu.exe"))
            {
                Interface.WriteLine($"[PARAM.CEMU.SEARCH] Cemu found in '{path}'", ConsoleColor.DarkGray);

                // Add cemu path
                paths.Add("Cemu", path);

                Verify.CheckMlc(path, ref paths);

                // Return dict
                return paths;
            }

            // Check root folder
            Interface.WriteLine($"[CEMU.SEARCH] Checking '{Directory.GetCurrentDirectory()}'");

            if (File.Exists($"{Directory.GetCurrentDirectory()}\\Cemu.exe"))
            {
                Interface.WriteLine($"[ROOT.CEMU.SEARCH] Cemu found in '{Directory.GetCurrentDirectory()}'", ConsoleColor.DarkGray);
                paths.Add("Cemu", Directory.GetCurrentDirectory());

                Verify.CheckMlc(Directory.GetCurrentDirectory(), ref paths);

                return paths;
            }

            // Check PC
            try
            {
                Interface.WriteLine("[CEMU.UNSTABLE]");

                foreach (var dv in DriveInfo.GetDrives().Reverse())
                {
                    Interface.WriteLine($"[CEMU.UNSTABLE] Searching {dv.Name}");

                    foreach (var file in Files.GetSafe(dv.Name, "Cemu.exe"))
                    {
                        Interface.WriteLine($"[CEMU.UNSTABLE] Found {file}");

                        var dir = new FileInfo(file).DirectoryName;
                        Interface.WriteLine($"[CEMU.UNSTABLE] Cemu found in '{dir}'", ConsoleColor.DarkGray);
                        paths.Add("Cemu", dir);

                        Verify.CheckMlc(dir, ref paths);

                        return paths;
                    }
                }
            }
            catch (Exception ex)
            {
                Interface.WriteLine($"[WARNING] {ex.Message}", ConsoleColor.DarkYellow);

                Interface.WriteLine("[CEMU.STABLE]");

                foreach (var dv in DriveInfo.GetDrives().Reverse())
                {
                    Interface.WriteLine($"[CEMU.STABLE] Searching {dv.Name}");

                    if (paths.ContainsKey("Cemu")) return paths;

                    foreach (var file in Files.GetSafeNoYield(dv.Name, "Cemu.exe"))
                    {
                        Interface.WriteLine($"[CEMU.STABLE] Found {file}");

                        var dir = new FileInfo(file).DirectoryName;
                        Interface.WriteLine($"[CEMU.STABLE] Cemu found in '{dir}'", ConsoleColor.DarkGray);
                        paths.Add("Cemu", dir);

                        Verify.CheckMlc(dir, ref paths);

                        return paths;
                    }
                }
            }

            return paths;
        }

        /// <summary>
        /// Searches for BotW and returns a Dictionary with the paths.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> Botw()
        {
            Interface.WriteLine("Searching for U-King.rpx . . .");

            Dictionary<string, string> paths = new();

            try
            {
                Interface.WriteLine("[UKING.UNSTABLE]");

                // Get SafeFiles [UNSTABLE]
                foreach (var dv in DriveInfo.GetDrives().Reverse())
                {
                    Interface.WriteLine($"[UKING.UNSTABLE] Searching {dv.Name}");

                    bool _break = false;

                    foreach (var file in Files.GetSafe(dv.Name, "U-King.rpx"))
                    {
                        Interface.WriteLine($"[UKING.UNSTABLE] Found {file}");

                        while (!Verify.UKing(file, ref paths))
                            break;

                        if (paths.ContainsKey("Game") && paths.ContainsKey("Update"))
                        {
                            Interface.WriteLine($"[UKING.UNSTABLE] Breaking Loop");

                            _break = true;
                            break;
                        }
                    }

                    if (_break) break;
                }
            }
            catch (Exception ex)
            {
                Interface.WriteLine($"[WARNING] {ex.Message}", ConsoleColor.DarkYellow);

                Interface.WriteLine("[UKING.STABLE]");

                // Get SafeFiles [STABLE]
                foreach (var dv in DriveInfo.GetDrives().Reverse())
                {
                    Interface.WriteLine($"[UKING.STABLE] Searching {dv.Name}");

                    bool _break = false;

                    foreach (var file in Files.GetSafeNoYield(dv.Name, "U-King.rpx"))
                    {
                        Interface.WriteLine($"[UKING.STABLE] Found {file}");

                        while (!Verify.UKing(file, ref paths))
                            break;

                        if (paths.ContainsKey("Game") && paths.ContainsKey("Update"))
                        {
                            Interface.WriteLine($"[UKING.STABLE] Breaking Loop");

                            _break = true;
                            break;
                        }
                    }

                    if (_break) break;
                }
            }

            if (!paths.ContainsKey("DLC"))
            {
                try
                {
                    Interface.WriteLine("[UKING.UNSTABLE.DLC]");

                    // Get SafeFiles [UNSTABLE]
                    foreach (var dv in DriveInfo.GetDrives().Reverse())
                    {
                        Interface.WriteLine($"[UKING.UNSTABLE.DLC] Searching {dv.Name}");

                        foreach (var file in Files.GetSafe(dv.Name, "RollpictDLC001.sbstftex"))
                        {
                            while (!Verify.RollPictDLC(file, ref paths))
                                break;

                            if (paths.ContainsKey("DLC"))
                                return paths;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Interface.WriteLine($"[WARNING] {ex.Message}", ConsoleColor.DarkYellow);

                    Interface.WriteLine("[UKING.STABLE.DLC]");

                    // Get SafeFiles [STABLE]
                    foreach (var dv in DriveInfo.GetDrives().Reverse())
                    {
                        Interface.WriteLine($"[UKING.STABLE.DLC] Searching {dv.Name}");

                        foreach (var file in Files.GetSafeNoYield(dv.Name, "RollpictDLC001.sbstftex"))
                        {
                            while (!Verify.RollPictDLC(file, ref paths))
                                break;

                            if (paths.ContainsKey("DLC"))
                                return paths;
                        }
                    }
                }
            }

            foreach (var set in Verify.Checks)
                if (!paths.ContainsKey(set.Key))
                    paths.Add(set.Key, "ERROR - NOT FOUND");

            return paths;
        }

        /// <summary>
        /// Searches for Python and returns a string dictating the path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string Python(string path = "A:")
        {
            Interface.WriteLine("Searching for python.exe . . .");

            // Check given path
            if (File.Exists($"{path}\\python38.dll") || File.Exists($"{path}\\python37.dll"))
            {
                return path;
            }

            // Check environment variables (PATH)
            string[] env = Environment.GetEnvironmentVariable("PATH").Split(';');
            foreach (string env_path in env)
            {
                if (File.Exists($"{env_path}\\python38.dll") || File.Exists($"{env_path}\\python37.dll"))
                    return env_path;
            }

            // Check PC
            try
            {
                foreach (var dv in DriveInfo.GetDrives())
                {
                    foreach (var file in Files.GetSafe(dv.Name, "python.exe"))
                    {
                        FileInfo info = new(file);

                        if (File.Exists($"{info.DirectoryName}\\python38.dll") || File.Exists($"{info.DirectoryName}\\python37.dll"))
                        {
                            // Add 'Python' to PATH
                            PATH.AddEntry(info.DirectoryName);
                            Interface.WriteLine($"Added '{info.DirectoryName}' to PATH", ConsoleColor.DarkGreen);

                            // Add 'Python\Scripts' to PATH
                            PATH.AddEntry($"{info.DirectoryName}\\Scripts");
                            Interface.WriteLine($"Added '{info.DirectoryName}\\Scripts' to PATH", ConsoleColor.DarkGreen);

                            // Return 'Python'
                            return info.DirectoryName;
                        }
                    }
                }
            }
            catch
            {
                foreach (var dv in DriveInfo.GetDrives())
                {
                    foreach (var file in Files.GetSafeNoYield(dv.Name, "python.exe"))
                    {
                        FileInfo info = new(file);

                        if (File.Exists($"{info.DirectoryName}\\python38.dll") || File.Exists($"{info.DirectoryName}\\python37.dll"))
                        {
                            // Add 'Python' to PATH
                            PATH.AddEntry(info.DirectoryName);
                            Interface.WriteLine($"Added '{info.DirectoryName}' to PATH", ConsoleColor.DarkGreen);

                            // Add 'Python\Scripts' to PATH
                            PATH.AddEntry($"{info.DirectoryName}\\Scripts");
                            Interface.WriteLine($"Added '{info.DirectoryName}\\Scripts' to PATH", ConsoleColor.DarkGreen);

                            // Return 'Python'
                            return info.DirectoryName;
                        }
                    }
                }
            }

            return "";
        }
    }
}
