using System;
using System.Threading.Tasks;
using Discord;
using PawsyApp.KittyColors;

namespace PawsyApp.Events;

internal class LogEvent
{
    internal static Task SocketRespond(LogMessage msg)
    {
        Console.WriteLine($"[{KittyColor.WrapInColor("Socket", ColorCode.Yellow)}]  {msg}");
        return Task.CompletedTask;
    }

    internal static Task RestRespond(LogMessage msg)
    {
        Console.WriteLine($"[{KittyColor.WrapInColor("RestAPI", ColorCode.Yellow)}] {msg}");
        return Task.CompletedTask;
    }
}
