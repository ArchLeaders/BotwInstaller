Console.WriteLine("Scanning source files . . .\n");

var baseId = "01007EF00011E000";
var dlcId = "01007EF00011F001";

var mode = "Base";
int i = 1;

var header = new string(
    "namespace BotwScripts.Lib.CheckSum\n" +
    "{\n" +
    $"	public class {mode}\n" +
    "	{\n" +
    "		public static List<string> Set(string gameFolder)\n" +
    "		{\n" +
    "			return new()\n" +
    "			{\n");

var footer = new string(
    "			};\n" +
    "		}\n" +
    "	}\n" +
    "}\n");

if (Directory.Exists($".\\{baseId}"))
{
    Console.WriteLine($"Loading {baseId} . . .");
    File.WriteAllText($".\\{mode}.cs", header);
    string[] files = Directory.GetFiles($".\\{baseId}", "*.*", SearchOption.AllDirectories);

    foreach (var file in files)
    {
        _ = Task.Run(() => Console.Out.WriteAsync($"\r{i++}/{files.Length}"));
        File.AppendAllText($".\\{mode}.cs", $"\t\t\t\t$\"{file.Replace($".\\{baseId}\\romfs\\", "{gameFolder}").Replace("\\", "\\\\")}\",\n");
    }

    File.AppendAllText($".\\{mode}.cs", footer);
}

if (Directory.Exists($".\\{dlcId}"))
{
    i = 1;
    mode = "DLC";
    Console.WriteLine($"\n\nLoading {dlcId} . . .");
    File.WriteAllText($".\\{mode}.cs", header);
    string[] files = Directory.GetFiles($".\\{dlcId}", "*.*", SearchOption.AllDirectories);

    foreach (var file in files)
    {
        _ = Task.Run(() => Console.Out.WriteAsync($"\r{i++}/{files.Length}"));
        File.AppendAllText($".\\{mode}.cs", $"\t\t\t\t$\"{file.Replace($".\\{dlcId}\\romfs\\", "{gameFolder}").Replace("\\", "\\\\")}\",\n");
    }

    File.AppendAllText($".\\{mode}.cs", footer);
}

Console.WriteLine($"\n\nDone!");
Console.ReadLine();