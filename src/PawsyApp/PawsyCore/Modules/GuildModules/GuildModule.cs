using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PawsyApp.Utils;

namespace PawsyApp.PawsyCore.Modules.GuildModules;

internal abstract class GuildModule(Guild Owner, string name, bool declaresConfig = false, bool declaresCommands = false) : IGuildModule
{
    public Guild Owner { get; } = Owner;
    public string Name { get => name; }
    public bool ModuleDeclaresConfig { get => declaresConfig; }
    public bool ModuleDeclaresCommands { get => declaresCommands; }

    public string GetSettingsLocation()
    {
        if (Owner is not null)
        {
            return Path.Combine(Helpers.GetPersistPath(Owner.ID), $"{Name}.json");
        }

        throw new System.SystemException($"GuildSubmodule {Name} has no owner");
    }

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
