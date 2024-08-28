using Discord;
using Discord.WebSocket;
using PawsyApp.GuildStorage;
using PawsyApp.Settings;
using System;
using System.Threading.Tasks;

namespace PawsyApp.Events.SlashCommands;

internal interface ISlashCommand
{
    SlashCommandHandler.SlashHandler Handler { get; }
    SlashCommandProperties BuiltCommand { get; }
    string ModuleName { get; }

    Task RunOnGuild(SocketSlashCommand command)
    {
        if ((command.GuildId is ulong realID)
        && AllSettings.GuildSettingsStorage.TryGetValue(realID, out GuildSettings? gSettings)
        && gSettings.EnabledModules.TryGetValue(ModuleName, out bool enabled)
        && enabled)
            return Handler(command);

        return command.RespondAsync("Sorry, that command is not enabled on this server, meow :sob:", ephemeral: true);
    }
}

