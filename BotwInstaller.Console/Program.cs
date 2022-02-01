using BotwInstaller.Lib;
using System.Diagnostics;

try
{
    //Console.WriteLine("Renaming U-Kings...");

    //foreach (var drive in DriveInfo.GetDrives())
    //    foreach (var file in Files.GetSafeNoYield(drive.Name, "U-King.rpx"))
    //    {
    //        Console.WriteLine(file);
    //        File.Move(file, $"{file}!");
    //    }

    //Console.WriteLine("\n");

    Stopwatch watch = new();

    watch.Start();

    var aio = await GameInfo.GetFiles();
    string strAio = "\n";

    foreach (var set in aio)
        strAio = $"{strAio}{set.Key}: {set.Value}\n";
    
    Console.WriteLine(strAio);

    watch.Stop();

    Console.WriteLine($"Completed in {watch.ElapsedMilliseconds / 1000.0} seconds.\n");

    //Console.WriteLine("Renaming U-Kings...");

    //foreach (var drive in DriveInfo.GetDrives())
    //    foreach (var file in Files.GetSafeNoYield(drive.Name, "U-King.rpx!"))
    //    {
    //        Console.WriteLine(file);
    //        File.Move(file, file.Replace("!", ""));
    //    }
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
