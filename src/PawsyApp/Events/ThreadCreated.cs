using System.Threading.Tasks;
using Discord.WebSocket;
using PawsyApp.Utils;

namespace PawsyApp.Events;

internal class ThreadEvent
{
    internal static async Task Respond(SocketThreadChannel channel)
    {
        await WriteLog.LineNormal("Heard a thread create event");
    }
}
