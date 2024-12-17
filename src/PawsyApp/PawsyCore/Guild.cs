using System.IO;
using System.Collections.Concurrent;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;
using System.Linq;
using System.Text;
using System;
using System.Reflection;

namespace PawsyApp.PawsyCore.Modules;

public class Guild : ISettingsOwner, IActivatable
{
    internal static string GetPersistPath(ulong guild)
    {
        return Path.Combine(PawsyCore.Pawsy.BaseConfigDir, guild.ToString());
    }

    public string Name { get; } = "guild-global";
    public string GetSettingsLocation() =>
    Path.Combine(GetPersistPath(ID), $"{Name}.json");
    public ulong ID { get => DiscordGuild.Id; }
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
    protected readonly WeakReference<Pawsy> Pawsy;

    protected readonly ConcurrentDictionary<ulong, SlashCommandBundle> GuildCommands = [];
    protected readonly GuildSettings Settings;
    protected readonly ConcurrentDictionary<string, IGuildModule> Modules = [];

    public Guild(SocketGuild guild, Pawsy pawsy)
    {
        DiscordGuild = guild;
        Pawsy = new(pawsy);

        DirectoryInfo storage = new(Path.Combine(PawsyCore.Pawsy.BaseConfigDir, ID.ToString()));

        if (!storage.Exists)
            storage.Create();

        Settings = (this as ISettingsOwner).LoadSettings<GuildSettings>();

        SetupModules();
    }

    protected void SetupModules()
    {
        var names = Settings.EnabledModules.ToArray();

        foreach (string name in names)
        {
            LoadModuleByName(name);
        }
    }

    protected void LoadModuleByName(string name)
    {
        if (!Pawsy.TryGetTarget(out var owner))
            return;

        if (owner.InstantiateModuleByName(name, [this]) is var module && module is not null)
        {
            try
            {
                if (module.Name is null)
                {
                    LogAppendLine(Name, $"The returned module name is null for {name}!");
                    return;
                }

                LogAppendLine(Name, $"Loading and activating {module}");

                Modules[module.Name] = module;

                LogAppendLine(Name, $"Done with {module.Name}!");
            }
            catch (Exception ex)
            {
                LogAppendLine(Name, $"An error happened while loading {name}\nStack:\n{ex}\n");
            }
        }
        else
        {
            LogAppendLine(Name, $"Failed to load : {name}, removing from active list");
            Settings.EnabledModules.Remove(name);
            (Settings as ISettings).Save<GuildSettings>(this);
        }
    }

    public void OnActivate()
    {
        //BuildGuildCommands();
        BuildGlobalGuildCommands();

        foreach (var item in Modules.Values)
        {
            //this might not be required to check given that if
            //we've instanced a module it must be by user action
            if (Settings.EnabledModules.Contains(item.Name))
            {
                LogAppendLine(Name, $"Activating {item.Name}");
                item.OnActivate();
            }
        }

        Available = true;
    }

    public void OnDeactivate()
    {
        foreach (var item in Modules.Values)
        {
            LogAppendLine(Name, $"Deactivating: {item.Name}");
            item.OnDeactivate();
        }

        Available = false;
    }

    public void Destroy()
    {
        foreach (IGuildModule module in Modules.Values)
        {
            LogAppendLine(Name, $"Destroying: {module.Name}");
            module.Destroy();
        }

        Modules.Clear();
    }

    /// <summary>
    /// This method adds the global commands such as /module-manage /module-config
    /// </summary>
    public void BuildGlobalGuildCommands()
    {
        if (!Pawsy.TryGetTarget(out var owner))
            return;

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

        foreach (var thing in owner.GetRegistry)
        {
            optionAdd.AddChoice(thing, thing);
        }

        foreach (var item in Modules.Values)
        {
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

        }

        RegisterSlashCommand(
            new(ModuleActivator, ModuleCommands.Build(), Name)
        );

        RegisterSlashCommand(
            new(GuildConfigurationEvent, ModuleConfigurationRoot.Build(), Name)
        );
    }

