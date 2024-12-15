using System;
using System.Reflection;
using System.Threading.Tasks;
using PawsyApp.PawsyCore;

namespace PawsyApp;
public class PawsyProgram
{
    public static async Task Main()
    {

        // Get the assembly informational version
        var informationalVersion = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "Unknown version";

        Console.WriteLine($"Pawsy version {informationalVersion}");

        Pawsy pawsy = new();
        bool running = true;

        while (running)
        {
            await Task.Delay(100);

            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Q && key.Modifiers == ConsoleModifiers.Control)
                {
                    pawsy.Destroy();
                    running = false;
                }
            }
        }

        Console.WriteLine("Program exit");
    }
}
