using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotwInstaller.Core
{
    public class Root
    {
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

        // Assigned Properties
        public string BaseGame { get; set; } = "Not Found";
        public string Update { get; set; } = "Not Found";
        public string Dlc { get; set; } = "Not Found";
        public string BaseGameNx { get; set; } = "Not Found";
        public string DlcNX { get; set; } = "Not Found";
        public string Cemu { get; set; } = "Not Found";
        public string Yuzu { get; set; } = "Not Found";
        public string Python { get; set; } = "Not Found";
        public string PythonVersion { get; set; } = "Not Found";
        public string NetVersion { get; set; } = "Not Found";
        public bool HasPython { get; set; } = false;
        public bool HasBcml { get; set; } = false;
        public bool HasYuzu { get; set; } = false;
        public bool HasCemu { get; set; } = false;

        // Floating Data
        public List<string> UKingFiles { get; set; } = new();
        public List<string> RollPictFiles { get; set; } = new();
        public List<string> PythonFiles { get; set; } = new();
    }
}
