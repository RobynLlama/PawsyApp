using System.IO;
using PawsyApp.PawsyCore.Modules.Settings;
using System.Collections.Concurrent;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;
using System.Linq;
using System.Text;
using PawsyApp.PawsyCore.Modules.GuildModules;
using System.Collections.Generic;

namespace PawsyApp.PawsyCore.Modules;

internal class Guild : IUnique<ulong>, ISettingsOwner, IUniqueCollection<string, IGuildModule>, IActivatable
{
    internal static string GetPersistPath(ulong guild)
    {
        return Path.Combine(Pawsy.BaseConfigDir, guild.ToString());
    }

    public string Name { get; } = "guild-global";
    public string GetSettingsLocation() =>
    Path.Combine(GetPersistPath(ID), $"{Name}.json");
    public ulong ID { get => DiscordGuild.Id; }
    public IEnumerable<IGuildModule> UniqueCollection => Modules;
    public bool Available = false;

    public SocketGuild DiscordGuild;
    public delegate Task GuildMessageHandler(SocketUserMessage message, SocketGuildChannel channel);
    public delegate Task GuildThreadCreatedHandler(SocketThreadChannel channel);
    public delegate Task GuildModalHandler(SocketModal modal);
    public delegate Task GuildButtonHandler(SocketMessageComponent component);
    public delegate Task GuildMenuHandler(SocketMessageComponent component);
    public event GuildMessageHandler? OnGuildMessage;
    public event GuildMessageHandler? OnGuildMessageEdit;
    public event GuildThreadCreatedHandler? OnGuildThreadCreated;
    public event GuildModalHandler? OnGuildModalSubmit;
    public event GuildButtonHandler? OnGuildButtonClicked;
    public event GuildMenuHandler? OnGuildMenuClicked;
    public readonly Pawsy Pawsy;

    protected readonly ConcurrentDictionary<ulong, SlashCommandBundle> GuildCommands = [];
    protected readonly GuildSettings Settings;
    protected readonly ConcurrentBag<IGuildModule> Modules = [];

    public Guild(SocketGuild guild, Pawsy pawsy)
    {
        DiscordGuild = guild;
        Pawsy = pawsy;

        DirectoryInfo storage = new(Path.Combine(Pawsy.BaseConfigDir, ID.ToString()));

        if (!storage.Exists)
            storage.Create();

        Settings = (this as ISettingsOwner).LoadSettings<GuildSettings>();

        Modules.Add(new MeowBoardModule(this));
        Modules.Add(new LogMuncherModule(this));
        Modules.Add(new FilterMatcherModule(this));
        Modules.Add(new ModderRoleCheckerModule(this));
    }

    public void OnActivate()
    {
        /*
        Pawsy.SocketClient.MessageReceived += OnMessage;
        Pawsy.SocketClient.MessageUpdated += OnMessageEdit;
        Pawsy.SocketClient.SlashCommandExecuted += OnSlashCommand;
        Pawsy.SocketClient.ThreadCreated += OnThreadCreated;
        Pawsy.SocketClient.ModalSubmitted += OnModalSubmit;
        Pawsy.SocketClient.ButtonExecuted += OnButtonClicked;
        Pawsy.SocketClient.SelectMenuExecuted += OnMenuSelected;
        */

        GuildCommandSetup();
        Available = true;
    }

    public void OnDeactivate()
    {
        /*
        Pawsy.SocketClient.MessageReceived -= OnMessage;
        Pawsy.SocketClient.MessageUpdated -= OnMessageEdit;
        Pawsy.SocketClient.SlashCommandExecuted -= OnSlashCommand;
        Pawsy.SocketClient.ThreadCreated -= OnThreadCreated;
        Pawsy.SocketClient.ModalSubmitted -= OnModalSubmit;
        Pawsy.SocketClient.ButtonExecuted -= OnButtonClicked;
        Pawsy.SocketClient.SelectMenuExecuted -= OnMenuSelected;
        */
        Available = false;
    }

    public void GuildCommandSetup()
    {
        var optionAdd = new SlashCommandOptionBuilder()
        .WithName("name")
        .WithType(ApplicationCommandOptionType.String)
        .WithDescription("the name of the module to activate")
        .WithRequired(true);

        var optionRemove = new SlashCommandOptionBuilder()
        .WithName("name")
        .WithType(ApplicationCommandOptionType.String)
        .WithDescription("the name of the module to deactivate")
        .WithRequired(true);


        var ModuleCommands = new SlashCommandBuilder()
        .WithName("module-manage")
        .WithDefaultMemberPermissions(GuildPermission.ManageGuild)
        .WithDescription("Manage your activated modules")
        .AddOption(new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName("activate")
            .WithDescription("Enables a module for your guild")
            .AddOption(optionAdd)
            )
        .AddOption(new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName("deactivate")
            .WithDescription("Disables a module for your guild")
            .AddOption(optionRemove)
            )
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName("list")
            .WithDescription("Lists all available modules")
        );

        //Module configuration command
        var ModuleConfigurationRoot = new SlashCommandBuilder()
        .WithName("module-config")
        .WithDescription("Configure Pawsy's modules")
        .WithDefaultMemberPermissions(GuildPermission.ManageGuild);

