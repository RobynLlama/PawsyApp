using Discord;

namespace PawsyApp.Events.SlashCommands;

internal interface ISlashCommand
{
    SlashCommandHandler.SlashHandler Handler { get; }
    SlashCommandProperties BuiltCommand { get; }
}

