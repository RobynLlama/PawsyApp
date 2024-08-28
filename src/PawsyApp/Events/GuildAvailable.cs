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

        // Next, lets create our slash command builder. This is like the embed builder but for slash commands.
        var guildCommand = new SlashCommandBuilder();

        // Note: Names have to be all lowercase and match the regular expression ^[\w-]{3,32}$
        guildCommand.WithName("meow");

        // Descriptions can have a max length of 100.
        guildCommand.WithDescription("Meow meow!");

        tasks.Add(guild.CreateApplicationCommandAsync(guildCommand.Build()));
        tasks.Add(WriteLog.Normally("Registered command for guild"));

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
