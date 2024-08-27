using System.Threading.Tasks;
using Discord.WebSocket;

namespace PawsyApp.Events.SlashCommands;
internal class SlashCommandHandler
{
    internal static async Task Respond(SocketSlashCommand command)
    {
        await command.RespondAsync($"Meow!");
    }
}
