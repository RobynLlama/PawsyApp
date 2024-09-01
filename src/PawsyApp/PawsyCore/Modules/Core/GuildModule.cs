using System.IO;
using PawsyApp.PawsyCore.Modules.Settings;
using PawsyApp.Utils;
using PawsyApp.PawsyCore.Modules.GuildSubmodules;
using System.Collections.Concurrent;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;
using System.Linq;
using System.Text;

namespace PawsyApp.PawsyCore.Modules.Core;

internal class GuildModule() : CoreModule, IModuleIdent
{
    public override IModule? Owner { get => _owner; set => _owner = value; }
    public override ConcurrentBag<IModule> Modules => _modules;
    public override string Name { get => "guild-global"; }
    public ulong ID { get => _id; set => _id = value; }
    public override IModuleSettings? Settings => Settings;
    public override string GetSettingsLocation() =>
    Path.Combine(Helpers.GetPersistPath(ID), $"{Name}.json");

    protected ulong _id;
    protected IModule? _owner;
    protected readonly ConcurrentBag<IModule> _modules = [];
    protected readonly ConcurrentDictionary<ulong, SlashCommandBundle> GuildCommands = [];
    protected GuildSettings? _settings;

    public delegate Task GuildMessageHandler(SocketUserMessage message, SocketGuildChannel channel);
    public delegate Task GuildThreadCreatedHandler(SocketThreadChannel channel);
    public event GuildMessageHandler? OnGuildMessage;
    public event GuildMessageHandler? OnGuildMessageEdit;
    public event GuildThreadCreatedHandler? OnGuildThreadCreated;

