using Discord.WebSocket;
using System.Threading.Tasks;
using PawsyApp.Utils;
using System.Collections.Generic;
using PawsyApp.Events.SlashCommands;
using PawsyApp.PawsyCore.Modules;
using PawsyApp.PawsyCore.Modules.Core;

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

        (PawsyProgram.Pawsy as IModuleIdent).AddModuleIdent<GuildModule>(guild.Id);

        GlobalTaskRunner.FireAndForget(SlashCommandHandler.AddCommandsToGuild(guild));

        await Task.WhenAll(tasks);
        return;
    }
}
