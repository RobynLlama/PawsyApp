using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
            await item.RunOnGuild(command);
        }
    }

    internal static async Task AddCommandsToGuild(SocketGuild guild)
    {
        List<Task> tasks = [];

        foreach (var item in Handlers)
        {
            tasks.Add(guild.CreateApplicationCommandAsync(item.Value.BuiltCommand));
        }

        await Task.WhenAll(tasks);
        return;
    }

    internal static void RegisterAllModules()
    {
        new SlashMeow().RegisterSlashCommand();
        new SlashHotReload().RegisterSlashCommand();
    }
}

internal static class SlashExt
{
    internal static void RegisterSlashCommand(this ISlashCommand slash) => _ = SlashCommandHandler.Handlers.TryAdd(
            slash.BuiltCommand.Name.GetValueOrDefault(), slash);
}
