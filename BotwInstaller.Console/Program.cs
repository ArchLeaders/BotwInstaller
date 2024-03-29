﻿using BotwInstaller.Lib;
using BotwScripts.Lib.Common;

try
{
    Config conf = new();

    Console.Write("Please select a mode:\n\n1. Cemu\n2. Switch/Yuzu\n3. WiiU\nX. Exit\n\n> ");
    var rl = Console.ReadLine();

    if (rl == "1")
        conf.UseCemu = true;
    else if (rl == "2")
        conf.IsNX = true;
    else if (rl != "3")
        return;

    Console.WriteLine();

    await Installer.RunInstallerAsync(Interface.WriteLine, Interface.UpdateNull, conf);
}
catch (Exception ex)
{
    string msg =
        $"An exception occured when starting the program.\n" +
        $"\n[EXCEPTION]\n{ex.Message}\n\n" +
        $"\n[STACK TRACE]\n{ex.StackTrace}\n\n" +
        $"\n[INNER EXCEPTION]\n{ex.InnerException}\n\n";

    File.WriteAllText(".\\ERROR.txt", msg);
    Console.WriteLine(msg);
    Console.ReadLine();
}