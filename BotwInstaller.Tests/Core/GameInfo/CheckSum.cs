using BotwScripts.Lib.CheckSum;
using BotwScripts.Lib.Formats.Byml.ActorInfolib;
using Nintendo.Yaz0;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BotwInstaller.Tests.Core.GameInfo
{
    [TestClass]
    public class CheckSum
    {
        [TestMethod]
        public void SerializeCheckSum()
        {
            List<string> ids = new() {
                "01007EF00011E000",
                "01007EF00011F001",

                "00050000101C9500",
                "0005000E101C9500",
                "0005000C101C9500",

                "00050000101C9400",
                "0005000E101C9400",
                "0005000C101C9400",

                "00050000101C9300",
                "0005000E101C9300",
                "0005000C101C9300",
            };

            foreach (var id in ids) {
                var checksum = id switch {
                    "01007EF00011E000" => BaseNX.Set(""),
                    "01007EF00011F001" => DlcNX.Set(""),

                    "00050000101C9500" => BaseEU.Set(""),
                    "0005000E101C9500" => UpdateEU.Set(""),
                    "0005000C101C9500" => DlcEU.Set(""),

                    "00050000101C9400" => BaseUS.Set(""),
                    "0005000E101C9400" => UpdateUS.Set(""),
                    "0005000C101C9400" => DlcUS.Set(""),

                    "00050000101C9300" => BaseJP.Set(""),
                    "0005000E101C9300" => UpdateJP.Set(""),
                    "0005000C101C9300" => DlcJP.Set(""),
                    _ => new()
                };

                List<string> hashedChecksum = new();

                foreach (var name in checksum) {
                    hashedChecksum.Add(Crc32.FromString(name));
                }

                File.WriteAllBytes(
                    $@"D:\Visual Studio\Projects\- Botw Scripts\BotwInstaller (RE)\BotwInstaller\BotwInstaller.Core\GameInfo\CheckSum\{id}",
                    Yaz0.CompressFast(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(hashedChecksum)), 9)
                );
            }
        }
    }
}
