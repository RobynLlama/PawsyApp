using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace PawsyApp.PawsyCore.Modules.GuildModules;

internal abstract class GuildModule(Guild Owner, string name, bool declaresConfig = false, bool declaresCommands = false) : IGuildModule
{
    public WeakReference<Guild> Owner { get; } = new(Owner);
    public string Name { get => name; }
    public string ID => Name;
    public bool ModuleDeclaresConfig { get => declaresConfig; }
    public bool ModuleDeclaresCommands { get => declaresCommands; }

    public string GetSettingsLocation()
    {
        if (Owner.TryGetTarget(out var owner))
        {
            return Path.Combine(Guild.GetPersistPath(owner.ID), $"{Name}.json");
        }

        throw new Exception($"GuildSubmodule {Name} has no owner");
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
        throw new Exception("OnModuleDeclareCommands called without a matching body in module, panic!");
    }
    public virtual Task OnConfigUpdated(SocketSlashCommand command, SocketSlashCommandDataOption options)
    {
        return command.RespondAsync("This module is not configurable or unavailable", ephemeral: true); ;
    }

    public Task LogAppendContext(object message, (object ContextName, object ContextValue)[] context)
    {
        if (Owner.TryGetTarget(out var owner))
            return owner.LogAppendContext(Name, message, context);

        throw new Exception("Guild does not have an owner in LogAppendContext");
    }
    public Task LogAppendLine(object message)
    {
        if (Owner.TryGetTarget(out var owner))
            return owner.LogAppendLine(Name, message);

        throw new Exception("Guild does not have an owner in LogAppendLine");
    }
}
