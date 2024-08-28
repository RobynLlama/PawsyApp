using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace PawsyApp.Events.SlashCommands;

internal class SlashMeow : ISlashCommand
{
    public SlashCommandHandler.SlashHandler Handler => HandleCommand;

    private static Task HandleCommand(SocketSlashCommand command)
    {
        return command.RespondAsync($"Meow!");
    }

    public SlashCommandProperties BuiltCommand => MyCommand;

    private static SlashCommandProperties MyCommand = new SlashCommandBuilder().WithName("meow").WithDescription("Pawsy will meow for you").Build();

}
