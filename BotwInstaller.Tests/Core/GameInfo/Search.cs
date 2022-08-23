using BotwScripts.Lib.Common.IO.FileSystems;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotwInstaller.Tests.Core.GameInfo
{
    [TestClass]
    public class Search
    {
        [TestMethod]
        public void Search1()
        {
            var files1 = BuildFileList.GetFiles("D:\\", "*.*");
            Debug.WriteLine(files1.Count);
        }

        [TestMethod]
        public async Task Search2()
        {
            var files2 = await new QueryBuilder("D:\\", "*.*").GetAsync();
            Debug.WriteLine(files2.Count);
        }
    }

    public class BuildFileList
    {
        public static List<FileInfo> GetFiles(string dir, string filter)
        {
            var di = new DirectoryInfo(dir);
            var directories = di.GetDirectories();
            var files = new List<FileInfo>();
            foreach (var directoryInfo in directories) {
                try {
                    GetFilesFromDirectory(directoryInfo.FullName, files, filter);
                }
                catch {}
            }
            return files;
        }


        private static void GetFilesFromDirectory(string directory, List<FileInfo> files, string filter)
        {
            var di = new DirectoryInfo(directory);
            var fs = di.GetFiles(filter, SearchOption.TopDirectoryOnly);
            files.AddRange(fs);
            var directories = di.GetDirectories();
            foreach (var directoryInfo in directories) {
                try {
                    GetFilesFromDirectory(directoryInfo.FullName, files, filter);
                }
                catch {}
            }
        }

    }

    public class QueryBuilder
    {
        private readonly string path;
        private readonly string filter;
        private readonly List<string> files = new();

        public async Task<List<string>> GetAsync()
        {
            await QueryFolder(path);
            return files;
        }

        private async Task QueryFolder(string folder)
        {
            string[] dirs;
            try {
                dirs = Directory.GetDirectories(folder);
            }
            catch { return; }
            
            List<Task> tasks = new();

            foreach (var dir in dirs) {
                tasks.Add(QueryFolder(dir));
            }

            tasks.Add(Task.Run(() => {
                foreach (var file in Directory.GetFiles(folder, filter)) {
                    files.Add(file);
                }
            }));

            await Task.WhenAll(tasks);
        }

        public QueryBuilder(string path, string filter)
        {
            this.path = path;
            this.filter = filter;
        }
    }
}
