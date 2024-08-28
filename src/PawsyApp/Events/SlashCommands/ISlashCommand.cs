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
        if (command.GuildId is ulong realID)
        {
            if (AllSettings.GuildSettingsStorage.TryGetValue(realID, out GuildSettings? gSettings))
            {
                if (gSettings.EnabledModules.TryGetValue(ModuleName, out bool enabled))
                {
                    if (enabled)
                    {
                        return Handler(command);
                    }
                }
            }
        }

        return command.RespondAsync("Sorry, that command is not enabled on this server, meow :sob:", ephemeral: true);
    }
}

