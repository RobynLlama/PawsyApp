using Discord;
using Discord.WebSocket;
using PawsyApp.PawsyCore.Modules.Settings;
using System.Threading.Tasks;

namespace PawsyApp.PawsyCore.Modules;

internal interface IGuildModule : ISettingsOwner, IUnique<string>
{
    Guild Owner { get; }
    string Name { get; }
    bool ModuleDeclaresConfig { get; }
    bool ModuleDeclaresCommands { get; }

    abstract void OnModuleActivation();
    abstract void OnModuleDeactivation();
    abstract void OnModuleDeclareConfig(SlashCommandOptionBuilder rootConfig);
    abstract Task OnConfigUpdated(SocketSlashCommand command, SocketSlashCommandDataOption options);
    abstract SlashCommandBundle OnModuleDeclareCommands(SlashCommandBuilder rootCommand);
}
