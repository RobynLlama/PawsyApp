using System.Text.Json.Serialization;
using PawsyApp.PawsyCore.Modules.Settings;

namespace MuncherModule.Settings;

[method: JsonConstructor]
internal class LogMuncherSettings() : ISettings
{
    [JsonInclude]
    public ulong MunchingChannel { get; set; } = 0;
}
