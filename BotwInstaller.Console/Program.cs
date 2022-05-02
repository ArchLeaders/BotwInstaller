using BotwInstaller.Lib;
using BotwScripts.Lib.Common;
using BotwScripts.Lib.Common.IO.FileSystems;

try
{
    await Installer.RunInstallerAsync(Interface.WriteLine, Interface.UpdateNull, new());
}
catch (Exception ex)
{
    string msg =
        $"Unhandled exception occured when starting the program.\n" +
        $"\n[EXCEPTION]\n{ex.Message}\n\n" +
        $"\n[STACK TRACE]\n{ex.StackTrace}\n\n" +
        $"\n[INNER EXCEPTION]\n{ex.InnerException}\n\n";

    File.WriteAllText(".\\ERROR.txt", msg);
    Console.WriteLine(msg);
    Console.ReadLine();
}