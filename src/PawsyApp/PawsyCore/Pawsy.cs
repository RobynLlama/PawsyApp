using System.Collections.Concurrent;
using PawsyApp.PawsyCore.Modules;
using PawsyApp.PawsyCore.Modules.GuildSubmodules;
using PawsyApp.PawsyCore.Modules.Settings;
using PawsyApp.Utils;

namespace PawsyApp.PawsyCore;

internal class Pawsy : CoreModule, IModuleIdent
{
    public override IModuleSettings? Settings => null;
    public ulong ID { get => 0; set { return; } }
    public override string Name => "pawsy-core";
    public override string GetSettingsLocation() => "";

    public override void Alive()
    {
        WriteLog.LineNormal("Pawsy Core Activated");
    }
}
