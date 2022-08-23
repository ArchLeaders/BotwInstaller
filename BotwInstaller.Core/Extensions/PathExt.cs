using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotwInstaller.Core.Extensions
{
    public enum TrimPosition
    {
        /// <summary>
        /// Trim the start off the target path.
        /// </summary>
        Start,

        /// <summary>
        /// Trim the end off the target path.
        /// </summary>
        End,
    }

    public enum QueryFolderPosition
    {
        /// <summary>
        /// Use the first occurance if the folder keyword
        /// </summary>
        First,

        /// <summary>
        /// Use the last occurance if the folder keyword
        /// </summary>
        Last,
    }

    public static class PathManipulation
    {
        public static string TrimFrom(string path, string queryFolder, TrimPosition trimPosition = TrimPosition.Start, QueryFolderPosition keyFolderPosition = QueryFolderPosition.Last)
        {
            path = path.ToSystemPath();
            string[] split = path.Split("\\");
            List<string> pending = new(split);
            StringBuilder result = new();

            foreach (var folder in split) {
                result.Append($"{folder}\\");
                pending.Remove(folder);
                if (folder == queryFolder) {
                    if (keyFolderPosition == QueryFolderPosition.Last && pending.Contains(queryFolder)) {
                        continue;
                    }
                    break;
                }
            }

            var res = trimPosition == TrimPosition.Start ? path.Replace(result.ToString(), $"{queryFolder}\\") : result.ToString();
            return res;
        }

        public static string TrimStartFromFirst(this string path, string queryFolder)
            => TrimFrom(path, queryFolder, TrimPosition.Start, QueryFolderPosition.First);

        public static string TrimStartFromLast(this string path, string queryFolder)
            => TrimFrom(path, queryFolder, TrimPosition.Start, QueryFolderPosition.Last);

        public static string TrimEndFromFirst(this string path, string queryFolder)
            => TrimFrom(path, queryFolder, TrimPosition.End, QueryFolderPosition.First);

        public static string TrimEndFromLast(this string path, string queryFolder)
            => TrimFrom(path, queryFolder, TrimPosition.End, QueryFolderPosition.Last);

        public static string ToSystemPath(this string path)
            => path.Replace("/", "\\");
    }
}
