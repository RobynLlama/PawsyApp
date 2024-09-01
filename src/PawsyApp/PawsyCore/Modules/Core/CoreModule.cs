using System.Collections.Concurrent;
using Discord;
using Discord.WebSocket;
using PawsyApp.PawsyCore.Modules.Settings;
using System.Threading.Tasks;

namespace PawsyApp.PawsyCore.Modules.GuildSubmodules;

/// <summary>
/// Core modules are added directly to PawsyApp and do not
/// have any other dependencies
/// </summary>
internal abstract class CoreModule() : IModule
{
    public IModule? Owner { get => _owner; set => _owner = value; }
    public ConcurrentBag<IModule> Modules => _modules;

    public virtual bool ModuleDeclaresConfig => _declaresConfigs;
    public virtual bool ModuleDeclaresCommands => _declaresCommands;
    protected bool _declaresConfigs = false;
    protected bool _declaresCommands = false;

    public abstract string Name { get; }
    public abstract IModuleSettings? Settings { get; }

    public abstract string GetSettingsLocation();

    protected IModule? _owner;
    protected readonly ConcurrentBag<IModule> _modules = [];

    public abstract void Alive();
    public virtual void OnModuleActivation()
    {
        return;
    }
    public virtual void OnModuleDeactivation()
    {
        return;
    }
    public virtual void OnModuleDeclareConfig(SlashCommandOptionBuilder rootConfig)
    {
        return;
    }
    public virtual SlashCommandBundle OnModuleDeclareCommands(SlashCommandBuilder builder)
    {
        throw new System.Exception("OnModuleDeclareCommands called without a matching body in module, panic!");
    }
    public virtual Task OnConfigUpdated(SocketSlashCommand command, SocketSlashCommandDataOption options)
    {
        return command.RespondAsync("This module is not configurable or unavailable", ephemeral: true); ;
    }
}