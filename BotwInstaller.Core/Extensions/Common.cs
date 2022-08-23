using BotwInstaller.Core.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotwInstaller.Core.Extensions
{
    public static class Common
    {
        public static async Task<List<string>> GetFilesAsync(this string path, string filter)
            => await new IterationBuilder(path, filter).GetAsync();

        public static async Task<List<string>> GetFilesAsync(this DirectoryInfo path, string filter)
            => await new IterationBuilder(path.FullName, filter).GetAsync();
    }
}
