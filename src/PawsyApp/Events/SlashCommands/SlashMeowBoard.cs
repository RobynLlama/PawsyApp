using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PawsyApp.Utils;

namespace PawsyApp.Events.SlashCommands;

internal class SlashMeowBoard : ISlashCommand
{
    public SlashCommandHandler.SlashHandler Handler => HandleCommand;

    private static Task HandleCommand(SocketSlashCommand command)
    {
        /*if (Helpers.GetSettings(command.GuildId) is GuildSettings settings)
            return settings.MeowBoard.EmbedMeowBoard(command, settings);*/

        command.RespondAsync("Something went wrong", ephemeral: true);

        return Task.CompletedTask;
    }

    public SlashCommandProperties BuiltCommand => MyCommand;

    public string ModuleName => "fun";

    private static SlashCommandProperties MyCommand = new SlashCommandBuilder().WithName("meowboard").WithDescription("View the MeowBoard").Build();

}
