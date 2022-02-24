﻿using BotwInstaller.Lib;
using BotwInstaller.Lib.Configurations.Cemu;

try
{
    ///
    /// TEST CONTROLLER WRITER
    ///
    #region TEST CONTROLLER WRITER

    ControllerProfile.Write(new Config() { Dirs = { Cemu = "D:\\Cemu_Test" } }, "XInput");
    ControllerProfile.Write(new Config() { Dirs = { Cemu = "D:\\Cemu_Test" }, ControllerApi = "DSUController" }, "DSU");
    ControllerProfile.Write(new Config() { Dirs = { Cemu = "D:\\Cemu_Test" }, ControllerApi = "SDLController-Joycon" }, "SDL_Joycons");
    ControllerProfile.Write(new Config() { Dirs = { Cemu = "D:\\Cemu_Test" }, ControllerApi = "SDLController" }, "SDL_Pro");

    #endregion

    ///
    /// RENAME UKING FILES - TEST
    ///

    #region RENAME UKING FILES - TEST

    /*

    Console.WriteLine("Renaming U-Kings...");

    foreach (var drive in DriveInfo.GetDrives())
        foreach (var file in Files.GetSafeNoYield(drive.Name, "U-King.rpx"))
        {
            Console.WriteLine(file);
            File.Move(file, $"{file}!");
        }

    Console.WriteLine("\n");

    */

    #endregion

    ///
    /// SEARCH GAME PATH - TEST
    ///
    #region SEARCH GAME PATH - TEST

    /*

    Stopwatch watch = new();

    watch.Start();

    var aio = await GameInfo.GetFiles();
    string strAio = "\n";

    foreach (var set in aio)
        strAio = $"{strAio}{set.Key}: {set.Value}\n";
    
    Console.WriteLine(strAio);

    watch.Stop();

    Console.WriteLine($"Completed in {watch.ElapsedMilliseconds / 1000.0} seconds.\n");

    */

    #endregion

    ///
    /// UNDO RENAME UKING FILES - TEST
    ///
    #region UNDO RENAME UKING FILES - TEST

    /*

    Console.WriteLine("Renaming U-Kings...");

    foreach (var drive in DriveInfo.GetDrives())
        foreach (var file in Files.GetSafeNoYield(drive.Name, "U-King.rpx!"))
        {
            Console.WriteLine(file);
            File.Move(file, file.Replace("!", ""));
        }

    */

    #endregion
}
catch (Exception ex)
{
    Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
}
finally
{
    Console.WriteLine("\nPress enter to continue . . .");
    Console.ReadLine();
}
