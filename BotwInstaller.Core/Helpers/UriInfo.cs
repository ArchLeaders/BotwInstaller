namespace BotwInstaller.Core.Helpers
{
    public class UriInfo
    {
        private static Dictionary<string, object> cache = new();

        public static string Get(string key)
        {
            Sync();
            return (string)cache[key];
        }

        public static void Sync(bool reload = false)
        {
            if (cache.Count == 0 || reload) {
                cache = new Resource("BotwInstaller.Core/Data/UriInfo.json").ToJson<Dictionary<string, object>>() ?? new();
            }
        }
    }
}
