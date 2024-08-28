using System.IO;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using PawsyApp.Utils;
using PawsyApp.GuildStorage;
using System.Text.Json;
using PawsyApp.Settings;
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
            ]),

            //clear local commands to force the list to refresh
            guild.DeleteApplicationCommandsAsync()
        };

        FileInfo file = new(GuildFile.Get(guild.Id));

        tasks.Add(WriteLog.Normally($"Trying to open {file.FullName}"));

        if (file.Exists)
        {
            using StreamReader data = new(file.FullName);
            var newSettings = JsonSerializer.Deserialize<GuildSettings>(data.ReadToEnd());

            if (newSettings is not null)
            {
                AllSettings.GuildSettingsStorage.TryAdd(guild.Id, newSettings);
            }
        }
        else
        {
            AllSettings.GuildSettingsStorage.TryAdd(guild.Id, new(guild.Id));
            AllSettings.SaveAll();
        }

        await Task.WhenAll(tasks);
        return;
    }
}
