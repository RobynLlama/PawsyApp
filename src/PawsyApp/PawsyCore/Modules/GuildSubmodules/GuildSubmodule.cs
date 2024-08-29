using System.Collections.Generic;
using PawsyApp.PawsyCore.Modules.Settings;

namespace PawsyApp.PawsyCore.Modules.GuildSubmodules;

/// <summary>
/// GuildSubmodules specifically rely on being added as a child
/// component of a GuildModule. They will not work otherwise
/// </summary>
internal class GuildSubmodule() : IModule
{
    public IModule? Owner { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public string Name => throw new System.NotImplementedException();

    public List<IModule> Modules => throw new System.NotImplementedException();

    public IModuleSettings? Settings => throw new System.NotImplementedException();

    public void Activate()
    {
        throw new System.NotImplementedException();
    }

    public string GetSettingsLocation()
    {
        throw new System.NotImplementedException();
    }
}
