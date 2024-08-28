using System.IO;
using Discord.WebSocket;
using System.Threading.Tasks;
using PawsyApp.Utils;
using PawsyApp.GuildStorage;
using System.Text.Json;
using PawsyApp.Settings;
using System.Collections.Generic;
using PawsyApp.Events.SlashCommands;
using System.Linq;

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

        FileInfo file = new(GuildFile.Get(guild.Id));

        tasks.Add(WriteLog.Normally($"Reading settings for guild"));
        GuildSettings? gSettings = null;

        if (file.Exists)
        {
            using StreamReader data = new(file.FullName);
            gSettings = JsonSerializer.Deserialize<GuildSettings>(data.ReadToEnd());
        }

        gSettings ??= new(guild.Id, guild.Name);
        AllSettings.GuildSettingsStorage.TryAdd(guild.Id, gSettings);

        tasks.Add(gSettings.Save());

        var settings = gSettings.EnabledModules.ToDictionary();

        //Iterate all modules
        foreach (var item in settings)
        {
            if (item.Value)
            {
                //module is enabled
                GlobalTaskRunner.FireAndForget(WriteLog.Cutely("Enabling a module", [
                    ("Module", item)
                ]));
            }
        }

        GlobalTaskRunner.FireAndForget(SlashCommandHandler.AddCommandsToGuild(guild));

        await Task.WhenAll(tasks);
        return;
    }
}
