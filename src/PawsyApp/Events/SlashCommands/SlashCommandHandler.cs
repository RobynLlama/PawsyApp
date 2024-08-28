using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace PawsyApp.Events.SlashCommands;
internal class SlashCommandHandler
{
    internal delegate Task SlashHandler(SocketSlashCommand command);
    internal static ConcurrentDictionary<string, ISlashCommand> Handlers = [];
    internal static async Task Respond(SocketSlashCommand command)
    {
        if (Handlers.TryGetValue(command.CommandName, out ISlashCommand? item))
        {
            await item.Handler(command);
        }
    }

    internal static void RegisterAllHandlers()
    {
        //All modules should be created and ready to use here
        new SlashMeow().RegisterSlashCommand();
    }
}

internal static class SlashExt
{
    internal static void RegisterSlashCommand(this ISlashCommand slash) => _ = SlashCommandHandler.Handlers.TryAdd(
            slash.BuiltCommand.Name.GetValueOrDefault(), slash);
}
