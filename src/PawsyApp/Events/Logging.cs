using System.Threading.Tasks;
using Discord;
using PawsyApp.Utils;

namespace PawsyApp.Events;

internal class LogEvent
{
    internal static Task Respond(LogMessage msg)
    {
        WriteLog.Normally(msg);
        return Task.CompletedTask;
    }
}
