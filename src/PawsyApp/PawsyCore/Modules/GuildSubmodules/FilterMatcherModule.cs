using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord.WebSocket;
using PawsyApp.PawsyCore.Modules.Core;
using PawsyApp.PawsyCore.Modules.Settings;
using PawsyApp.Utils;

namespace PawsyApp.PawsyCore.Modules.GuildSubmodules;

internal class FilterMatcherModule : GuildSubmodule
{
    public override IModule? Owner { get => _owner; set => _owner = value; }
    public override string Name => "FilterMatcher";
    public override ConcurrentBag<IModule> Modules => _modules;
    public override IModuleSettings? Settings => _settings;

    protected IModule? _owner;
    protected readonly ConcurrentBag<IModule> _modules = [];
    protected FilterMatcherSettings? _settings;

    public override void Activate()
    {
        _settings = (this as IModule).LoadSettings<FilterMatcherSettings>();
        WriteLog.Cutely("Filters loaded", [
            ("Filter Count", _settings.RuleList.Count.ToString())
        ]);

        if (Owner is GuildModule guild)
        {
            guild.OnGuildMessage += MessageCallBack;
        }
    }

    private async Task MessageCallBack(SocketUserMessage message, SocketGuildChannel channel)
    {
        await WriteLog.Normally("FilterMatcher Call Back!");
    }
}
