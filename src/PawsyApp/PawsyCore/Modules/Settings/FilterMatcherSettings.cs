using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace PawsyApp.PawsyCore.Modules.Settings;

internal class FilterMatcherSettings() : IModuleSettings
{
    [JsonIgnore]
    public string Location { get => _location; set => _location = value; }
    [JsonIgnore]
    public IModule? Owner { get => _owner; set => _owner = value; }

    [JsonInclude]
    internal ConcurrentDictionary<ulong, RuleBundle> RuleList { get; set; } = [];
    [JsonInclude]
    internal ulong LoggingChannelID { get; set; } = 0;
    [JsonInclude]
    internal ulong NextRuleID { get; set; } = 0;

    [JsonIgnore]
    protected string _location = string.Empty;
    [JsonIgnore]
    protected IModule? _owner;
}
