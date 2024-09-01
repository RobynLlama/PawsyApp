using System.Collections.Concurrent;
using Discord;
using Discord.WebSocket;
using PawsyApp.PawsyCore.Modules.Settings;

namespace PawsyApp.PawsyCore.Modules.GuildSubmodules;

/// <summary>
/// Core modules are added directly to PawsyApp and do not
/// have any other dependencies
/// </summary>
internal abstract class CoreModule() : IModule
{
    public abstract IModule? Owner { get; set; }

    public abstract string Name { get; }

    public abstract ConcurrentBag<IModule> Modules { get; }

    public abstract IModuleSettings? Settings { get; }

    public abstract string GetSettingsLocation();

    public abstract void Alive();
    public virtual void OnModuleActivation()
    {
        return;
    }
    public virtual void OnModuleDeactivation()
    {
        return;
    }
    public virtual void OnModuleDeclareConfig(SlashCommandBuilder rootConfig)
    {
        return;
    }
    public virtual async void OnConfigUpdated(SocketSlashCommand command)
    {
        await command.RespondAsync("No configs defined for this module", ephemeral: true); ;
    }
}
