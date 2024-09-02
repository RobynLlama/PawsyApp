using System.IO;
using PawsyApp.PawsyCore.Modules.Settings;
using PawsyApp.Utils;
using System.Collections.Concurrent;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;
using System.Linq;
using System.Text;
using PawsyApp.PawsyCore.Modules;
using PawsyApp.PawsyCore.Modules.GuildModules;
using System.Collections.Generic;

namespace PawsyApp.PawsyCore;

internal class Guild : IUnique<ulong>, ISettingsOwner, IUniqueCollection<string>
{
    internal static string GetPersistPath(ulong guild)
    {
        return Path.Combine(Pawsy.BaseConfigDir, guild.ToString());
    }

    public string Name { get; } = "guild-global";
    public string GetSettingsLocation() =>
    Path.Combine(GetPersistPath(ID), $"{Name}.json");
    public ulong ID { get; }
    public IEnumerable<IUnique<string>> UniqueCollection => Modules;

    public delegate Task GuildMessageHandler(SocketUserMessage message, SocketGuildChannel channel);
    public delegate Task GuildThreadCreatedHandler(SocketThreadChannel channel);
    public event GuildMessageHandler? OnGuildMessage;
    public event GuildMessageHandler? OnGuildMessageEdit;
    public event GuildThreadCreatedHandler? OnGuildThreadCreated;

    protected readonly ConcurrentDictionary<ulong, SlashCommandBundle> GuildCommands = [];
    protected readonly GuildSettings Settings;
    protected readonly ConcurrentBag<IGuildModule> Modules = [];

    public Guild(ulong ID)
    {
        this.ID = ID;

        DirectoryInfo storage = new(Path.Combine(Pawsy.BaseConfigDir, ID.ToString()));

        if (!storage.Exists)
            storage.Create();

        Settings = (this as ISettingsOwner).LoadSettings<GuildSettings>();

        Modules.Add(new MeowBoardModule(this));
        Modules.Add(new LogMuncherModule(this));
        Modules.Add(new FilterMatcherModule(this));
        Modules.Add(new ModderRoleCheckerModule(this));

        GuildSetup();
    }

    public void GuildSetup()
    {
        var ModuleCommands = new SlashCommandBuilder()
        .WithName("module-manage")
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

        //Module configuration command
        var ModuleConfigurationRoot = new SlashCommandBuilder()
        .WithName("module-config")
        .WithDescription("Configure Pawsy's modules")
        .WithDefaultMemberPermissions(GuildPermission.ManageGuild);

        //Subscribe to events
        PawsyProgram.SocketClient.MessageReceived += OnMessage;
        PawsyProgram.SocketClient.MessageUpdated += OnMessageEdit;
        PawsyProgram.SocketClient.SlashCommandExecuted += OnSlashCommand;
        PawsyProgram.SocketClient.ThreadCreated += OnThreadCreated;

        foreach (var item in Modules)
        {

            //add a config option for each module
            if (item.ModuleDeclaresConfig)
            {
                var configRoot = new SlashCommandOptionBuilder()
                .WithName(item.Name)
                .WithDescription($"Configure the {item.Name} module")
                .WithType(ApplicationCommandOptionType.SubCommand);

                item.OnModuleDeclareConfig(configRoot);
                ModuleConfigurationRoot.AddOption(configRoot);
            }

            if (item.ModuleDeclaresCommands)
            {
                var commandRoot = new SlashCommandBuilder()
                .WithName(item.Name)
                .WithDescription($"Run a command from {item.Name}");

                RegisterSlashCommand(item.OnModuleDeclareCommands(commandRoot));
            }

            if (Settings is not null && Settings.EnabledModules.Contains(item.Name))
            {
                WriteLog.LineNormal($"Activating {item.Name}");
                item.OnModuleActivation();
            }
        }

        RegisterSlashCommand(
            new(GuildConfigurationEvent, ModuleConfigurationRoot.Build(), Name)
        );
    }

    private async Task GuildConfigurationEvent(SocketSlashCommand command)
    {
        var configOptions = command.Data.Options.First();
        var modName = configOptions.Name;

        await WriteLog.LineNormal($"Config command for {modName}");

        if (Settings is null)
        {
            await command.RespondAsync("Settings are null in Configuration Event", ephemeral: true);
            return;
        }

        if (!Settings.EnabledModules.Contains(modName))
        {
            await command.RespondAsync("That module is disabled", ephemeral: true);
            return;
        }

        IGuildModule? module = null;

        foreach (var item in Modules)
        {
            if (item.Name == modName)
            {
                module = item;
                break;
            }
        }

        if (module is null)
        {
            await command.RespondAsync($"Error, unable to locate module {modName}");
            return;
        }

        await module.OnConfigUpdated(command, configOptions);
    }

    private async Task ModuleActivator(SocketSlashCommand command)
    {

        if (Settings is null)
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
                if (Settings.EnabledModules.Contains(modName))
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
                        Settings.EnabledModules.Add(modName);
                        (Settings as ISettings).Save<GuildSettings>(this);
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
                if (!Settings.EnabledModules.Contains(modName))
                {
                    await command.RespondAsync("That module is not active", ephemeral: true);
                    return;
                }

                foreach (var item in Modules)
                {
                    if (item.Name == modName)
                    {
                        item.OnModuleDeactivation();
                        Settings.EnabledModules.Remove(modName);
                        (Settings as ISettings).Save<GuildSettings>(this);
                        await command.RespondAsync($"{modName} disabled, meow!");
                        return;
                    }
                }

                await command.RespondAsync("Error: module is in the active list but does not exist.", ephemeral: true);
                return;
            case "list":
                StringBuilder sb = new("All Modules:\n");

                foreach (var item in Modules)
                {
                    var enabled = Settings.EnabledModules.Contains(item.Name);

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

        if (Settings is null || !GuildCommands.TryGetValue(command.CommandId, out SlashCommandBundle bundle))
        {
            await command.RespondAsync("Sowwy, meow. That command is not available", ephemeral: true);
            return;
        }

        if (bundle.ModuleName == "guild-global" || Settings.EnabledModules.Contains(bundle.ModuleName))
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
