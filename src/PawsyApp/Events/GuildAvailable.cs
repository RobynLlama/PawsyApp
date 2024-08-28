using System.IO;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using PawsyApp.Utils;
using PawsyApp.GuildStorage;
using System.Text.Json;
using PawsyApp.Settings;
using System.Collections.Generic;
using PawsyApp.Events.SlashCommands;

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
        gSettings.Save();

        //Iterate all modules
        foreach (var item in gSettings.EnabledModules.Keys)
        {
            if (gSettings.EnabledModules[item])
            {
                //module is enabled
                tasks.Add(WriteLog.Cutely("Enabling a module", [
                    ("Module", item)
                ]));

                tasks.Add(SlashCommandHandler.AddCommandsToGuild(guild));
            }
        }

        await Task.WhenAll(tasks);
        return;
    }
}
