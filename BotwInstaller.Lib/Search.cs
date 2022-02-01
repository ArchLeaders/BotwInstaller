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
            if (File.Exists($"{path}\\Cemu.exe"))
            {
                Interface.WriteLine($"[PARAM.CEMU.SEARCH] Cemu found in '{path}'", ConsoleColor.DarkGray);

                // Add cemu path
                paths.Add("Cemu", path);

                // Check for botw install

                // Return dict
                return paths;
            }

            // Check root folder
            if (File.Exists($"{Directory.GetCurrentDirectory()}\\Cemu.exe"))
            {
                Interface.WriteLine($"[ROOT.CEMU.SEARCH] Cemu found in '{Directory.GetCurrentDirectory()}'", ConsoleColor.DarkGray);
                paths.Add("Cemu", Directory.GetCurrentDirectory());

                // Check for botw install

                return paths;
            }

            // Check PC
            try
            {
                foreach (var dv in DriveInfo.GetDrives().Reverse())
                {
                    foreach (var file in Files.GetSafe(dv.Name, "Cemu.exe"))
                    {
                        var dir = new FileInfo(file).DirectoryName;
                        Interface.WriteLine($"[CEMU.SEARCH] Cemu found in '{dir}'", ConsoleColor.DarkGray);
                        paths.Add("Cemu", dir);

                        Verify.CheckMlc(dir, ref paths);

                        return paths;
                    }
                }
            }
            catch
            {
                foreach (var dv in DriveInfo.GetDrives().Reverse())
                {
                    if (paths.ContainsKey("Cemu")) return paths;

                    foreach (var file in Files.GetSafeNoYield(dv.Name, "Cemu.exe"))
                    {
                        var dir = new FileInfo(file).DirectoryName;
                        Interface.WriteLine($"[CEMU.SEARCH] Cemu found in '{dir}'", ConsoleColor.DarkGray);
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
                // Get SafeFiles [UNSTABLE]
                foreach (var dv in DriveInfo.GetDrives().Reverse())
                {
                    bool _break = false;

                    foreach (var file in Files.GetSafe(dv.Name, "U-King.rpx"))
                    {
                        while (!Verify.UKing(file, ref paths))
                            continue;

                        if (paths.ContainsKey("Game") && paths.ContainsKey("Update"))
                        {
                            _break = true;
                            break;
                        }
                    }

                    if (_break) break;
                }
            }
            catch
            {
                // Get SafeFiles [STABLE]
                foreach (var dv in DriveInfo.GetDrives().Reverse())
                {
                    bool _break = false;

                    foreach (var file in Files.GetSafeNoYield(dv.Name, "U-King.rpx"))
                    {
                        while (!Verify.UKing(file, ref paths))
                            continue;

                        if (paths.ContainsKey("Game") && paths.ContainsKey("Update"))
                        {
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
                    // Get SafeFiles [UNSTABLE]
                    foreach (var dv in DriveInfo.GetDrives().Reverse())
                    {
                        foreach (var file in Files.GetSafe(dv.Name, "RollpictDLC001.sbstftex"))
                        {
                            while (!Verify.RollPictDLC(file, ref paths))
                                continue;

                            if (paths.ContainsKey("DLC"))
                                return paths;
                        }
                    }
                }
                catch
                {
                    // Get SafeFiles [STABLE]
                    foreach (var dv in DriveInfo.GetDrives().Reverse())
                    {
                        foreach (var file in Files.GetSafeNoYield(dv.Name, "RollpictDLC001.sbstftex"))
                        {
                            while (!Verify.RollPictDLC(file, ref paths))
                                continue;

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
