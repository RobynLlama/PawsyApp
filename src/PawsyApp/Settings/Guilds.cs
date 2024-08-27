using PawsyApp.GuildStorage;
using System.Collections.Generic;

namespace PawsyApp.Settings;

public partial class AllSettings
{
    internal static Dictionary<ulong, GuildSettings> GuildSettingsStorage = [];
}
