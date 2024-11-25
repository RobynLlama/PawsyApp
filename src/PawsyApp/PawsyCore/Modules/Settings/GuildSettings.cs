using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PawsyApp.PawsyCore.Modules.Settings;

[method: JsonConstructor]
public class GuildSettings() : ISettings
{
    [JsonInclude]
    public List<string> EnabledModules { get; set; } = [];
}
