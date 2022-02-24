using System.Xml.Serialization;

namespace BotwInstaller.Lib.Configurations.Cemu
{
    interface DSUMappings
    {
        public static int DSU_A { get; } = 13;
        public static int DSU_B { get; } = 14;
        public static int DSU_X { get; } = 12;
        public static int DSU_Y { get; } = 15;
    }

    public class ControllerProfile
    {
        public static void Write(Config conf, string name = "XInput", int a = 13, int b = 12, int x = 15, int y = 14)
        {
            A = a; B = b; X = x; Y = y; Api = conf.ControllerApi;
            Directory.CreateDirectory($"{conf.Dirs.Cemu}\\controllerProfiles");

            XmlSerializerNamespaces xmlns = new();
            xmlns.Add(string.Empty, string.Empty);

            EmulatedControllerClass ecc = new();
            ecc.profile = name;

            if (conf.ControllerApi.StartsWith("SDLController"))
            {
                List<MappingClass> SetMapping(SortedDictionary<int, int> mapping)
                {
                    List<MappingClass> mappings = new();

                    foreach (var set in mapping)
                        mappings.Add(new MappingClass() { mapping = set.Key, button = set.Value });

                    return mappings;
                }

                SortedDictionary<int, int> leftMapping = new()
                {
                    { 5, 9 },
                    { 7, 42 },
                    { 10, 4 },
                    { 11, 11 },
                    { 12, 12 },
                    { 13, 13 },
                    { 14, 14 },
                    { 15, 7 },
                    { 17, 45 },
                    { 18, 39 },
                    { 19, 44 },
                    { 20, 38 },
                    { 25, 15 }
                };

                SortedDictionary<int, int> rightMapping = new()
                {
                    { 1, 0 },
                    { 2, 1 },
                    { 3, 2 },
                    { 4, 3 },
                    { 6, 10 },
                    { 8, 43 },
                    { 9, 6 },
                    { 16, 8 },
                    { 21, 47 },
                    { 22, 41 },
                    { 23, 46 },
                    { 24, 40 }
                };

                if (conf.ControllerApi.EndsWith("Joycon"))
                {
                    ecc.controller.Clear();

                    ecc.controller.Add(new()
                    {
                        api = "SDLController",
                        uuid = "0_030000007e0500000620000000006800",
                        display_name = "Nintendo Switch Joy-Con Left",
                        mappings = SetMapping(leftMapping)
                    });

                    ecc.controller.Add(new()
                    {
                        api = "SDLController",
                        uuid = "0_030000007e0500000720000000006800",
                        display_name = "Nintendo Switch Joy-Con Right",
                        mappings = SetMapping(rightMapping)
                    });
                }
                else
                {
                    SortedDictionary<int, int> mappings = new();

                    foreach (var set in leftMapping)
                        mappings.Add(set.Key, set.Value);

                    foreach (var set in rightMapping)
                        mappings.Add(set.Key, set.Value);

                    ecc.controller[0].uuid = "0_030000007e0500000920000000006800";
                    ecc.controller[0].display_name = "Nintendo Switch Pro Controller";
                    ecc.controller[0].mappings = SetMapping(mappings);
                }
            }

            XmlSerializer serializer = new XmlSerializer(typeof(EmulatedControllerClass));
            FileStream stream = File.OpenWrite($"{conf.Dirs.Cemu}\\controllerProfiles\\{name}.xml");

            serializer.Serialize(stream, ecc, xmlns);
            stream.Dispose();
        }

        public EmulatedControllerClass emulated_controller { get; set; } = new();
        public static int A { get; set; } = 13;
        public static int B { get; set; } = 12;
        public static int X { get; set; } = 15;
        public static int Y { get; set; } = 14;
        public static string Api { get; set; } = "XInput";

        [XmlRoot(Namespace = null, ElementName = "emulated_controller")]
        public class EmulatedControllerClass
        {
            public string type { get; set; } = "Wii U GamePad";
            public string profile { get; set; } = "JP";

            [XmlElement("User", ElementName = "controller")]
            public List<ControllerClass> controller { get; set; } = new() { new ControllerClass() };

        }
        public class ControllerClass
        {
            public string api { get; set; } = Api;
            public string uuid { get; set; } = "0";
            public string display_name { get; set; } = "Controller 1";
            public bool motion { get; set; } = true;
            public int rumble { get; set; } = 10;
            public RangeClass axis { get; set; } = new();
            public RangeClass rotation { get; set; } = new();
            public RangeClass trigger { get; set; } = new();

            [XmlArrayItem(ElementName = "entry")]
            public List<MappingClass> mappings { get; set; } = new()
            {
                new() { mapping = 1, button = A },
                new() { mapping = 2, button = B },
                new() { mapping = 3, button = X },
                new() { mapping = 4, button = Y },
                new() { mapping = 5, button = 10 },
                new() { mapping = 6, button = 11 },
                new() { mapping = 7, button = 8 },
                new() { mapping = 8, button = 9 },
                new() { mapping = 9, button = 3 },
                new() { mapping = 10, button = 0 },
                new() { mapping = 11, button = 4 },
                new() { mapping = 12, button = 6 },
                new() { mapping = 13, button = 7 },
                new() { mapping = 14, button = 5 },
                new() { mapping = 15, button = 1 },
                new() { mapping = 16, button = 2 },
                new() { mapping = 17, button = 39 },
                new() { mapping = 18, button = 45 },
                new() { mapping = 19, button = 44 },
                new() { mapping = 20, button = 38 },
                new() { mapping = 21, button = 41 },
                new() { mapping = 22, button = 47 },
                new() { mapping = 23, button = 46 },
                new() { mapping = 24, button = 40 },
            };
        }
        public class RangeClass
        {
            public double deadzone { get; set; } = 0.25;
            public int range { get; set; } = 1;
        }
        
        public class MappingClass
        {
            public int mapping { get; set; } = 0;
            public int button { get; set; } = 0;
        }
    }
}
