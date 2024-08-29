using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace PawsyApp.PawsyCore.Modules.Settings;

[method: JsonConstructor]
internal class GuildSettings() : IModuleSettings
{
    public string Location { get => _location; set => _location = value; }
    public IModule? Owner { get => _owner; set => _owner = value; }

    [JsonInclude]
    public readonly ConcurrentDictionary<string, bool> EnabledModules = [];

    protected string _location = string.Empty;
    protected IModule? _owner;
}
