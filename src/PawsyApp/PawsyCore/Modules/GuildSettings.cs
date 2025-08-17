using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PawsyApp.PawsyCore.Modules;

[method: JsonConstructor]
public class GuildSettings() : ISettings
{
    [JsonInclude]
    public List<string> EnabledModules { get; set; } = [];

    [JsonInclude]
    public string GuildName { get; set; } = string.Empty;

    [JsonInclude]
    public ulong OwnerSnowflake { get; set; } = 0u;

    [JsonInclude]
    public bool Parted { get; set; } = false;
}
