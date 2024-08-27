using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PawsyApp.Events;
using PawsyApp.Events.SlashCommands;
using PawsyApp.GuildStorage;
using PawsyApp.Utils;

namespace PawsyApp;
public class PawsyProgram
{
    internal static DiscordSocketClient? _client;

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
        _client.MessageReceived += MessageEvent.Respond;
        _client.Ready += ClientReady;
        _client.GuildAvailable += GuildAvailable.Respond;
        _client.SlashCommandExecuted += SlashCommandHandler.Respond;

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

    private static Task ClientReady()
    {
        WriteLog.Normally("I'm awake!");

        //Kills all our global commands cuz I added a bunch on accident
        _client?.BulkOverwriteGlobalApplicationCommandsAsync([]);

        return Task.CompletedTask;
    }
}
