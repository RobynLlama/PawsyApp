using System.Collections.Concurrent;
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

    public override void Activate()
    {
        _settings = (this as IModule).LoadSettings<MeowBoardSettings>();
    }
}
