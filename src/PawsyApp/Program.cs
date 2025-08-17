using System;
using System.Reflection;
using System.Threading.Tasks;
using PawsyApp.PawsyCore;

namespace PawsyApp;

public partial class PawsyProgram
{

    public static readonly Pawsy pawsy = new();
    public static bool running = true;
    public static async Task Main()
    {

        // Get the assembly informational version
        var informationalVersion = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "Unknown version";

        Console.WriteLine($"Pawsy version {informationalVersion}");

        var InputHandler = Task.Run(HandleInputAsync);

        while (running)
        {
            await Task.Delay(100);
        }
    }
}
