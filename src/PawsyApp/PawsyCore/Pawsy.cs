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
    public string Name => "pawsy-core";

    private readonly ConcurrentBag<IModule> _modules = [];
    private IModule? _owner;
    void IModule.Alive()
    {
        WriteLog.LineNormal("Pawsy Core Activated");
    }

    void IModule.OnModuleActivation() { return; }
    void IModule.OnModuleDeactivation() { return; }
    void IModule.OnModuleDeclareConfig(Discord.SlashCommandBuilder rootConfig) { return; }
    void IModule.OnConfigUpdated(Discord.WebSocket.SocketSlashCommand command) { return; }

    public string GetSettingsLocation() => "";
}
