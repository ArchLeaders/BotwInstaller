using BotwInstaller.Core.Extensions;
using Nintendo.Yaz0;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BotwInstaller.Core.Helpers
{
    public class Resource
    {
        private const string BaseUrl = "https://raw.githubusercontent.com/ArchLeaders/BotwInstaller/master";

        public byte[] Data { get; set; } = Array.Empty<byte>();
        public Resource(string resource, bool isCompressed = false)
        {
            resource = resource.ToSystemPath();
            Stream? stream = Assembly.GetCallingAssembly().GetManifestResourceStream($"BotwInstaller.{resource.Replace("\\", ".")}");

            if (stream == null) {

                try {
                    using HttpClient client = new();
                    stream = client.GetStreamAsync($"{BaseUrl}/BotwInstaller.Core/{resource.Replace("\\", "/")}?v={Random.Shared.Next(1,100)}").Result;
                }
                catch { 
                    throw new FileNotFoundException($"Could not find the file 'BotwInstaller.{resource}'.");
                }
            }

            using BinaryReader reader = new(stream);
            byte[] data = reader.ReadBytes((int)stream.Length);
            Data = isCompressed ? Yaz0.Decompress(data) : data;
        }

        /// <summary>
        /// Returns a UTF8 encoded string of the resource.
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        public override string ToString() => Encoding.UTF8.GetString(Data);

        /// <summary>
        /// Returns a UTF8 encoded string of the resource.
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        public T? ToJson<T>() => JsonSerializer.Deserialize<T>(Data);
    }
}