        foreach (var item in Modules)
        {

            optionAdd.AddChoice(item.Name, item.Name);
            optionRemove.AddChoice(item.Name, item.Name);

            //add a config option for each module
            if (item.ModuleDeclaresConfig)
            {
                var configRoot = new SlashCommandOptionBuilder()
                .WithName(item.Name)
                .WithDescription($"Configure the {item.Name} module")
                .WithType(ApplicationCommandOptionType.SubCommand);

                item.OnConfigDeclared(configRoot);
                ModuleConfigurationRoot.AddOption(configRoot);
            }

            if (item.ModuleDeclaresCommands)
            {
                var commandRoot = new SlashCommandBuilder()
                .WithName(item.Name)
                .WithDescription($"Run a command from {item.Name}");

                RegisterSlashCommand(item.OnCommandsDeclared(commandRoot));
            }

            if (Settings is not null && Settings.EnabledModules.Contains(item.Name))
            {
                LogAppendLine(Name, $"Activating {item.Name}");
                item.OnActivate();
            }
        }

        RegisterSlashCommand(
            new(ModuleActivator, ModuleCommands.Build(), Name)
        );

        RegisterSlashCommand(
            new(GuildConfigurationEvent, ModuleConfigurationRoot.Build(), Name)
        );
    }

    private async Task GuildConfigurationEvent(SocketSlashCommand command)
    {
        var configOptions = command.Data.Options.First();
        var modName = configOptions.Name;

        await LogAppendLine(Name, $"Config command for {modName}");

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
                await LogAppendLine(Name, $"Activate a module {modName}");
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
                        item.OnActivate();
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
                await LogAppendLine(Name, $"Deactivate a module {modName}");

                //Check if its active
                if (!Settings.EnabledModules.Contains(modName))
                {
                    await command.RespondAsync("That module is not active", ephemeral: true);
                    return;
                }

                var uItem = (this as IUniqueCollection<string, IGuildModule>).GetUniqueItem(modName);

                if (uItem is not null)
                    if (uItem.Name == modName)
                    {
                        uItem.OnDeactivate();
                        Settings.EnabledModules.Remove(modName);
                        (Settings as ISettings).Save<GuildSettings>(this);
                        await command.RespondAsync($"{modName} disabled, meow!");
                        return;
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
                await LogAppendLine(Name, "Something went very wrong in HandleActivation");
                break;
        }

        await command.RespondAsync("Error: Something went really wrong!", ephemeral: true);
        return;
    }

    protected async void RegisterSlashCommand(SlashCommandBundle bundle)
    {
        await LogAppendLine(Name, $"Registering a command from {bundle.ModuleName}");
        var sockCommand = await DiscordGuild.CreateApplicationCommandAsync(bundle.BuiltCommand);
        //var restCommand = await PawsyProgram.RestClient.CreateGuildCommand(bundle.BuiltCommand, ID);
        GuildCommands.TryAdd(sockCommand.Id, bundle);
    }

    public override string ToString()
    {
        return $"Guild:({ID})";
    }

    public async Task OnThreadCreated(SocketThreadChannel channel)
    {
        if (OnGuildThreadCreated is not null)
        {
            await OnGuildThreadCreated(channel);
        }
    }
    public async Task OnModalSubmit(SocketModal modal)
    {
        if (modal.GuildId != DiscordGuild.Id)
            return;

        if (OnGuildModalSubmit is not null)
        {
            await OnGuildModalSubmit(modal);
        }
    }
    public async Task OnButtonClicked(SocketMessageComponent component)
    {
        if (component.GuildId != DiscordGuild.Id)
            return;

        if (OnGuildButtonClicked is not null)
        {
            await OnGuildButtonClicked(component);
        }
    }
    public async Task OnMenuSelected(SocketMessageComponent component)
    {
        if (component.GuildId != DiscordGuild.Id)
            return;

        if (OnGuildMenuClicked is not null)
        {
            await OnGuildMenuClicked(component);
        }
    }
    public async Task OnSlashCommand(SocketSlashCommand command)
    {
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
    public async Task OnMessage(SocketUserMessage message, SocketTextChannel channel)
    {
        if (OnGuildMessage is not null)
        {
            await OnGuildMessage(message, channel);
        }
    }

    public async Task OnMessageEdit(Cacheable<IMessage, ulong> cacheable, SocketUserMessage message, SocketTextChannel channel)
    {
        //Guild messages
        if (OnGuildMessageEdit is not null)
        {
            /*
            await WriteLog.Cutely("Pawsy heard an update", [
            ("CacheID", cacheable.Id),
            ("Cached", cacheable.HasValue),
            ("Author", uMessage.Author),
            ("Channel", gChannel.Guild.Name),
            ]);
            */

            await OnGuildMessageEdit(message, channel);
        }
    }

    public Task LogAppendContext(string source, object message, (object ContextName, object ContextValue)[] context)
    {
        //prepend ourselves to the source
        source = $"{ToString()}->{source}";
        //Pass it upstream
        Pawsy.LogAppendContext(source, message, context);
        return Task.CompletedTask;
    }

    public Task LogAppendLine(string source, object message)
    {
        //prepend ourselves to the source
        source = $"{ToString()}->{source}";
        //Pass it upstream
        Pawsy.LogAppendLine(source, message);
        return Task.CompletedTask;
    }
}
