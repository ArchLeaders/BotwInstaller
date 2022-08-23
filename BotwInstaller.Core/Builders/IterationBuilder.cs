using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotwInstaller.Core.Builders
{
    public class IterationBuilder
    {
        private readonly string path;
        private readonly string filter;
        private readonly List<string> files = new();

        public async Task<List<string>> GetAsync()
        {
            await IterateDirectory(path);
            return files;
        }

        private async Task IterateDirectory(string folder)
        {
            string[] dirs;
            try {
                dirs = Directory.GetDirectories(folder);
            }
            catch { return; }

            List<Task> tasks = new();

            foreach (var dir in dirs) {
                tasks.Add(IterateDirectory(dir));
            }

            tasks.Add(Task.Run(() => {
                foreach (var file in Directory.GetFiles(folder, filter)) {
                    files.Add(file);
                }
            }));

            await Task.WhenAll(tasks);
        }

        public IterationBuilder(string path, string filter)
        {
            this.path = path;
            this.filter = filter;
        }
    }
}
