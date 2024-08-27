using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PawsyApp.Events;
using PawsyApp.Events.SlashCommands;

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

        _client.Log += LogEvent.Respond;
        _client.MessageReceived += MessageEvent.Respond;
        _client.Ready += ClientReady.Respond;
        _client.GuildAvailable += GuildAvailable.Respond;
        _client.SlashCommandExecuted += SlashCommandHandler.Respond;

        //Get token from env
        String? token = Environment.GetEnvironmentVariable("PAWSY_AUTH");

        if (token is not null)
        {
            Console.WriteLine("Logging in with token");
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            while (true)
            {
                await Task.Delay(250);
            }
        }
        else
        {
            Console.WriteLine("Token not found");
            return;
        }

    }
}