    public override void Alive()
    {
        //Decide if we should activate modules here
        if (this is IModule module)
        {
            _settings = module.LoadSettings<GuildSettings>();
            module.AddModule<MeowBoardModule>();
            module.AddModule<FilterMatcherModule>();
            module.AddModule<LogMuncherModule>();
            module.AddModule<ModderRoleCheckerModule>();
        }

        var ModuleCommands = new SlashCommandBuilder()
        .WithName("module")
        .WithDefaultMemberPermissions(GuildPermission.ManageGuild)
        .WithDescription("Manage your activated modules")
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName("activate")
            .WithDescription("Enables a module for your guild")
            .AddOption("name", ApplicationCommandOptionType.String, "the name of the module to activate", isRequired: true)
        )
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName("deactivate")
            .WithDescription("Disables a module for your guild")
            .AddOption("name", ApplicationCommandOptionType.String, "the name of the module to deactivate", isRequired: true)
        )
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName("list")
            .WithDescription("Lists all available modules")
        );

        RegisterSlashCommand(
            new(ModuleActivator, ModuleCommands.Build(), Name)
        );

        //Subscribe to events
        PawsyProgram.SocketClient.MessageReceived += OnMessage;
        PawsyProgram.SocketClient.MessageUpdated += OnMessageEdit;
        PawsyProgram.SocketClient.SlashCommandExecuted += OnSlashCommand;
        PawsyProgram.SocketClient.ThreadCreated += OnThreadCreated;

        foreach (var item in Modules)
        {
            if (_settings is not null && _settings.EnabledModules.Contains(item.Name))
            {
                WriteLog.LineNormal($"Activating {item.Name}");
                item.OnModuleActivation();
            }
        }
    }
    void IModule.OnModuleActivation()
    {
        return;
    }
    void IModule.OnModuleDeactivation()
    {
        //I can't think of any reasons a Guild would be deactivated
        return;
    }

    void IModule.OnModuleDeclareConfig(SlashCommandBuilder rootConfig)
    {
        return;
    }

    void IModule.OnConfigUpdated(SocketSlashCommand command)
    {
        return;
    }

    private async Task ModuleActivator(SocketSlashCommand command)
    {

        if (_settings is null)
        {
            await command.RespondAsync("This guild's config is unavailable, something really went wrong.", ephemeral: true);
            return;
        }

        //Activation has been called

        var subCommand = command.Data.Options.First();
        var activationType = subCommand.Name;
        string modName;

        switch (activationType)
        {
            case "activate":
                modName = subCommand.Options.First().Value.ToString() ?? string.Empty;
                await WriteLog.LineNormal($"Activate a module {modName}");
                //Already active?
                if (_settings.EnabledModules.Contains(modName))
                {
                    await command.RespondAsync("That module is already active", ephemeral: true);
                    return;
                }

                bool EnabledAnything = false;

                foreach (var item in Modules)
                {
                    if (item.Name == modName)
                    {
                        EnabledAnything = true;
                        item.OnModuleActivation();
                        _settings.EnabledModules.Add(modName);
                        (_settings as IModuleSettings).Save<GuildSettings>();
                        await command.RespondAsync($"{modName} enabled, meow!");
                    }
                }

                if (!EnabledAnything)
                    await command.RespondAsync("Sorry, meow! I couldn't find that module (and I looked really hard, too)", ephemeral: true);

                return;
            case "deactivate":
                modName = subCommand.Options.First().Value.ToString() ?? string.Empty;
                await WriteLog.LineNormal($"Deactivate a module {modName}");

                //Check if its active
                if (!_settings.EnabledModules.Contains(modName))
                {
                    await command.RespondAsync("That module is not active", ephemeral: true);
                    return;
                }

                foreach (var item in Modules)
                {
                    if (item.Name == modName)
                    {
                        item.OnModuleDeactivation();
                        _settings.EnabledModules.Remove(modName);
                        (_settings as IModuleSettings).Save<GuildSettings>();
                        await command.RespondAsync($"{modName} disabled, meow!");
                        return;
                    }
                }

                await command.RespondAsync("Error: module is in the active list but does not exist.", ephemeral: true);
                return;
            case "list":
                StringBuilder sb = new("All Modules:\n");

                foreach (var item in _modules)
                {
                    var enabled = _settings.EnabledModules.Contains(item.Name);

                    sb.Append(item.Name);
                    sb.Append(": [");
                    sb.AppendLine(enabled ? "Enabled]" : "Disabled]");
                }

                await command.RespondAsync(sb.ToString(), ephemeral: true);

                return;
            default:
                await WriteLog.LineNormal("Something went very wrong in HandleActivation");
                break;
        }

        await command.RespondAsync("Error: Something went really wrong!", ephemeral: true);
        return;
    }

    public async void RegisterSlashCommand(SlashCommandBundle bundle)
    {
        await WriteLog.LineNormal("Registering a command");
        var sockCommand = await PawsyProgram.SocketClient.GetGuild(ID).CreateApplicationCommandAsync(bundle.BuiltCommand);
        //var restCommand = await PawsyProgram.RestClient.CreateGuildCommand(bundle.BuiltCommand, ID);
        GuildCommands.TryAdd(sockCommand.Id, bundle);
    }
    private async Task OnThreadCreated(SocketThreadChannel channel)
    {
        if (channel.Guild.Id != ID)
            return;

        if (OnGuildThreadCreated is not null)
        {
            await OnGuildThreadCreated(channel);
        }
    }
    private async Task OnSlashCommand(SocketSlashCommand command)
    {
        if (command.GuildId is not ulong guild)
            return;

        if (guild != ID)
            return;

        if (_settings is null || !GuildCommands.TryGetValue(command.CommandId, out SlashCommandBundle bundle))
        {
            await command.RespondAsync("Sowwy, meow. That command is not available", ephemeral: true);
            return;
        }

        if (bundle.ModuleName == "guild-global" || _settings.EnabledModules.Contains(bundle.ModuleName))
        {
            await bundle.Handler(command);
            return;
        }

        await command.RespondAsync($"Sowwy, meow :sob: The {bundle.ModuleName} module is disabled on this guild.", ephemeral: true);
    }
    private async Task OnMessage(SocketMessage message)
    {
        //Filter out bots, system and webhook message
        if (message.Author.IsBot || message.Author.IsWebhook || message.Source == MessageSource.System)
            return;

        if (message is SocketUserMessage uMessage)
        {
            //Guild messages
            if (OnGuildMessage is not null && message.Channel is SocketGuildChannel gChannel && gChannel.Guild.Id == ID)
                await OnGuildMessage(uMessage, gChannel);
        }
    }

    private async Task OnMessageEdit(Cacheable<IMessage, ulong> cacheable, SocketMessage message, ISocketMessageChannel channel)
    {
        //Filter out bots, system and webhook message
        if (message.Author.IsBot || message.Author.IsWebhook || message.Source == MessageSource.System)
            return;

        if (message is SocketUserMessage uMessage)
        {
            //Guild messages
            if (OnGuildMessageEdit is not null && message.Channel is SocketGuildChannel gChannel && gChannel.Guild.Id == ID)
            {
                /*
                await WriteLog.Cutely("Pawsy heard an update", [
                ("CacheID", cacheable.Id),
                ("Cached", cacheable.HasValue),
                ("Author", uMessage.Author),
                ("Channel", gChannel.Guild.Name),
                ]);
                */

                await OnGuildMessageEdit(uMessage, gChannel);
            }
        }
    }
}
