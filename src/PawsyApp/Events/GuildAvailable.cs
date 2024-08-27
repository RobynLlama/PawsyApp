using System.IO;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using PawsyApp.Utils;
using PawsyApp.GuildStorage;
using System.Text.Json;
using System.Reflection;
using System.Collections.Generic;
using PawsyApp.Settings;

namespace PawsyApp.Events;

internal class GuildAvailable
{
    internal static Task Respond(SocketGuild guild)
    {
        WriteLog.Cutely("Guild Available", [
            ("GuildID", guild.Id),
        ]);

        //clear local commands to force the list to refresh
        guild.DeleteApplicationCommandsAsync();

        // Next, lets create our slash command builder. This is like the embed builder but for slash commands.
        var guildCommand = new SlashCommandBuilder();

        // Note: Names have to be all lowercase and match the regular expression ^[\w-]{3,32}$
        guildCommand.WithName("meow");

        // Descriptions can have a max length of 100.
        guildCommand.WithDescription("Meow meow!");

        guild.CreateApplicationCommandAsync(guildCommand.Build());
        WriteLog.Normally("Registered command for guild");

        if (AllSettings.GuildSettingsStorage.ContainsKey(guild.Id))
            return Task.CompletedTask;

        FileInfo file = new(Path.Combine(Assembly.GetExecutingAssembly().Location.Replace("PawsyApp.dll", ""), "PawsyPersist", $"{guild.Id}.json"));

        WriteLog.Normally($"Trying to open {file.FullName}");

        if (file.Exists)
        {
            using StreamReader data = new(file.FullName);
            var newSettings = JsonSerializer.Deserialize<GuildSettings>(data.ReadToEnd());

            if (newSettings is not null)
            {
                AllSettings.GuildSettingsStorage.Add(guild.Id, newSettings);
            }
        }

        return Task.CompletedTask;
    }
}
