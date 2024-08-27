using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PawsyApp.Events;
using PawsyApp.GuildStorage;
using PawsyApp.Utils;
using System.Text.Json;
using System.IO;
using System.Reflection;

namespace PawsyApp;
public class PawsyProgram
{
    internal static DiscordSocketClient? _client;
    internal static Dictionary<ulong, GuildSettings> SettingsStorage = [];

    public static async Task Main()
    {
        //Setup connection client
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            MessageCacheSize = 100,
            LogLevel = LogSeverity.Info,
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
        });

        _client.Log += LogEvent.Handler;
        _client.MessageReceived += MessageEvent.Handler;
        _client.Ready += ClientReady;
        _client.GuildAvailable += GuildAvailable;
        _client.SlashCommandExecuted += SlashCommandHandler;

        //Get token from env
        String? token = Environment.GetEnvironmentVariable("PAWSY_AUTH");

        if (token is not null)
        {
            Console.WriteLine("Logging in with token");
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
        else
        {
            Console.WriteLine("Token not found");
            return;
        }

    }

    private static async Task SlashCommandHandler(SocketSlashCommand command)
    {
        await command.RespondAsync($"Meow!");
    }

    private static Task GuildAvailable(SocketGuild guild)
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

        if (SettingsStorage.ContainsKey(guild.Id))
            return Task.CompletedTask;

        FileInfo file = new(Path.Combine(Assembly.GetExecutingAssembly().Location.Replace("PawsyApp.dll", ""), "PawsyPersist", $"{guild.Id}.json"));

        WriteLog.Normally($"Trying to open {file.FullName}");

        if (file.Exists)
        {
            using (StreamReader data = new(file.FullName))
            {
                var newSettings = JsonSerializer.Deserialize<GuildSettings>(data.ReadToEnd());

                if (newSettings is not null)
                {
                    SettingsStorage.Add(guild.Id, newSettings);
                }
            }
        }

        return Task.CompletedTask;
    }

    private static Task ClientReady()
    {
        WriteLog.Normally("I'm awake!");

        //Kills all our global commands cuz I added a bunch on accident
        _client?.BulkOverwriteGlobalApplicationCommandsAsync([]);

        return Task.CompletedTask;
    }
}
