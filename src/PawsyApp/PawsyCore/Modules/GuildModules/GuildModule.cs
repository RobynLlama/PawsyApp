using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace PawsyApp.PawsyCore.Modules.GuildModules;

internal abstract class GuildModule(Guild Owner, string name, bool declaresConfig = false, bool declaresCommands = false) : IGuildModule
{
    public Guild Owner { get; } = Owner;
    public string Name { get => name; }
    public string ID => Name;
    public bool ModuleDeclaresConfig { get => declaresConfig; }
    public bool ModuleDeclaresCommands { get => declaresCommands; }

    public string GetSettingsLocation()
    {
        if (Owner is not null)
        {
            return Path.Combine(Guild.GetPersistPath(Owner.ID), $"{Name}.json");
        }

        throw new System.SystemException($"GuildSubmodule {Name} has no owner");
    }

    public virtual void OnActivate()
    {
        return;
    }
    public virtual void OnDeactivate()
    {
        return;
    }
    public virtual void OnConfigDeclared(SlashCommandOptionBuilder rootConfig)
    {
        return;
    }
    public virtual SlashCommandBundle OnCommandsDeclared(SlashCommandBuilder builder)
    {
        throw new System.Exception("OnModuleDeclareCommands called without a matching body in module, panic!");
    }
    public virtual Task OnConfigUpdated(SocketSlashCommand command, SocketSlashCommandDataOption options)
    {
        return command.RespondAsync("This module is not configurable or unavailable", ephemeral: true); ;
    }

    public Task LogAppendContext(object message, (object ContextName, object ContextValue)[] context) => Owner.LogAppendContext(Name, message, context);
    public Task LogAppendLine(object message) => Owner.LogAppendLine(Name, message);
}
