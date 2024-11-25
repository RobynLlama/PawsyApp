using System.Collections.Concurrent;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using Discord;
using System.Threading.Tasks;
using PawsyApp.KittyColors;
using PawsyApp.PawsyCore.Modules;

namespace PawsyApp.PawsyCore;

public class Pawsy
{
    protected readonly DiscordSocketClient SocketClient = new(new DiscordSocketConfig
    {
        MessageCacheSize = 100,
        LogLevel = LogSeverity.Info,
        AlwaysDownloadUsers = true,
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMembers & ~GatewayIntents.GuildInvites & ~GatewayIntents.GuildScheduledEvents,
    });
    public ulong ID { get => PawsyID; }
    public string Name { get; } = "Pawsy";
    public static string BaseConfigDir { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Pawsy");

    protected ConcurrentDictionary<ulong, Guild> Guilds = [];
    private readonly string? token = Environment.GetEnvironmentVariable("PAWSY_AUTH");
    private readonly ulong PawsyID;
    private static ulong NextPawsyID = 0;

    public Pawsy()
    {

        PawsyID = GetNextID();

        DirectoryInfo config = new(BaseConfigDir);
        if (!config.Exists)
            config.Create();

        //Client Events
        SocketClient.GuildAvailable += GuildAvailable;
        SocketClient.GuildUnavailable += GuildUnavailable;
        SocketClient.Log += SocketLog;

        //Guild Events
        SocketClient.MessageReceived += MessageReceived;
        SocketClient.MessageUpdated += OnMessageEdit;
        SocketClient.SlashCommandExecuted += OnSlashCommand;
        SocketClient.ThreadCreated += OnThreadCreated;
        SocketClient.ModalSubmitted += OnModalSubmit;
        SocketClient.ButtonExecuted += OnButtonClicked;
        SocketClient.SelectMenuExecuted += OnMenuSelected;

        if (token is not null)
        {
            LogAppendLine(Name, "Logging in with token");
            SocketClient.LoginAsync(TokenType.Bot, token);
            //RestClient.LoginAsync(TokenType.Bot, token);
            SocketClient.StartAsync();
        }
        else
        {
            LogAppendLine(Name, "Token not found");
        }
    }
    private async Task MessageReceived(SocketMessage message)
    {
        //Filter out bots, system and webhook message
        if (message.Author.IsBot || message.Author.IsWebhook || message.Source == MessageSource.System)
            return;

        await LogAppendContext(Name, "Heard Something", [
            ("Author", (message.Author as SocketGuildUser)?.Nickname ?? message.Author.GlobalName ?? message.Author.Username ?? "Nobody"),
            ("CleanContent", message.CleanContent),
            ("Channel", message.Channel.Name),
            ("Guild", (message.Channel as SocketGuildChannel)?.Guild.Name ?? "No Guild"),
            ]);

        //Find which guild it belongs to
        if (message is SocketUserMessage uMessage && message.Channel is SocketTextChannel tChannel)
        {
            if (Guilds.TryGetValue(tChannel.Guild.Id, out var thisGuild))
            {
                await thisGuild.OnMessage(uMessage, tChannel);
            }
        }
    }
    private async Task OnMessageEdit(Cacheable<IMessage, ulong> cacheable, SocketMessage message, ISocketMessageChannel channel)
    {
        //Filter out bots, system and webhook message
        if (message.Author.IsBot || message.Author.IsWebhook || message.Source == MessageSource.System)
            return;

        //Find which guild it belongs to
        if (message is SocketUserMessage uMessage && message.Channel is SocketTextChannel tChannel)
        {
            if (Guilds.TryGetValue(tChannel.Guild.Id, out var thisGuild))
            {
                await thisGuild.OnMessageEdit(cacheable, uMessage, tChannel);
            }
        }
    }

    private async Task OnSlashCommand(SocketSlashCommand command)
    {

        await LogAppendLine(Name, $"Command {command.CommandName}");

        //Meow
        if (command.CommandName == "meow-meow")
        {
            await command.RespondAsync("Meow!", ephemeral: true);
            return;
        }

        //Commands sent in a DM
        if (command.Channel is not SocketTextChannel tChannel)
        {
            await command.RespondAsync("Meow! Please use my commands inside a guild, okay?", ephemeral: true);
            return;
        }

        //Commands sent in a Guild
        if (Guilds.TryGetValue(tChannel.Guild.Id, out var thisGuild))
        {
            await thisGuild.OnSlashCommand(command);
            return;
        }
    }

    private async Task OnThreadCreated(SocketThreadChannel channel)
    {
        //Find which guild it belongs to
        if (channel is SocketTextChannel tChannel)
        {
            if (Guilds.TryGetValue(tChannel.Guild.Id, out var thisGuild))
            {
                await thisGuild.OnThreadCreated(channel);
            }
        }
    }

    private async Task OnModalSubmit(SocketModal modal)
    {
        //Find which guild it belongs to
        if (modal.Channel is SocketTextChannel tChannel)
        {
            if (Guilds.TryGetValue(tChannel.Guild.Id, out var thisGuild))
            {
                await thisGuild.OnModalSubmit(modal);
            }
        }
    }

    private async Task OnButtonClicked(SocketMessageComponent component)
    {
        //Find which guild it belongs to
        if (component.Channel is SocketTextChannel tChannel)
        {
            if (Guilds.TryGetValue(tChannel.Guild.Id, out var thisGuild))
            {
                await thisGuild.OnButtonClicked(component);
            }
        }
    }

    private async Task OnMenuSelected(SocketMessageComponent component)
    {
        //Find which guild it belongs to
        if (component.Channel is SocketTextChannel tChannel)
        {
            if (Guilds.TryGetValue(tChannel.Guild.Id, out var thisGuild))
            {
                await thisGuild.OnMenuSelected(component);
            }
        }
    }

    public Task LogAppendContext(string source, object message, (object ContextName, object ContextValue)[] context) => WriteLogInternal.AppendContext($"Pawsy~{ID}->{source}", message, context);
    public Task LogAppend(string source, object message) => WriteLogInternal.Append($"Pawsy~{ID}->{source}", message);
    public Task LogAppendLine(string source, object message) => LogAppend(source, message + "\n");

    private static class WriteLogInternal
    {
        public static Task AppendContext(string source, object msg, (object ContextName, object ContextValue)[] context)
        {
            StringBuilder sb = new(msg.ToString());
            sb.AppendLine();
            var dump = false;

            foreach (var (ContextName, ContextValue) in context)
            {
                if (ContextName is null)
                {
                    sb.AppendLine(KittyColor.WrapInColor("ContextName is null", ColorCode.Red));
                    dump = true;
                    continue;
                }
                if (ContextValue is null)
                {
                    sb.AppendLine(KittyColor.WrapInColor("ContextValue is null", ColorCode.Red));
                    dump = true;
                    continue;
                }


                sb.Append("  ");
                sb.Append(KittyColor.WrapInColor(ContextName.ToString(), ColorCode.Cyan));
                sb.Append(": ");
                sb.AppendLine(ContextValue.ToString());
            }

            if (dump)
            {
                using StreamWriter writer = new("pawsy-errors.log", true);
                writer.WriteLine(sb);
            }

            sb.AppendLine();
            return Append(source, sb);
        }

        public static Task Append(string source, object msg)
        {
            Console.Write($"[{KittyColor.WrapInColor(source, ColorCode.Magenta)}]  {msg}");
            return Task.CompletedTask;
        }
    }

    private static ulong GetNextID() => ++NextPawsyID;
    private Task SocketLog(LogMessage message)
    {
        Console.WriteLine($"[{KittyColor.WrapInColor("Socket", ColorCode.Yellow)}]  {message}");
        return Task.CompletedTask;
    }

    private async Task GuildAvailable(SocketGuild guild)
    {
        var tasks = new List<Task>
        {
            LogAppendContext(Name, "Guild Available", [
            ("GuildID", guild.Id),
            ("GuildName", guild.Name)
            ]),

            //clear local commands to force the list to refresh
            //guild.DeleteApplicationCommandsAsync()
        };

        var pGuild = AddGuild(guild);
        pGuild.OnActivate();

        await Task.WhenAll(tasks);
        return;
    }
    private async Task GuildUnavailable(SocketGuild guild)
    {
        var tasks = new List<Task>
        {
            LogAppendContext(Name, "Guild Unavailable", [
            ("GuildID", guild.Id),
            ("GuildName", guild.Name)
            ]),
        };

        if (Guilds.TryGetValue(guild.Id, out var thisGuild))
        {
            tasks.Add(LogAppendLine(Name, "Removing Guild hooks"));
            thisGuild.OnDeactivate();
        }

        await Task.WhenAll(tasks);
    }

    protected Guild AddGuild(SocketGuild guild)
    {
        var ID = guild.Id;
        LogAppendLine(Name, $"Creating Guild Instance for {ID}");
        Guild newItem = new(guild, this);
        Guilds[ID] = newItem;
        return newItem;

        throw new Exception($"Unreachable code in AddOrGetGuild. ID: {ID}");
    }
}
