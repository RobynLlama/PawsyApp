using Discord.WebSocket;
using System.Threading.Tasks;
using PawsyApp.Utils;
using System.Collections.Generic;

namespace PawsyApp.Events;

internal class GuildAvailable
{
    internal static async Task Respond(SocketGuild guild)
    {

        var tasks = new List<Task>
        {
            WriteLog.Cutely("Guild Available", [
            ("GuildID", guild.Id),
            ("GuildName", guild.Name)
            ]),

            //clear local commands to force the list to refresh
            //guild.DeleteApplicationCommandsAsync()
        };

        PawsyProgram.Pawsy.AddOrGetGuild(guild.Id);

        await Task.WhenAll(tasks);
        return;
    }
}
