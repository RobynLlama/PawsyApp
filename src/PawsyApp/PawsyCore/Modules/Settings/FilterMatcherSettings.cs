using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace PawsyApp.PawsyCore.Modules.Settings;

internal class FilterMatcherSettings() : ISettings
{
    [JsonInclude]
    internal ConcurrentDictionary<long, RuleBundle> RuleList { get; set; } = [];
    [JsonInclude]
    internal ulong LoggingChannelID { get; set; } = 0;
    [JsonInclude]
    internal ulong NextRuleID { get; set; } = 0;
}
