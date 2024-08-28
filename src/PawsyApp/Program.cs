using OpenAI_API;
using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PawsyApp.Events;
using PawsyApp.Events.SlashCommands;
using Discord.Rest;

namespace PawsyApp;
public class PawsyProgram
{
    internal static DiscordSocketClient SocketClient = new(new DiscordSocketConfig
    {
        MessageCacheSize = 100,
        LogLevel = LogSeverity.Info,
        AlwaysDownloadUsers = true,
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMembers & ~GatewayIntents.GuildInvites & ~GatewayIntents.GuildScheduledEvents,
    });

    internal static DiscordRestClient RestClient = new(new DiscordRestConfig
    {
        LogLevel = LogSeverity.Info
    });
    internal static OpenAIAPI api = new(Environment.GetEnvironmentVariable("OPENAI_AUTH"));

    public static async Task Main()
    {
        SlashCommandHandler.RegisterAllModules();

        SocketClient.Log += LogEvent.SocketRespond;
        SocketClient.MessageReceived += MessageEvent.Respond;
        SocketClient.Ready += ClientReady.Respond;
        SocketClient.GuildAvailable += GuildAvailable.Respond;
        SocketClient.SlashCommandExecuted += SlashCommandHandler.Respond;
        SocketClient.MessageUpdated += MessageUpdatedEvent.Respond;
        RestClient.Log += LogEvent.RestRespond;

        //Get token from env
        String? token = Environment.GetEnvironmentVariable("PAWSY_AUTH");

        if (token is not null)
        {
            Console.WriteLine("Logging in with token");
            await SocketClient.LoginAsync(TokenType.Bot, token);
            await RestClient.LoginAsync(TokenType.Bot, token);
            await SocketClient.StartAsync();

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
