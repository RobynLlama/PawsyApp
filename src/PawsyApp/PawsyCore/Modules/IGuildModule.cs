using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace PawsyApp.PawsyCore.Modules;

public interface IGuildModule : ISettingsOwner, IActivatable
{
    string Name { get; }
    bool ModuleDeclaresConfig { get; }
    bool ModuleDeclaresCommands { get; }

    abstract void IActivatable.OnActivate();
    abstract void IActivatable.OnDeactivate();
    abstract void OnConfigDeclared(SlashCommandOptionBuilder rootConfig);
    abstract Task OnConfigUpdated(SocketSlashCommand command, SocketSlashCommandDataOption options);
    abstract SlashCommandBundle OnCommandsDeclared(SlashCommandBuilder rootCommand);
    abstract void Destroy();
}
