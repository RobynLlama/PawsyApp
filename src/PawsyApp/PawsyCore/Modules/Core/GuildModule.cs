using System.Collections.Generic;
using System.IO;
using PawsyApp.PawsyCore.Modules.Settings;
using PawsyApp.Utils;

namespace PawsyApp.PawsyCore.Modules.Core;

internal class GuildModule() : IModuleIdent
{
    public IModule? Owner { get => _owner; set => _owner = value; }
    public List<IModule> Modules => _modules;
    public string Name { get => "GuildModule"; }
    public ulong ID { get => _id; set => _id = value; }
    IModuleSettings? IModule.Settings => Settings;

    public string GetSettingsLocation() =>
    Path.Combine(Helpers.GetPersistPath(ID), $"{Name}.json");

    protected ulong _id;
    protected IModule? _owner;
    protected readonly List<IModule> _modules = [];
    protected GuildSettings? Settings;

    public void Activate()
    {
        Settings = (this as IModule).LoadSettings<GuildSettings>();
    }

}
