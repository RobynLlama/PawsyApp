using System.Collections.Concurrent;
using PawsyApp.PawsyCore.Modules;
using PawsyApp.PawsyCore.Modules.Settings;
using PawsyApp.Utils;

namespace PawsyApp.PawsyCore;

public class Pawsy : IModuleIdent
{
    IModule? IModule.Owner { get => _owner; set => _owner = value; }
    ConcurrentBag<IModule> IModule.Modules => _modules;
    IModuleSettings? IModule.Settings => null;
    public ulong ID { get => 0; set { return; } }
    public string Name => "PawsyCore";

    private readonly ConcurrentBag<IModule> _modules = [];
    private IModule? _owner;
    void IModule.Activate()
    {
        WriteLog.Normally("Pawsy Core Activated");
    }

    void IModule.RegisterHooks() { return; }

    public string GetSettingsLocation() => "";
}
