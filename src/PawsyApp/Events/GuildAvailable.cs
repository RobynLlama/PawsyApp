using Discord.WebSocket;
using System.Threading.Tasks;
using PawsyApp.Utils;
using System.Collections.Generic;
using System.IO;

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

        //Prepare the storage directory
        DirectoryInfo storage = new(Helpers.GetPersistPath(guild.Id));

        if (!storage.Exists)
            storage.Create();

        PawsyProgram.Pawsy.AddGuild(guild.Id);

        await Task.WhenAll(tasks);
        return;
    }
}
