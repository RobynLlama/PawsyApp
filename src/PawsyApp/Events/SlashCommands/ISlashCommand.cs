using Discord;
using Discord.WebSocket;
using PawsyApp.GuildStorage;
using PawsyApp.PawsyCore.Modules;
using PawsyApp.Utils;
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
        && (CommonGetters.GetSettings(realID) is GuildSettings gSettings)
        && gSettings.EnabledModules.TryGetValue(ModuleName, out bool enabled)
        && enabled)
            return Handler(command);

        return command.RespondAsync("Sorry, that command is not enabled on this server, meow :sob:", ephemeral: true);
    }
}

