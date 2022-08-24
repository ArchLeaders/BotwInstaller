using BotwInstaller.Core.Helpers;
using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BotwInstaller.Core.Software
{
    public enum DotNetType { Runtime, SDK }

    public class DotNET
    {
        public static string Version {
            get {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    return (string)(Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\dotnet\\Setup\\InstalledVersions\\x64\\sharedhost", "Version", "-1") ?? "Not Found");
                }
                else {
                    return "Could not find .NET on non-win32 system";
                }
            }
        }

        public static async Task Install(DotNetType dotNetType = DotNetType.Runtime)
        {
            var url = UriInfo.Get($"DotNET.{dotNetType}");
            var path = $"{Environment.GetEnvironmentVariable("%TEMP%")}\\DotNET-{dotNetType}-Setup";

            using HttpClient client = new();
            byte[] bytes = await client.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(path, bytes);

            if (File.Exists(path)) {
                await Process.Start(path, "/quiet /norestart").WaitForExitAsync();
                File.Delete(path);
            }
        }
    }
}
