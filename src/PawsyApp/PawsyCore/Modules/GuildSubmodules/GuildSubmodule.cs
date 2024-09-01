using System.Collections.Concurrent;
using System.IO;
using Discord;
using Discord.WebSocket;
using PawsyApp.PawsyCore.Modules.Core;
using PawsyApp.PawsyCore.Modules.Settings;
using PawsyApp.Utils;

namespace PawsyApp.PawsyCore.Modules.GuildSubmodules;

/// <summary>
/// GuildSubmodules specifically rely on being added as a child
/// component of a GuildModule. They will not work otherwise
/// </summary>
internal abstract class GuildSubmodule() : CoreModule
{
    public override string GetSettingsLocation()
    {
        if (Owner is GuildModule guild)
        {
            return Path.Combine(Helpers.GetPersistPath(guild.ID), $"{Name}.json");
        }

        throw new System.SystemException($"GuildSubmodule {Name} is not a child of a GuildModule");
    }
}
