using System.Text;

namespace BotwInstaller.Core
{
    public class Root
    {
        public string NetVersion { get; set; } = "Not Found";
        public bool HasPython { get; set; } = false;
        public bool HasPip { get; set; } = false;
        public bool HasBcml { get; set; } = false;
        public bool HasYuzu { get; set; } = false;
        public bool HasCemu { get; set; } = false;
        public bool HasSailedTheHighSeas { get; set; } = false;

        public Dictionary<string, bool> BaseGame { get; set; } = new();
        public Dictionary<string, bool> Update { get; set; } = new();
        public Dictionary<string, bool> Dlc { get; set; } = new();
        public Dictionary<string, bool> BaseGameNx { get; set; } = new();
        public Dictionary<string, bool> DlcNx { get; set; } = new();
        public Dictionary<string, bool> Cemu { get; set; } = new();
        public Dictionary<string, bool> Python { get; set; } = new();
        public List<string> Yuzu { get; set; } = new();

        public Dictionary<string, List<string>> BaseGameMissing { get; set; } = new();
        public Dictionary<string, List<string>> UpdateMissing { get; set; } = new();
        public Dictionary<string, List<string>> DlcMissing { get; set; } = new();
        public Dictionary<string, List<string>> BaseGameNxMissing { get; set; } = new();
        public Dictionary<string, List<string>> DlcNxMissing { get; set; } = new();

        public List<string> UKingFiles { get; set; } = new();
        public List<string> RollPictFiles { get; set; } = new();
        public List<string> AglResourceFiles { get; set; } = new();
        public List<string> CemuFiles { get; set; } = new();
        public List<string> PythonFiles { get; set; } = new();
        public List<string> PythonPathDirs { get; set; } = new();
        public List<string> TheHighSeas { get; set; } = new();


        public object? this[string key]
        {
            get {
                try {
                    return typeof(Root).GetProperty(key)!.GetValue(this);
                }
                catch {
                    throw new KeyNotFoundException($"Could not find property '{key}' in '{this}'");
                }
            }
            set {
                try {
                    typeof(Root).GetProperty(key)!.SetValue(this, value);
                }
                catch {
                    throw new KeyNotFoundException($"Could not find property '{key}' in '{this}'");
                }
            }
        }

        public object? GetDefault(string name)
        {
            var obj = this[name];
            if (obj is List<string> list) {
                return list.FirstOrDefault("Not Found");
            }
            else {
                return obj;
            }
        }

        public override string ToString()
        {
            StringBuilder str = new();
            foreach (var prop in typeof(Root).GetProperties()) {
                str.AppendLine($"{prop.Name}: {GetDefault(prop.Name)}");
            }

            return str.ToString();
        }
    }
}
