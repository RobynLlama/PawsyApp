using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace PawsyApp.PawsyCore;
public delegate Task SlashCommandHandler(SocketSlashCommand command);
public readonly struct SlashCommandBundle(SlashCommandHandler Handler, SlashCommandProperties BuiltCommand, string ModuleName)
{
    public SlashCommandHandler Handler { get; } = Handler;
    public SlashCommandProperties BuiltCommand { get; } = BuiltCommand;
    public string ModuleName { get; } = ModuleName;
}

