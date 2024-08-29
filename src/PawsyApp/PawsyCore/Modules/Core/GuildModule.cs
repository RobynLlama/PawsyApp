using System.IO;
using PawsyApp.PawsyCore.Modules.Settings;
using PawsyApp.Utils;
using PawsyApp.PawsyCore.Modules.GuildSubmodules;
using System.Collections.Concurrent;

namespace PawsyApp.PawsyCore.Modules.Core;

internal class GuildModule() : IModuleIdent
{
    public IModule? Owner { get => _owner; set => _owner = value; }
    public ConcurrentBag<IModule> Modules => _modules;
    public string Name { get => "GuildGlobal"; }
    public ulong ID { get => _id; set => _id = value; }
    IModuleSettings? IModule.Settings => Settings;
    public string GetSettingsLocation() =>
    Path.Combine(Helpers.GetPersistPath(ID), $"{Name}.json");

    protected ulong _id;
    protected IModule? _owner;
    protected readonly ConcurrentBag<IModule> _modules = [];
    protected GuildSettings? Settings;

    public void Activate()
    {
        Settings = (this as IModule).LoadSettings<GuildSettings>();

        //Decide if we should activate modules here
        (this as IModule).AddModule<MeowBoardModule>();
        (this as IModule).AddModule<FilterMatcherModule>();
    }

}
