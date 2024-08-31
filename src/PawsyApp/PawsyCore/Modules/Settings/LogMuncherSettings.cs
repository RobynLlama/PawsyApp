using System.Text.Json.Serialization;

namespace PawsyApp.PawsyCore.Modules.Settings;

[method: JsonConstructor]
internal class LogMuncherSettings() : IModuleSettings
{
    [JsonIgnore]
    public string Location { get => _location; set => _location = value; }
    [JsonIgnore]
    public IModule? Owner { get => _owner; set => _owner = value; }

    [JsonInclude]
    public ulong MunchingChannel { get; set; } = 0;

    [JsonIgnore]
    protected string _location = string.Empty;
    [JsonIgnore]
    protected IModule? _owner;
}
