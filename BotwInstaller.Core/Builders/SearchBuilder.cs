using BotwInstaller.Core.Extensions;
using BotwInstaller.Core.Software;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BotwInstaller.Core.Builders
{
    public class SearchBuilder
    {
        private readonly Print print;
        private readonly Dictionary<string, string> filterMap = new() {
            { "u-king.rpx", "UKingFiles" },
            { "rollpictdlc001.sbstftex", "RollPictFiles" },
            { "agl_resource.nngfx_nx_nvn.release.ssarc", "AglResourceFiles" },
            { "cemu.exe", "CemuFiles" },
            { "python.exe", "PythonFiles" },
            { "usbhelperlauncher.exe", "TheHighSeas" }
        };

        public Root root = new();

        public async Task<Root> RunAsync()
        {
            List<Task> tasks = new();
            List<Task> setters = new();
            List<string> output = new();

            var drives = DriveInfo.GetDrives();
            print($"System Search Initialized. ETA: {drives.Length}min");

            foreach (var drive in drives) {
                tasks.Add(Task.Run(async () => {

                    var files = await drive.Name.GetFilesAsync("*.*");
                    foreach (var file in files) {
                        string name = Path.GetFileName(file).ToLower();
                        if (filterMap.ContainsKey(name)) {
                            setters.Add(Task.Run(() => ((List<string>)root[filterMap[name]]!).Add(file)));
                        }
                    }
                }));
            }

            tasks.Add(Task.Run(() => {
                root.NetVersion = DotNET.Version;
            }));

            await Task.WhenAll(setters);
            await Task.WhenAll(tasks);

            File.WriteAllText("D:\\root.json", JsonSerializer.Serialize(root));

            tasks.Clear();
            tasks.Add(CheckPython());
            tasks.Add(CheckCemu());
            tasks.Add(CheckYuzu());
            tasks.Add(CheckGame());
            tasks.Add(CheckLegality());

            await Task.WhenAll(tasks);

            File.WriteAllText("D:\\c-root.json", JsonSerializer.Serialize(root));
            return root;
        }

        private Task CheckPython()
        {
            string[] pathVars = Environment.GetEnvironmentVariable("PATH")!.Split(";");
            foreach (var file in root.PythonFiles) {
                string dir = Path.GetDirectoryName(file) ?? "NULL";

                // Check for python install files
                // to validate a full install
                if ((File.Exists($"{dir}\\python37.dll") || File.Exists($"{dir}\\python38.dll")) && File.Exists($"{dir}\\python3.dll")) {

                    // Add a valid dir
                    root.Python.Add(dir, pathVars.Contains(dir) && pathVars.Contains($"{dir}\\Scripts\\"));

                    // Check for packages on valid install
                    root.HasBcml = File.Exists($"{dir}\\Lib\\site-packages\\bcml\\__main__.py");
                    root.HasPip = File.Exists($"{dir}\\Lib\\site-packages\\pip\\__main__.py");

                    continue;
                }

                // Add all dirs found in the PATH to
                // the corresponding root field
                if (pathVars.Contains(dir) && pathVars.Contains($"{dir}\\Scripts\\")) {
                    root.PythonPathDirs.Add(dir);
                }
            }

            return Task.CompletedTask;
        }

        private Task CheckCemu()
        {
            foreach (var file in root.CemuFiles) {
                string? dir = Path.GetDirectoryName(file);
                if (dir != null) {
                    root.Cemu.Add(dir, File.Exists($"{dir}\\settings.xml"));
                    root.HasCemu = File.Exists($"{dir}\\settings.xml");
                }
            }

            return Task.CompletedTask;
        }

        private Task CheckYuzu()
        {
            root.HasYuzu = File.Exists($"{Environment.GetEnvironmentVariable("%LOCALAPPDATA%")}\\yuzu\\yuzu-windows-msvc\\yuzu.exe");
            return Task.CompletedTask;
        }

        private async Task CheckGame()
        {
            List<Task> tasks = new() {
                CheckGameFiles(root.UKingFiles, 2),
                CheckGameFiles(root.RollPictFiles, 4),
                CheckGameFiles(root.AglResourceFiles, 4),
            };

            await Task.WhenAll(tasks);
        }

        private async Task CheckGameFiles(List<string> roots, int trace)
        {
            List<Task> tasks = new();

            foreach (var file in roots) {
                var split = file.ToSystemPath().Split("\\");
                var _root = string.Join("\\", split[0..(split.Length-trace)]);

                root.HasSailedTheHighSeas = File.Exists($"{_root}\\content\\System\\BuildTime.txt");

                if (_root.EndsWith("content")) {
                    _root = string.Join("\\", split[0..(split.Length - trace - 1)]);
                }
                
                tasks.Add(Task.Run(async() => {
                    var prop = BotwInfo.GetTitleId(_root, TitleIdType.CommonName);
                    var check = await BotwInfo.CheckFiles(_root);

                    ((Dictionary<string, bool>)root[prop]!).Add(_root, check.Key);

                    if (check.Value.Count > 0) {
                        ((Dictionary<string, List<string>>)root[prop + "Missing"]!).Add(_root, check.Value);
                    }
                }));
            }

            await Task.WhenAll(tasks);
        }

        private Task CheckLegality()
        {
            root.HasSailedTheHighSeas = root.TheHighSeas.Count > 0;
            return Task.CompletedTask;
        }

        public SearchBuilder(Print print)
        {
            this.print = print;
        }
    }
}
