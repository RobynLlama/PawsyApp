using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace PawsyApp.PawsyCore;
internal delegate Task SlashCommandHandler(SocketSlashCommand command);
internal readonly struct SlashCommandBundle(SlashCommandHandler Handler, SlashCommandProperties BuiltCommand, string ModuleName)
{
    public SlashCommandHandler Handler { get; } = Handler;
    public SlashCommandProperties BuiltCommand { get; } = BuiltCommand;
    public string ModuleName { get; } = ModuleName;
}

