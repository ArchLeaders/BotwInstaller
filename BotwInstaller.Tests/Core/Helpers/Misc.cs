using BotwInstaller.Core;
using BotwInstaller.Core.Builders;
using BotwInstaller.Core.Extensions;
using BotwInstaller.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BotwInstaller.Tests.Core.Helpers
{
    [TestClass]
    public class Misc
    {
        [TestMethod]
        public async Task TestFunc()
        {
            // var check = await BotwInfo.CheckFiles(@"D:\Botw\Cemu (Stable)\mlc01\usr\title\0005000e\101c9500\");
            // Debug.Write(check.Key ? "Game files verified successfully." : $"Found {check.Value.Count} missing file(s) in 'D:\\Botw\\Cemu (Stable)\\mlc01\\usr\\title\\0005000e\\101c9500'");

            // Full took 54.4 sec
            // Partial tool 35.5

            Root root = await new SearchBuilder(Print).RunAsync();
            File.WriteAllText("D:\\root.json", JsonSerializer.Serialize(root));
        }

        private void Print(string txt) => Debug.WriteLine(txt);
    }
}
