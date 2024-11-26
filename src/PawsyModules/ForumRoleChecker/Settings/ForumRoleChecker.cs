using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using PawsyApp.PawsyCore.Modules;

namespace ForumRoleChecker.Settings;

[method: JsonConstructor]
public class ForumRoleCheckerSettings() : ISettings
{
    [JsonInclude]
    public ConcurrentDictionary<ulong, ulong> WatchList = [];
    public ulong AlertChannel { get; set; } = 0;
}
