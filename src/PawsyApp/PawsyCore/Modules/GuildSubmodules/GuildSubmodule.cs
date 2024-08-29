using System.Collections.Concurrent;
using System.IO;
using PawsyApp.PawsyCore.Modules.Core;
using PawsyApp.PawsyCore.Modules.Settings;
using PawsyApp.Utils;

namespace PawsyApp.PawsyCore.Modules.GuildSubmodules;

/// <summary>
/// GuildSubmodules specifically rely on being added as a child
/// component of a GuildModule. They will not work otherwise
/// </summary>
internal abstract class GuildSubmodule() : IModule
{
    public abstract IModule? Owner { get; set; }

    public abstract string Name { get; }

    public abstract ConcurrentBag<IModule> Modules { get; }

    public abstract IModuleSettings? Settings { get; }

    public abstract void Activate();
    public abstract void RegisterHooks();

    public string GetSettingsLocation()
    {
        if (Owner is GuildModule guild)
        {
            return Path.Combine(Helpers.GetPersistPath(guild.ID), $"{Name}.json");
        }

        return string.Empty;
    }
}
