using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PawsyApp.PawsyCore.Modules.Core;
using PawsyApp.PawsyCore.Modules.Settings;

namespace PawsyApp.PawsyCore.Modules.GuildSubmodules;

internal class MeowBoardModule : GuildSubmodule
{
    public override IModule? Owner { get => _owner; set => _owner = value; }
    public override string Name => "MeowBoard";
    public override ConcurrentBag<IModule> Modules => _modules;
    public override IModuleSettings? Settings => _settings;

    protected IModule? _owner;
    protected readonly ConcurrentBag<IModule> _modules = [];
    protected MeowBoardSettings? _settings;
    internal static Emote PawsySmall = new(1277935719805096066, "pawsysmall");

    public override void Activate()
    {
        _settings = (this as IModule).LoadSettings<MeowBoardSettings>();

        if (Owner is GuildModule guild)
        {
            guild.OnGuildMessage += MessageCallback;
        }
    }

    private Task MessageCallback(SocketUserMessage message, SocketGuildChannel channel)
    {
        if (message.CleanContent.Contains("meow"))
        {
            _settings?.AddUserMeow(message.Author.Id);
            message.AddReactionAsync(PawsySmall);
        }

        return Task.CompletedTask;
    }
}
