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
        await Task.Delay(-1);
    }
}
