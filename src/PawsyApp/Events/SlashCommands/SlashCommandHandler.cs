using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace PawsyApp.Events.SlashCommands;
internal class SlashCommandHandler
{
    internal delegate Task SlashHandler(SocketSlashCommand command);
    internal static ConcurrentDictionary<string, SlashHandler> Handlers = [];
    internal static async Task Respond(SocketSlashCommand command)
    {
        if (Handlers.TryGetValue(command.CommandName, out SlashHandler? handler))
        {
            await handler(command);
        }
    }
}

internal static class SlashExt
{
    internal static void RegisterSlashCommand(this ISlashCommand slash) => _ = SlashCommandHandler.Handlers.TryAdd(
            slash.BuiltCommand.Name.GetValueOrDefault(), slash.Handler);
}
