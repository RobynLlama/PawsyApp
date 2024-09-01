using System.Text.Json.Serialization;

namespace PawsyApp.PawsyCore.Modules.Settings;

[method: JsonConstructor]
internal class LogMuncherSettings() : ISettings
{
    [JsonInclude]
    public ulong MunchingChannel { get; set; } = 0;
}