    /// <summary>
    /// This method adds the specific commands from each module
    /// </summary>
    public void BuildGuildCommands()
    {
        if (!Pawsy.TryGetTarget(out var owner))
            return;

        foreach (var item in Modules.Values)
        {
            if (item.ModuleDeclaresCommands)
            {
                var commandRoot = new SlashCommandBuilder()
                .WithName(item.Name)
                .WithDescription($"Run a command from {item.Name}");

                RegisterSlashCommand(item.OnCommandsDeclared(commandRoot));
            }
        }
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

        foreach (var item in Modules.Values)
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

        if (!Pawsy.TryGetTarget(out var owner))
            return;

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

                if (Modules.TryGetValue(modName, out var module))
                {
                    await command.RespondAsync("That module is already active", ephemeral: true);
                    return;
                }

                LoadModuleByName(modName);
                Settings.EnabledModules.Add(modName);
                (Settings as ISettings).Save<GuildSettings>(this);

                if (Modules.TryGetValue(modName, out var newMod))
                {
                    newMod.OnActivate();
                    BuildGuildCommands();
                    BuildGlobalGuildCommands();
                    await command.RespondAsync($"Activating {modName}");
                    return;
                }

                await command.RespondAsync($"Something went wrong activating {modName}");
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

                if (Modules.TryGetValue(modName, out var uItem))
                {
                    uItem.OnDeactivate();
                    Settings.EnabledModules.Remove(modName);
                    (Settings as ISettings).Save<GuildSettings>(this);
                    Modules.TryRemove(modName, out var _);
                    BuildGuildCommands();
                    BuildGlobalGuildCommands();
                    await command.RespondAsync($"{modName} disabled, meow!");
                    return;
                }
                else
                    await command.RespondAsync("Error: module is in the active list but does not exist.", ephemeral: true);
                return;
            case "list":
                StringBuilder sb = new("All Modules:\n");

                foreach (var item in Modules.Values)
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

    protected bool RegisterSlashCommand(SlashCommandBundle bundle)
    {
        try
        {
            var sockCommand = DiscordGuild.CreateApplicationCommandAsync(bundle.BuiltCommand).Result;
            LogAppendLine(Name, $"Registering a command from {bundle.ModuleName}");
            GuildCommands.TryAdd(sockCommand.Id, bundle);
            return true;
        }
        catch (Exception ex)
        {
            LogAppendContext(Name, "Failed to create a guild command", [
                ("Module",bundle.ModuleName),
                ("Entry", bundle.Handler.GetType().Name),
                ("Exception", ex.GetType().Name)
            ]
            );
        }

        return false;
    }

    public override string ToString()
    {
        return $"Guild:({ID})";
    }

    public async Task OnThreadCreated(SocketThreadChannel channel)
    {
        if (!Available)
            return;

        if (OnGuildThreadCreated is not null)
        {
            await OnGuildThreadCreated(channel);
        }
    }
    public async Task OnModalSubmit(SocketModal modal)
    {
        if (!Available)
            return;

        if (OnGuildModalSubmit is not null)
        {
            await OnGuildModalSubmit(modal);
        }
    }
    public async Task OnButtonClicked(SocketMessageComponent component)
    {
        if (!Available)
            return;

        if (OnGuildButtonClicked is not null)
        {
            await OnGuildButtonClicked(component);
        }
    }
    public async Task OnMenuSelected(SocketMessageComponent component)
    {
        if (!Available)
            return;

        if (OnGuildMenuClicked is not null)
        {
            await OnGuildMenuClicked(component);
        }
    }
    public async Task OnSlashCommand(SocketSlashCommand command)
    {
        if (!Available)
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
    public async Task OnMessage(SocketUserMessage message, SocketTextChannel channel)
    {
        if (!Available)
            return;

        if (OnGuildMessage is not null)
        {
            await OnGuildMessage(message, channel);
        }
    }

    public async Task OnMessageEdit(Cacheable<IMessage, ulong> cacheable, SocketUserMessage message, SocketTextChannel channel)
    {
        if (!Available)
            return;

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
        if (!Pawsy.TryGetTarget(out var owner))
            throw new Exception("Unable to access owner in Guild");

        //prepend ourselves to the source
        source = $"{ToString()}->{source}";
        //Pass it upstream
        owner.LogAppendContext(source, message, context);
        return Task.CompletedTask;
    }

    public Task LogAppendLine(string source, object message)
    {
        if (!Pawsy.TryGetTarget(out var owner))
            throw new Exception("Unable to access owner in Guild");

        //prepend ourselves to the source
        source = $"{ToString()}->{source}";
        //Pass it upstream
        owner.LogAppendLine(source, message);
        return Task.CompletedTask;
    }
}
