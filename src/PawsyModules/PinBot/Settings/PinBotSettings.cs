using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using PawsyApp.PawsyCore.Modules;

namespace PinBot.Settings;

public class PinBotSettings() : ISettings
{
    [JsonInclude]
    internal ConcurrentDictionary<ulong, bool> RolesWithPerms = [];

    internal ConcurrentDictionary<ulong, bool> AutoPinChannels = [];
}
